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
                isPathMove = true;
                UpAndFindToTarget(instance, self, target);
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

    public static void SkillMove(this SkillInstance instance, SkillHelper.MoveLogic moveLogic, Transform self, System.Func<Transform> getTargetFunc, out bool isPathMove)
    {
        isPathMove = false;
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.None:
                break;
            case SkillHelper.MoveLogic.MoveAndScale:
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

    #region ParabolaAndRebound - 手榴弹反弹

    private static void ParabolaAndRebound(SkillInstance instance)
    {
        var transform = instance.transform;
        var moveContext = instance.MoveContext;
        if (moveContext?.Grenade == null) return;

        var grenade = moveContext.Grenade.Value;

        // 初始化阶段（仅首次调用）
        if (!grenade.IsInitialized)
        {
            InitializeGrenade(instance, moveContext);
            return;
        }

        // 根据运动阶段执行不同逻辑
        switch (grenade.Phase)
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

        var grenade = moveContext.Grenade ?? GrenadeData.Default;
        // 设置初始水平速度（基于移动速度和朝向）
        grenade.HorizontalVelocity = forward * speed;
        // 设置初始垂直速度（向上抛出）
        grenade.VerticalVelocity = SkillConstants.GRENADE_INITIAL_VERTICAL_SPEED;
        // 初始化其他参数
        grenade.Phase = 0; // 飞行阶段
        grenade.BounceCount = 0;
        grenade.IsInitialized = true;
        moveContext.Grenade = grenade;

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
        var grenade = moveContext.Grenade.Value;

        // 应用重力
        grenade.VerticalVelocity -= SkillConstants.GRENADE_GRAVITY * deltaTime;

        // 更新位置
        Vector3 position = transform.position;
        position += grenade.HorizontalVelocity * deltaTime;
        position.y += grenade.VerticalVelocity * deltaTime;

        // 地面碰撞检测
        if (position.y <= SkillConstants.GRENADE_GROUND_Y)
        {
            // 修正位置到地面
            position.y = SkillConstants.GRENADE_GROUND_Y;

            // 反弹处理
            grenade.BounceCount++;
            grenade.Phase = 1; // 反弹中

            // 反转并衰减垂直速度
            grenade.VerticalVelocity = -grenade.VerticalVelocity * SkillConstants.GRENADE_BOUNCE_DAMPING;
            // 衰减水平速度
            grenade.HorizontalVelocity *= SkillConstants.GRENADE_HORIZONTAL_DAMPING;

            // 检查是否应该进入滚动阶段（垂直速度过小）
            if (Mathf.Abs(grenade.VerticalVelocity) < SkillConstants.GRENADE_MIN_BOUNCE_VELOCITY)
            {
                grenade.Phase = 2; // 滚动阶段
                grenade.VerticalVelocity = 0;
                grenade.HasLanded = true;
            }
        }

        moveContext.Grenade = grenade;
        transform.position = position;

        // 更新朝向（朝向速度方向）
        Vector3 velocity = grenade.HorizontalVelocity + Vector3.up * grenade.VerticalVelocity;
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
        var grenade = moveContext.Grenade.Value;

        // 应用摩擦力衰减水平速度
        float friction = 1f - SkillConstants.GRENADE_ROLL_FRICTION * deltaTime;
        friction = Mathf.Max(0f, friction); // 确保不会变成负数
        grenade.HorizontalVelocity *= friction;

        // 更新位置
        Vector3 position = transform.position;
        position += grenade.HorizontalVelocity * deltaTime;
        position.y = SkillConstants.GRENADE_GROUND_Y; // 保持在地面
        transform.position = position;

        // 检查是否应该停止
        if (grenade.HorizontalVelocity.sqrMagnitude < SkillConstants.GRENADE_MIN_ROLL_SPEED * SkillConstants.GRENADE_MIN_ROLL_SPEED)
        {
            grenade.Phase = 3; // 停止
            grenade.HorizontalVelocity = Vector3.zero;
        }

        moveContext.Grenade = grenade;
    }

    #endregion

    #region DurationSweep - 扫射

    private static void DurationSweep(SkillInstance instance, Transform self)
    {
        var transform = instance.transform;
        transform.position = self.position;

        // 计算每帧应旋转的角度
        float rotationSpeed = instance.SkillExeContext.GetMoveSpeed() * Time.deltaTime;
        float currentAngle = transform.rotation.eulerAngles.y; // 获取当前的旋转角度

        var moveContext = instance.MoveContext;
        if (moveContext?.Sweep == null) return;

        var sweep = moveContext.Sweep.Value;
        float maxSweepAngle = sweep.MaxAngle;

        // 应用旋转
        if (sweep.DirectionPositive)
        {
            transform.Rotate(Vector3.up, rotationSpeed);
            if (currentAngle >= maxSweepAngle)
            {
                sweep.DirectionPositive = false;
            }
        }
        else
        {
            transform.Rotate(Vector3.up, -rotationSpeed);
            if (currentAngle <= -maxSweepAngle)
            {
                sweep.DirectionPositive = true;
            }
        }

        // 更新扫射角度记录
        sweep.Angle = currentAngle;
        moveContext.Sweep = sweep;
    }

    #endregion

    #region UpAndFindToTarget - 火箭弹道

    private static void UpAndFindToTarget(SkillInstance instance, Transform self, Transform target)
    {
        if (target == null) return;
        var transform = instance.transform;
        var moveContext = instance.MoveContext;

        // ========== 第二阶段：上升完成后，每帧追踪目标（跟踪弹效果） ==========
        if (moveContext?.Rocket != null && moveContext.Rocket.Value.IsRiseComplete)
        {
            RocketTrackTarget(instance, transform, target);
            return;
        }

        // ========== 第一阶段：上升（仅在 Init 时调用一次） ==========
        if (moveContext == null) return;

        var rocket = moveContext.Rocket ?? RocketData.Default;
        float randomHeight;
        float randomHorizontalOffset;

        // 使用 MoveContext 缓存随机值，避免每帧重新生成
        if (rocket.RandomHeight == 0)
        {
            // 首次调用，生成随机值并缓存
            randomHeight = Random.Range(SkillConstants.ROCKET_MIN_HEIGHT, SkillConstants.ROCKET_MAX_HEIGHT);
            randomHorizontalOffset = Random.Range(SkillConstants.ROCKET_MIN_HORIZONTAL_OFFSET, SkillConstants.ROCKET_MAX_HORIZONTAL_OFFSET);
            rocket.RandomHeight = randomHeight;
            rocket.RandomHorizontalOffset = randomHorizontalOffset;
            moveContext.Rocket = rocket;
        }
        else
        {
            // 使用缓存的随机值
            randomHeight = rocket.RandomHeight;
            randomHorizontalOffset = rocket.RandomHorizontalOffset;
        }

        // 使用对象的本地坐标系来计算水平偏移
        Vector3 randomTargetPosition =
            transform.position + self.right * randomHorizontalOffset + self.up * randomHeight;

        transform.forward = randomTargetPosition - transform.position;

        // 记录起始位置
        moveContext.StartPosition = transform.position;
        moveContext.EndPosition = target.position;

        // 向上移动（DOTween 驱动，完成后切换为每帧追踪）
        transform.DOMove(randomTargetPosition, SkillConstants.ROCKET_RISE_DURATION).SetEase(Ease.InSine).OnComplete(() =>
        {
            // 标记上升阶段完成，后续帧将由 UpDateSkill 切换为每帧追踪模式
            var r = moveContext.Rocket ?? RocketData.Default;
            r.IsRiseComplete = true;
            moveContext.Rocket = r;

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

    #endregion

    #region FollowSelfAndTurnAround - 环绕

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
        if (moveContext?.Orbit != null)
        {
            var orbit = moveContext.Orbit.Value;
            orbit.Angle += rotationThisFrame;
            // 保持角度在 0-360 范围内
            if (orbit.Angle >= SkillConstants.FULL_ROTATION_ANGLE)
                orbit.Angle -= SkillConstants.FULL_ROTATION_ANGLE;
            moveContext.Orbit = orbit;
        }
    }

    #endregion

    #region 其他移动方法

    private static void FollowSelfAndTurnToTarget(SkillInstance instance, Transform self, Transform target)
    {
        if (target == null) return;
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

    #endregion

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
