using DG.Tweening;
using UnityEngine;

public static class SkillMoveMethods
{
    public static void SkillMove(this SkillInstance instance, SkillHelper.MoveLogic moveLogic, Transform self, Transform target, out bool isPathMove)
    {
        isPathMove = false;
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.None:
                break;
            case SkillHelper.MoveLogic.MoveAndScale:
                // isPathMove  = true;
                // UpAndFindToTarget(instance, self, target);
                var end = new Vector3(target.position.x, instance.transform.position.y, target.position.z);
                instance.transform.forward = end - instance.transform.position;
                MoveAndScale(instance);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnToTarget:
                FollowSelfAndTurnToTarget(instance, self, target);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                FollowSelfAndTurnAround(instance, self);
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                isPathMove  = true;
                UpAndFindToTarget(instance, self, target);
                break;
            case SkillHelper.MoveLogic.DurationSweep:
                DurationSweep(instance, self);
                // instance.transform.localScale = Vector3.one * 20;
                break;
            case SkillHelper.MoveLogic.OnlyEffect:
                break;
            case SkillHelper.MoveLogic.ParabolaAndRebound:
                ParabolaAndRebound(instance);
                break;
            case SkillHelper.MoveLogic.MoveForward:
                MoveForward(instance);
                break;
        }
    }
    
    public static void SkillMove(this SkillInstance instance, SkillHelper.MoveLogic moveLogic, Transform self, System.Func<Transform> getTargetFunc, out bool isPathMove)
    {
        isPathMove = false;
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.None:
                break;
            case SkillHelper.MoveLogic.MoveAndScale:
                // UpAndFindToTarget(instance, self, getTargetFunc());
                MoveAndScale(instance);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnToTarget:
                FollowSelfAndTurnToTarget(instance, self, getTargetFunc());
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                FollowSelfAndTurnAround(instance, self);
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                UpAndFindToTarget(instance, self, getTargetFunc());
                break;
            case SkillHelper.MoveLogic.DurationSweep:
                DurationSweep(instance, self);
                break;
            case SkillHelper.MoveLogic.OnlyEffect:
                break;
            case SkillHelper.MoveLogic.ParabolaAndRebound:
                ParabolaAndRebound(instance);
                break;
            case SkillHelper.MoveLogic.MoveForward:
                MoveForward(instance);
                break;
        }
    }

    private static void ParabolaAndRebound(SkillInstance instance)
    {
        var transform = instance.transform;
        var moveContext = instance.MoveContext;
        if (moveContext == null) return;

        // 初始化阶段（仅首次调用）
        if (!moveContext.IsGrenadeInitialized)
        {
            InitializeGrenade(instance, moveContext);
            return;
        }

        // 根据运动阶段执行不同逻辑
        switch (moveContext.GrenadePhase)
        {
            case 0: // 飞行阶段
            case 1: // 反弹中
                UpdateGrenadeFlying(transform, moveContext);
                break;
            case 2: // 滚动阶段
                UpdateGrenadeRolling(transform, moveContext);
                break;
            case 3: // 已停止
                break;
        }
    }

    /// <summary>
    /// 初始化手榴弹运动参数
    /// 根据施法者朝向和移动速度计算初始速度
    /// </summary>
    private static void InitializeGrenade(SkillInstance instance, SkillMoveContext moveContext)
    {
        var transform = instance.transform;
        float speed = instance.SkillExeContext.GetMoveSpeed();
        
        // 计算水平方向（使用当前朝向）
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        
        // 设置初始水平速度（基于移动速度和朝向）
        moveContext.HorizontalVelocity = forward * speed;
        
        // 设置初始垂直速度（向上抛出）
        moveContext.VerticalVelocity = SkillConstants.GRENADE_INITIAL_VERTICAL_SPEED;
        
        // 初始化其他参数
        moveContext.GrenadePhase = 0; // 飞行阶段
        moveContext.BounceCount = 0;
        moveContext.IsGrenadeInitialized = true;
        
        // 记录起始位置
        moveContext.StartPosition = transform.position;
    }

    /// <summary>
    /// 更新手榴弹飞行阶段
    /// 应用重力、更新位置、检测地面碰撞、处理反弹
    /// </summary>
    private static void UpdateGrenadeFlying(Transform transform, SkillMoveContext moveContext)
    {
        float deltaTime = Time.deltaTime;
        
        // 应用重力
        moveContext.VerticalVelocity -= SkillConstants.GRENADE_GRAVITY * deltaTime;
        
        // 更新位置
        Vector3 position = transform.position;
        position += moveContext.HorizontalVelocity * deltaTime;
        position.y += moveContext.VerticalVelocity * deltaTime;
        
        // 地面碰撞检测
        if (position.y <= SkillConstants.GRENADE_GROUND_Y)
        {
            // 修正位置到地面
            position.y = SkillConstants.GRENADE_GROUND_Y;
            
            // 反弹处理
            moveContext.BounceCount++;
            moveContext.GrenadePhase = 1; // 反弹中
            
            // 反转并衰减垂直速度
            moveContext.VerticalVelocity = -moveContext.VerticalVelocity * SkillConstants.GRENADE_BOUNCE_DAMPING;
            
            // 衰减水平速度
            moveContext.HorizontalVelocity *= SkillConstants.GRENADE_HORIZONTAL_DAMPING;
            
            // 检查是否应该进入滚动阶段（垂直速度过小）
            if (Mathf.Abs(moveContext.VerticalVelocity) < SkillConstants.GRENADE_MIN_BOUNCE_VELOCITY)
            {
                moveContext.GrenadePhase = 2; // 滚动阶段
                moveContext.VerticalVelocity = 0;
                moveContext.HasLanded = true;
            }
        }
        
        transform.position = position;
        
        // 更新朝向（朝向速度方向）
        Vector3 velocity = moveContext.HorizontalVelocity + Vector3.up * moveContext.VerticalVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }
    }

    /// <summary>
    /// 更新手榴弹滚动阶段
    /// 应用摩擦力，逐渐减速直到停止
    /// </summary>
    private static void UpdateGrenadeRolling(Transform transform, SkillMoveContext moveContext)
    {
        float deltaTime = Time.deltaTime;
        
        // 应用摩擦力衰减水平速度
        float friction = 1f - SkillConstants.GRENADE_ROLL_FRICTION * deltaTime;
        friction = Mathf.Max(0f, friction); // 确保不会变成负数
        moveContext.HorizontalVelocity *= friction;
        
        // 更新位置
        Vector3 position = transform.position;
        position += moveContext.HorizontalVelocity * deltaTime;
        position.y = SkillConstants.GRENADE_GROUND_Y; // 保持在地面
        transform.position = position;
        
        // 检查是否应该停止
        if (moveContext.HorizontalVelocity.sqrMagnitude < SkillConstants.GRENADE_MIN_ROLL_SPEED * SkillConstants.GRENADE_MIN_ROLL_SPEED)
        {
            moveContext.GrenadePhase = 3; // 停止
            moveContext.HorizontalVelocity = Vector3.zero;
        }
    }

    private static void DurationSweep(SkillInstance instance, Transform self)
    {
        var transform = instance.transform;
        transform.position = self.position;

        // 计算每帧应旋转的角度
        float rotationSpeed = instance.SkillExeContext.GetMoveSpeed() * Time.deltaTime;
        float currentAngle = transform.rotation.eulerAngles.y; // 获取当前的旋转角度

        // 获取移动上下文（如果可用）
        var moveContext = instance.MoveContext;
        float maxSweepAngle = moveContext?.SweepMaxAngle ?? SkillConstants.DEFAULT_SWEEP_MAX_ANGLE;

        // 应用旋转 - 使用 MoveContext 或回退到旧的 MoveParam1
        bool sweepPositive = moveContext?.SweepDirectionPositive ?? instance.MoveParam1;
        
        if (sweepPositive)
        {
            transform.Rotate(Vector3.up, rotationSpeed);
            if (currentAngle >= maxSweepAngle)
            {
                // 使用新的语义化属性
                if (moveContext != null)
                    moveContext.SweepDirectionPositive = false;
                else
                    instance.MoveParam1 = false;
            }
        }
        else
        {
            transform.Rotate(Vector3.up, -rotationSpeed);
            if (currentAngle <= -maxSweepAngle)
            {
                // 使用新的语义化属性
                if (moveContext != null)
                    moveContext.SweepDirectionPositive = true;
                else
                    instance.MoveParam1 = true;
            }
        }
        
        // 更新扫射角度记录
        if (moveContext != null)
        {
            moveContext.SweepAngle = currentAngle;
        }
    }

    private static void UpAndFindToTarget(SkillInstance instance, Transform self, Transform target)
    {
        if(target == null) return;
        var transform = instance.transform;
        var moveContext = instance.MoveContext;
        
        // ========== 第二阶段：上升完成后，每帧追踪目标（跟踪弹效果） ==========
        if (moveContext != null && moveContext.IsRiseComplete)
        {
            RocketTrackTarget(instance, transform, target);
            return;
        }
        
        // ========== 第一阶段：上升（仅在 Init 时调用一次） ==========
        
        // 使用 MoveContext 缓存随机值，避免每帧重新生成
        float randomHeight;
        float randomHorizontalOffset;
        
        if (moveContext != null && moveContext.RandomHeight == 0)
        {
            // 首次调用，生成随机值并缓存
            randomHeight = Random.Range(SkillConstants.ROCKET_MIN_HEIGHT, SkillConstants.ROCKET_MAX_HEIGHT);
            randomHorizontalOffset = Random.Range(SkillConstants.ROCKET_MIN_HORIZONTAL_OFFSET, SkillConstants.ROCKET_MAX_HORIZONTAL_OFFSET);
            moveContext.RandomHeight = randomHeight;
            moveContext.RandomHorizontalOffset = randomHorizontalOffset;
        }
        else if (moveContext != null)
        {
            // 使用缓存的随机值
            randomHeight = moveContext.RandomHeight;
            randomHorizontalOffset = moveContext.RandomHorizontalOffset;
        }
        else
        {
            // 回退到旧行为
            randomHeight = Random.Range(SkillConstants.ROCKET_MIN_HEIGHT, SkillConstants.ROCKET_MAX_HEIGHT);
            randomHorizontalOffset = Random.Range(SkillConstants.ROCKET_MIN_HORIZONTAL_OFFSET, SkillConstants.ROCKET_MAX_HORIZONTAL_OFFSET);
        }
        
        // 使用对象的本地坐标系来计算水平偏移
        Vector3 randomTargetPosition =
            transform.position + self.right * randomHorizontalOffset + self.up * randomHeight;
        
        transform.forward = randomTargetPosition - transform.position;
        
        // 记录起始位置
        if (moveContext != null)
        {
            moveContext.StartPosition = transform.position;
            moveContext.EndPosition = target.position;
        }
        
        // 向上移动（DOTween 驱动，完成后切换为每帧追踪）
        transform.DOMove(randomTargetPosition, SkillConstants.ROCKET_RISE_DURATION).SetEase(Ease.InSine).OnComplete(() =>
        {
            // 标记上升阶段完成，后续帧将由 UpDateSkill 切换为每帧追踪模式
            if (moveContext != null)
            {
                moveContext.IsRiseComplete = true;
            }
            
            // 初始朝向目标，为追踪阶段提供一个合理的初始方向
            if (target != null)
            {
                transform.LookAt(target);
            }
        });
    }

    /// <summary>
    /// 火箭弹追踪阶段：每帧平滑转向目标并向前移动（跟踪弹效果）
    /// 目标位置实时更新，火箭弹会持续追踪移动中的目标
    /// </summary>
    private static void RocketTrackTarget(SkillInstance instance, Transform transform, Transform target)
    {
        Vector3 direction = target.position - transform.position;
        
        if (direction.sqrMagnitude > 0.001f)
        {
            // 平滑转向：限制每帧最大转向角度，形成自然的弧线追踪轨迹
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                SkillConstants.ROCKET_TRACKING_TURN_SPEED * Time.deltaTime);
        }
        
        // 沿当前朝向前进
        float speed = instance.SkillExeContext.GetMoveSpeed() * SkillConstants.ROCKET_TRACKING_SPEED_MULTIPLIER;
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private static void FollowSelfAndTurnAround(SkillInstance instance, Transform self)
    {
        var transform = instance.transform;
        transform.position = self.position;
        
        // 计算每帧应旋转的角度
        float rotationThisFrame = instance.SkillExeContext.GetMoveSpeed() * Time.deltaTime;
        
        // 应用旋转
        transform.Rotate(Vector3.up, rotationThisFrame);
        
        // 更新移动上下文中的环绕角度
        var moveContext = instance.MoveContext;
        if (moveContext != null)
        {
            moveContext.OrbitAngle += rotationThisFrame;
            // 保持角度在 0-360 范围内
            if (moveContext.OrbitAngle >= SkillConstants.FULL_ROTATION_ANGLE)
                moveContext.OrbitAngle -= SkillConstants.FULL_ROTATION_ANGLE;
        }
    }

    private static void FollowSelfAndTurnToTarget(SkillInstance instance, Transform self, Transform target)
    {
        if(target == null) return;
        var transform = instance.transform;
        transform.position = self.position;
        // 计算朝向目标的方向
        Vector3 directionToTarget = target.position - transform.position;
        // 计算朝向目标的四元数
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        // 使用Slerp进行平滑旋转
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
            instance.SkillExeContext.GetMoveSpeed() * Time.deltaTime);
    }

    private static void MoveAndScale(SkillInstance instance)
    {
        var transform = instance.transform;
        transform.position += transform.forward.normalized * Time.deltaTime * instance.SkillExeContext.GetMoveSpeed();
        transform.localScale += Vector3.one * instance.SkillExeContext.GetMoveSpeed() * .3f * Time.deltaTime;
    }

    private static void MoveForward(SkillInstance instance)
    {
        var transform = instance.transform;
        transform.position += transform.forward.normalized * Time.deltaTime * instance.SkillExeContext.GetMoveSpeed();
    }


    #region 曲线计算相关
    private static Vector3 CalculateCubicBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; //first term
        p += 3 * uu * t * p1; //second term
        p += 3 * u * tt * p2; //third term
        p += ttt * p3; //fourth term

        return p;
    }
    #endregion
}
