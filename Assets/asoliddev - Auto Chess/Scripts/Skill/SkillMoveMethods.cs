using UnityEngine;

public static class SkillMoveMethods
{
    public static void SkillMove(this Transform transform, SkillHelper.MoveLogic moveLogic, Transform self, Transform targetTransform, out bool isPathMove)
    {
        isPathMove = false;
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.None:
                break;
            case SkillHelper.MoveLogic.MoveAndScale:
                MoveAndScale(transform, targetTransform);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnToTarget:
                FollowSelfAndTurnToTarget(transform, targetTransform);
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                FollowSelfAndTurnAround(transform, targetTransform);
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                UpAndFindToTarget(transform, targetTransform);
                break;
            case SkillHelper.MoveLogic.DurationSweep:
                DurationSweep(transform, targetTransform);
                break;
            case SkillHelper.MoveLogic.OnlyEffect:
                break;
            case SkillHelper.MoveLogic.ParabolaAndRebound:
                ParabolaAndRebound(transform, targetTransform);
                break;
            case SkillHelper.MoveLogic.MoveForward:
                MoveForward(transform, targetTransform);
                break;
        }
    }

    private static void ParabolaAndRebound(Transform transform, Transform targetTransform)
    {
    }

    private static void DurationSweep(Transform transform, Transform targetTransform)
    {
    }

    private static void UpAndFindToTarget(Transform transform, Transform targetTransform)
    {
    }

    private static void FollowSelfAndTurnAround(Transform transform, Transform targetTransform)
    {
    }

    private static void FollowSelfAndTurnToTarget(Transform transform, Transform targetTransform)
    {
    }

    private static void MoveAndScale(Transform transform, Transform targetTransform)
    {
    }

    private static void MoveForward(Transform transform, Transform targetTransform)
    {
    }
}
