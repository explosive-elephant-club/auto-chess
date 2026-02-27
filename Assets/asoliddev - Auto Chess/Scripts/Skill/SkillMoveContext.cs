using UnityEngine;

#region 专用数据结构体

/// <summary>
/// 扫射移动数据 - 用于 DurationSweep
/// </summary>
public struct SweepData
{
    public float Angle;
    public bool DirectionPositive;
    public float MaxAngle;

    public static SweepData Default => new SweepData
    {
        Angle = 0,
        DirectionPositive = true,
        MaxAngle = SkillConstants.DEFAULT_SWEEP_MAX_ANGLE
    };
}

/// <summary>
/// 火箭弹道数据 - 用于 UpAndFindToTarget
/// </summary>
public struct RocketData
{
    public bool IsRiseComplete;
    public float RandomHeight;
    public float RandomHorizontalOffset;

    public static RocketData Default => new RocketData
    {
        IsRiseComplete = false,
        RandomHeight = 0,
        RandomHorizontalOffset = 0
    };
}

/// <summary>
/// 手榴弹反弹数据 - 用于 ParabolaAndRebound
/// </summary>
public struct GrenadeData
{
    /// <summary>
    /// 当前运动阶段：0=飞行, 1=反弹中, 2=滚动, 3=停止
    /// </summary>
    public int Phase;
    public float VerticalVelocity;
    public Vector3 HorizontalVelocity;
    public int BounceCount;
    public bool IsInitialized;
    public bool HasLanded;

    public static GrenadeData Default => new GrenadeData
    {
        Phase = 0,
        VerticalVelocity = 0,
        HorizontalVelocity = Vector3.zero,
        BounceCount = 0,
        IsInitialized = false,
        HasLanded = false
    };
}

/// <summary>
/// 环绕数据 - 用于 FollowSelfAndTurnAround
/// </summary>
public struct OrbitData
{
    public float Angle;
    public float Radius;

    public static OrbitData Default => new OrbitData
    {
        Angle = 0,
        Radius = 2f
    };
}

#endregion

/// <summary>
/// 技能移动上下文
/// 使用组合模式：通用参数 + 按需初始化的专用数据块
/// </summary>
public class SkillMoveContext
{
    #region 通用参数（所有移动类型共用）

    /// <summary>
    /// 技能实例的Transform
    /// </summary>
    public Transform SkillTransform { get; set; }

    /// <summary>
    /// 施法者Transform
    /// </summary>
    public Transform CasterTransform { get; set; }

    /// <summary>
    /// 目标Transform
    /// </summary>
    public Transform TargetTransform { get; set; }

    /// <summary>
    /// 移动速度
    /// </summary>
    public float MoveSpeed { get; set; }

    /// <summary>
    /// 是否为路径移动（由DOTween驱动）
    /// </summary>
    public bool IsPathMove { get; set; }

    /// <summary>
    /// 当前持续时间
    /// </summary>
    public float Duration { get; set; }

    /// <summary>
    /// 起始位置
    /// </summary>
    public Vector3 StartPosition { get; set; }

    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3 EndPosition { get; set; }

    #endregion

    #region 专用数据块（可空，按需初始化）

    /// <summary>
    /// 扫射数据 - 用于 DurationSweep
    /// </summary>
    public SweepData? Sweep { get; set; }

    /// <summary>
    /// 火箭弹道数据 - 用于 UpAndFindToTarget
    /// </summary>
    public RocketData? Rocket { get; set; }

    /// <summary>
    /// 手榴弹数据 - 用于 ParabolaAndRebound
    /// </summary>
    public GrenadeData? Grenade { get; set; }

    /// <summary>
    /// 环绕数据 - 用于 FollowSelfAndTurnAround
    /// </summary>
    public OrbitData? Orbit { get; set; }

    #endregion

    #region 方法

    /// <summary>
    /// 重置所有参数
    /// </summary>
    public void Reset()
    {
        SkillTransform = null;
        CasterTransform = null;
        TargetTransform = null;
        MoveSpeed = 0;
        IsPathMove = false;
        Duration = 0;
        StartPosition = Vector3.zero;
        EndPosition = Vector3.zero;

        Sweep = null;
        Rocket = null;
        Grenade = null;
        Orbit = null;
    }

    /// <summary>
    /// 初始化移动上下文
    /// </summary>
    public void Initialize(Transform skillTransform, Transform casterTransform, Transform targetTransform, float moveSpeed)
    {
        Reset();
        SkillTransform = skillTransform;
        CasterTransform = casterTransform;
        TargetTransform = targetTransform;
        MoveSpeed = moveSpeed;

        if (targetTransform != null)
        {
            EndPosition = targetTransform.position;
        }
        if (skillTransform != null)
        {
            StartPosition = skillTransform.position;
        }
    }

    /// <summary>
    /// 根据移动逻辑类型初始化对应的专用数据块
    /// </summary>
    public void InitializeFor(SkillHelper.MoveLogic moveLogic)
    {
        switch (moveLogic)
        {
            case SkillHelper.MoveLogic.DurationSweep:
                Sweep = SweepData.Default;
                break;
            case SkillHelper.MoveLogic.UpAndFindToTarget:
                Rocket = RocketData.Default;
                break;
            case SkillHelper.MoveLogic.ParabolaAndRebound:
                Grenade = GrenadeData.Default;
                break;
            case SkillHelper.MoveLogic.FollowSelfAndTurnAround:
                Orbit = OrbitData.Default;
                break;
        }
    }

    /// <summary>
    /// 更新持续时间
    /// </summary>
    public void UpdateDuration(float deltaTime)
    {
        Duration += deltaTime;
    }

    #endregion
}
