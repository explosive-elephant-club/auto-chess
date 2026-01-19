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
        Debug.LogError("ParabolaAndRebound");
    }

    private static void DurationSweep(SkillInstance instance, Transform self)
    {
        var transform = instance.transform;
        transform.position = self.position;

        // 计算每帧应旋转的角度
        float rotationSpeed = instance.SkillExeContext.GetMoveSpeed() * Time.deltaTime;
        float currentAngle = transform.rotation.eulerAngles.y; // 获取当前的旋转角度

        // 应用旋转
        if (instance.MoveParam1)
        {
            transform.Rotate(Vector3.up, rotationSpeed);
            if (currentAngle >= 30)
            {
                instance.MoveParam1 = false; // 到达最大角度后反转方向
            }
        }
        else
        {
            transform.Rotate(Vector3.up, -rotationSpeed);
            if (currentAngle <= -30)
            {
                instance.MoveParam1 = true; // 到达最小角度后反转方向
            }
        }
    }

    private static void UpAndFindToTarget(SkillInstance instance, Transform self, Transform target)
    {
        if(target == null) return;
        var transform = instance.transform;
        // 随机生成向上移动的目标高度
        float randomHeight = Random.Range(3f, 7f); // 例如，随机高度在3到7米之间
        // 随机生成一个水平方向的偏移
        float randomHorizontalOffset = Random.Range(-3f, 3f); // 例如，水平方向偏移在-3到3米之间
        // 使用对象的本地坐标系来计算水平偏移
        Vector3 randomTargetPosition =
            transform.position + self.right * randomHorizontalOffset + self.up * randomHeight;
        
        transform.forward = randomTargetPosition - transform.position;
        // 向上移动
        transform.DOMove(randomTargetPosition, 0.5f).SetEase(Ease.InSine).OnComplete(() =>
        {
            transform.LookAt(target);
            // 计算平滑转向路径
            Vector3 start = transform.position;
            Vector3 end = target.position;
            Vector3 controlPoint1 = start + Vector3.up * 5f; // 第一个控制点，可以调整
            Vector3 controlPoint2 = (start + end) / 2f + Vector3.up * 2f; // 第二个控制点，可以调整

            // 使用贝塞尔曲线计算转向路径
            Vector3[] turnPath = new Vector3[5]; // 计算5个点，可以根据需要调整
            for (int i = 0; i < turnPath.Length; i++)
            {
                float t = (float)i / (turnPath.Length - 1); // 归一化时间
                turnPath[i] = CalculateCubicBezierPoint(start, controlPoint1, controlPoint2, end, t);
            }
            transform.DOPath(turnPath, 1f, PathType.CatmullRom, PathMode.Full3D).SetEase(Ease.InSine);
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
