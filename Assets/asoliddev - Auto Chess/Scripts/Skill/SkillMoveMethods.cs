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
                MoveAndScale(instance);
                return true;
            case SkillHelper.MoveLogic.FollowSelfAndTurnToTarget:
                FollowSelfAndTurnToTarget(instance, self, target);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                FollowSelfAndTurnAround(instance, self);
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                UpAndFindToTarget(instance);
                return true;
            case SkillHelper.MoveLogic.DurationSweep:
                DurationSweep(instance);
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
                MoveAndScale(instance);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnToTarget:
                FollowSelfAndTurnToTarget(instance, self, getTargetFunc());
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                FollowSelfAndTurnAround(instance, self);
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                UpAndFindToTarget(instance);
                break;
            case SkillHelper.MoveLogic.DurationSweep:
                DurationSweep(instance);
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

    private static void DurationSweep(SkillInstance instance)
    {
        Debug.LogError("DurationSweep");

    }

    private static void UpAndFindToTarget(SkillInstance instance)
    {
        Debug.LogError("UpAndFindToTarget");
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
}
