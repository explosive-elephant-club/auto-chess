using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public struct MoveResult
{
    public bool IsPathMove;
    public static MoveResult Default => new MoveResult { IsPathMove = false };
    public static MoveResult PathMove => new MoveResult { IsPathMove = true };
}

public delegate MoveResult MoveHandler(SkillInstance instance, Transform self, Transform target);

public static class SkillMoveMethods
{
    private static readonly Dictionary<SkillHelper.MoveLogic, MoveHandler> MoveHandlers = new()
    {
        { SkillHelper.MoveLogic.None, (_, _, _) => MoveResult.Default },
        { SkillHelper.MoveLogic.MoveAndScale, HandleMoveAndScale },
        { SkillHelper.MoveLogic.FollowSelfAndTurnToTarget, HandleFollowSelfAndTurnToTarget },
        { SkillHelper.MoveLogic.FollowSelfAndTurnAround, HandleFollowSelfAndTurnAround },
        { SkillHelper.MoveLogic.UpAndFindToTarget, HandleUpAndFindToTarget },
        { SkillHelper.MoveLogic.DurationSweep, HandleDurationSweep },
        { SkillHelper.MoveLogic.OnlyEffect, (_, _, _) => MoveResult.Default },
        { SkillHelper.MoveLogic.ParabolaAndRebound, HandleParabolaAndRebound },
        { SkillHelper.MoveLogic.MoveForward, HandleMoveForward },
    };

    public static void SkillMove(this SkillInstance instance, SkillHelper.MoveLogic moveLogic, Transform self, Transform target, out bool isPathMove)
    {
        isPathMove = false;
        if (MoveHandlers.TryGetValue(moveLogic, out var handler))
        {
            var result = handler(instance, self, target);
            isPathMove = result.IsPathMove;
        }
    }

    public static void SkillMove(this SkillInstance instance, SkillHelper.MoveLogic moveLogic, Transform self, System.Func<Transform> getTargetFunc, out bool isPathMove)
    {
        isPathMove = false;
        if (MoveHandlers.TryGetValue(moveLogic, out var handler))
        {
            var target = getTargetFunc?.Invoke();
            var result = handler(instance, self, target);
            isPathMove = result.IsPathMove;
        }
    }

    #region MoveAndScale - 移动并缩放

    private static MoveResult HandleMoveAndScale(SkillInstance instance, Transform self, Transform target)
    {
        var transform = instance.transform;
        if (target != null)
        {
            var end = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.forward = end - transform.position;
        }
        transform.position += transform.forward.normalized * Time.deltaTime * instance.SkillExeContext.GetMoveSpeed();
        transform.localScale += Vector3.one * instance.SkillExeContext.GetMoveSpeed() * .3f * Time.deltaTime;
        return MoveResult.Default;
    }

    #endregion

    #region MoveForward - 向前移动

    private static MoveResult HandleMoveForward(SkillInstance instance, Transform self, Transform target)
    {
        var transform = instance.transform;
        transform.position += transform.forward.normalized * Time.deltaTime * instance.SkillExeContext.GetMoveSpeed();
        return MoveResult.Default;
    }

    #endregion

    #region FollowSelfAndTurnToTarget - 跟随自身并转向目标

