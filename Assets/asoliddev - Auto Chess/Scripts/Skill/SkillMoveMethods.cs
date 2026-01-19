using DG.Tweening;
using UnityEngine;

public static class SkillMoveMethods
{
    public static bool SkillMove(this SkillInstance instance, SkillHelper.MoveLogic moveLogic, Transform self, Transform target, out bool isPathMove)
    {
        isPathMove = false;
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.None:
                break;
            case SkillHelper.MoveLogic.MoveAndScale:
                isPathMove  = true;
                UpAndFindToTarget(instance, self, target);
                
                // MoveAndScale(instance);
                return true;
            case SkillHelper.MoveLogic.FollowSelfAndTurnToTarget:
                FollowSelfAndTurnToTarget(instance, self, target);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                FollowSelfAndTurnAround(instance, self);
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                isPathMove  = true;
                UpAndFindToTarget(instance, self, target);
                return true;
            case SkillHelper.MoveLogic.DurationSweep:
                DurationSweep(instance, self);
                // instance.transform.localScale = Vector3.one * 20;
                return true;
                break;
            case SkillHelper.MoveLogic.OnlyEffect:
                break;
            case SkillHelper.MoveLogic.ParabolaAndRebound:
                ParabolaAndRebound(instance);
                return true;
            case SkillHelper.MoveLogic.MoveForward:
                MoveForward(instance);
                return true;
        }

        return false;
    }
    
    public static void SkillMove(this SkillInstance instance, SkillHelper.MoveLogic moveLogic, Transform self, System.Func<Transform> getTargetFunc, out bool isPathMove)
    {
        isPathMove = false;
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.None:
                break;
            case SkillHelper.MoveLogic.MoveAndScale:
                UpAndFindToTarget(instance, self, getTargetFunc());
                
                // MoveAndScale(instance);
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
        Debug.LogError("ParabolaAndRebound");
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
        var transform = instance.transform;
        var moveContext = instance.MoveContext;
        
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
        
        // 向上移动
        transform.DOMove(randomTargetPosition, SkillConstants.ROCKET_RISE_DURATION).SetEase(Ease.InSine).OnComplete(() =>
        {
            // 标记上升阶段完成
            if (moveContext != null)
            {
                moveContext.IsRiseComplete = true;
            }
            
            transform.LookAt(target);
            // 计算平滑转向路径
            Vector3 start = transform.position;
            Vector3 end = target.position;
            Vector3 controlPoint1 = start + Vector3.up * SkillConstants.BEZIER_CONTROL_POINT_1_HEIGHT;
            Vector3 controlPoint2 = (start + end) / 2f + Vector3.up * SkillConstants.BEZIER_CONTROL_POINT_2_HEIGHT;

            // 使用贝塞尔曲线计算转向路径
            Vector3[] turnPath = new Vector3[SkillConstants.BEZIER_SAMPLE_POINTS];
            for (int i = 0; i < turnPath.Length; i++)
            {
                float t = (float)i / (turnPath.Length - 1); // 归一化时间
                turnPath[i] = CalculateCubicBezierPoint(start, controlPoint1, controlPoint2, end, t);
            }
            transform.DOPath(turnPath, SkillConstants.ROCKET_TRACK_DURATION, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.InSine).OnComplete(() =>
            {
                // 标记路径完成
                if (moveContext != null)
                {
                    moveContext.IsPathComplete = true;
                }
            });
        });
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
