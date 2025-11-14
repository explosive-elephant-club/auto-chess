using UnityEngine;

public static class SkillMoveMethods
{
    public static void SkillMove(this Transform transform, SkillHelper.MoveLogic moveLogic, Transform self, Transform target, out bool isPathMove)
    {
        isPathMove = false;
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.None:
                break;
            case SkillHelper.MoveLogic.MoveAndScale:
                MoveAndScale(transform);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnToTarget:
                FollowSelfAndTurnToTarget(transform);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                FollowSelfAndTurnAround(transform);
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                UpAndFindToTarget(transform);
                break;
            case SkillHelper.MoveLogic.DurationSweep:
                DurationSweep(transform);
                break;
            case SkillHelper.MoveLogic.OnlyEffect:
                break;
            case SkillHelper.MoveLogic.ParabolaAndRebound:
                ParabolaAndRebound(transform);
                break;
            case SkillHelper.MoveLogic.MoveForward:
                MoveForward(transform);
                break;
        }
    }
    
    public static void SkillMove(this Transform transform, SkillHelper.MoveLogic moveLogic, Transform self, System.Func<Transform> getTargetFunc, out bool isPathMove)
    {
        isPathMove = false;
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.None:
                break;
            case SkillHelper.MoveLogic.MoveAndScale:
                MoveAndScale(transform);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnToTarget:
                FollowSelfAndTurnToTarget(transform);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                FollowSelfAndTurnAround(transform);
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                UpAndFindToTarget(transform);
                break;
            case SkillHelper.MoveLogic.DurationSweep:
                DurationSweep(transform);
                break;
            case SkillHelper.MoveLogic.OnlyEffect:
                break;
            case SkillHelper.MoveLogic.ParabolaAndRebound:
                ParabolaAndRebound(transform);
                break;
            case SkillHelper.MoveLogic.MoveForward:
                MoveForward(transform);
                break;
        }
    }

    private static void ParabolaAndRebound(Transform transform)
    {
        Debug.LogError("ParabolaAndRebound");
    }

    private static void DurationSweep(Transform transform)
    {
        Debug.LogError("DurationSweep");

    }

    private static void UpAndFindToTarget(Transform transform)
    {
        Debug.LogError("UpAndFindToTarget");
    }

    private static void FollowSelfAndTurnAround(Transform transform)
    {
        Debug.LogError("FollowSelfAndTurnAround");
    }

    private static void FollowSelfAndTurnToTarget(Transform transform)
    {
        Debug.LogError("FollowSelfAndTurnToTarget");
    }

    private static void MoveAndScale(Transform transform)
    {
        Debug.LogError("MoveAndScale");
    }

    private static void MoveForward(Transform transform)
    {
        Debug.LogError("MoveForward");
    }
}