    private static MoveResult HandleFollowSelfAndTurnToTarget(SkillInstance instance, Transform self, Transform target)
    {
        if (target == null) return MoveResult.Default;
        var transform = instance.transform;
        transform.position = self.position;
        Vector3 directionToTarget = target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
            instance.SkillExeContext.GetMoveSpeed() * Time.deltaTime);
        return MoveResult.Default;
    }

    #endregion

    #region FollowSelfAndTurnAround - 环绕

    private static MoveResult HandleFollowSelfAndTurnAround(SkillInstance instance, Transform self, Transform target)
    {
        var transform = instance.transform;
        transform.position = self.position;

        float rotationThisFrame = instance.SkillExeContext.GetMoveSpeed() * Time.deltaTime;
        transform.Rotate(Vector3.up, rotationThisFrame);

        var moveContext = instance.MoveContext;
        if (moveContext?.Orbit != null)
        {
            var orbit = moveContext.Orbit.Value;
            orbit.Angle += rotationThisFrame;
            if (orbit.Angle >= SkillConstants.FULL_ROTATION_ANGLE)
                orbit.Angle -= SkillConstants.FULL_ROTATION_ANGLE;
            moveContext.Orbit = orbit;
        }
        return MoveResult.Default;
    }

    #endregion

    #region DurationSweep - 扫射

    private static MoveResult HandleDurationSweep(SkillInstance instance, Transform self, Transform target)
    {
        var transform = instance.transform;
        transform.position = self.position;

        float rotationSpeed = instance.SkillExeContext.GetMoveSpeed() * Time.deltaTime;
        float currentAngle = transform.rotation.eulerAngles.y;

        var moveContext = instance.MoveContext;
        if (moveContext?.Sweep == null) return MoveResult.Default;

        var sweep = moveContext.Sweep.Value;
        float maxSweepAngle = sweep.MaxAngle;

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

        sweep.Angle = currentAngle;
        moveContext.Sweep = sweep;
        return MoveResult.Default;
    }

    #endregion

    #region UpAndFindToTarget - 火箭弹道

    private static MoveResult HandleUpAndFindToTarget(SkillInstance instance, Transform self, Transform target)
    {
        if (target == null) return MoveResult.Default;
        var transform = instance.transform;
        var moveContext = instance.MoveContext;

        if (moveContext?.Rocket != null && moveContext.Rocket.Value.IsRiseComplete)
        {
            RocketTrackTarget(instance, transform, target);
            return MoveResult.PathMove;
        }

        if (moveContext == null) return MoveResult.Default;

        var rocket = moveContext.Rocket ?? RocketData.Default;
        float randomHeight;
        float randomHorizontalOffset;

        if (rocket.RandomHeight == 0)
        {
            randomHeight = Random.Range(SkillConstants.ROCKET_MIN_HEIGHT, SkillConstants.ROCKET_MAX_HEIGHT);
            randomHorizontalOffset = Random.Range(SkillConstants.ROCKET_MIN_HORIZONTAL_OFFSET, SkillConstants.ROCKET_MAX_HORIZONTAL_OFFSET);
            rocket.RandomHeight = randomHeight;
            rocket.RandomHorizontalOffset = randomHorizontalOffset;
            moveContext.Rocket = rocket;
        }
        else
        {
            randomHeight = rocket.RandomHeight;
            randomHorizontalOffset = rocket.RandomHorizontalOffset;
        }

        Vector3 randomTargetPosition =
            transform.position + self.right * randomHorizontalOffset + self.up * randomHeight;

        transform.forward = randomTargetPosition - transform.position;

        moveContext.StartPosition = transform.position;
        moveContext.EndPosition = target.position;

        transform.DOMove(randomTargetPosition, SkillConstants.ROCKET_RISE_DURATION).SetEase(Ease.InSine).OnComplete(() =>
        {
            var r = moveContext.Rocket ?? RocketData.Default;
            r.IsRiseComplete = true;
            moveContext.Rocket = r;

            if (target != null)
            {
                transform.LookAt(target);
            }
        });

        return MoveResult.PathMove;
    }

    private static void RocketTrackTarget(SkillInstance instance, Transform transform, Transform target)
    {
        Vector3 direction = target.position - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                SkillConstants.ROCKET_TRACKING_TURN_SPEED * Time.deltaTime);
        }

        float speed = instance.SkillExeContext.GetMoveSpeed() * SkillConstants.ROCKET_TRACKING_SPEED_MULTIPLIER;
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    #endregion

    #region ParabolaAndRebound - 手榴弹反弹

    private static MoveResult HandleParabolaAndRebound(SkillInstance instance, Transform self, Transform target)
    {
        var transform = instance.transform;
        var moveContext = instance.MoveContext;
        if (moveContext?.Grenade == null) return MoveResult.Default;

        var grenade = moveContext.Grenade.Value;

        if (!grenade.IsInitialized)
        {
            InitializeGrenade(instance, moveContext);
            return MoveResult.Default;
        }

        switch (grenade.Phase)
        {
            case 0:
            case 1:
                UpdateGrenadeFlying(transform, moveContext);
                break;
            case 2:
                UpdateGrenadeRolling(transform, moveContext);
                break;
            case 3:
                break;
        }
        return MoveResult.Default;
    }

    private static void InitializeGrenade(SkillInstance instance, SkillMoveContext moveContext)
    {
        var transform = instance.transform;
        float speed = instance.SkillExeContext.GetMoveSpeed();

        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();

        var grenade = moveContext.Grenade ?? GrenadeData.Default;
        grenade.HorizontalVelocity = forward * speed;
        grenade.VerticalVelocity = SkillConstants.GRENADE_INITIAL_VERTICAL_SPEED;
        grenade.Phase = 0;
        grenade.BounceCount = 0;
        grenade.IsInitialized = true;
        moveContext.Grenade = grenade;

        moveContext.StartPosition = transform.position;
    }

    private static void UpdateGrenadeFlying(Transform transform, SkillMoveContext moveContext)
    {
        float deltaTime = Time.deltaTime;
        var grenade = moveContext.Grenade.Value;

        grenade.VerticalVelocity -= SkillConstants.GRENADE_GRAVITY * deltaTime;

        Vector3 position = transform.position;
        position += grenade.HorizontalVelocity * deltaTime;
        position.y += grenade.VerticalVelocity * deltaTime;

        if (position.y <= SkillConstants.GRENADE_GROUND_Y)
        {
            position.y = SkillConstants.GRENADE_GROUND_Y;

            grenade.BounceCount++;
            grenade.Phase = 1;

            grenade.VerticalVelocity = -grenade.VerticalVelocity * SkillConstants.GRENADE_BOUNCE_DAMPING;
            grenade.HorizontalVelocity *= SkillConstants.GRENADE_HORIZONTAL_DAMPING;

            if (Mathf.Abs(grenade.VerticalVelocity) < SkillConstants.GRENADE_MIN_BOUNCE_VELOCITY)
            {
                grenade.Phase = 2;
                grenade.VerticalVelocity = 0;
                grenade.HasLanded = true;
            }
        }

        moveContext.Grenade = grenade;
        transform.position = position;

        Vector3 velocity = grenade.HorizontalVelocity + Vector3.up * grenade.VerticalVelocity;
        if (velocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }
    }

    private static void UpdateGrenadeRolling(Transform transform, SkillMoveContext moveContext)
    {
        float deltaTime = Time.deltaTime;
        var grenade = moveContext.Grenade.Value;

        float friction = 1f - SkillConstants.GRENADE_ROLL_FRICTION * deltaTime;
        friction = Mathf.Max(0f, friction);
        grenade.HorizontalVelocity *= friction;

        Vector3 position = transform.position;
        position += grenade.HorizontalVelocity * deltaTime;
        position.y = SkillConstants.GRENADE_GROUND_Y;
        transform.position = position;

        if (grenade.HorizontalVelocity.sqrMagnitude < SkillConstants.GRENADE_MIN_ROLL_SPEED * SkillConstants.GRENADE_MIN_ROLL_SPEED)
        {
            grenade.Phase = 3;
            grenade.HorizontalVelocity = Vector3.zero;
        }

        moveContext.Grenade = grenade;
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

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }

    #endregion
}
