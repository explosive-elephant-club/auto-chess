using UnityEngine;

/// <summary>
/// 技能移动上下文
/// 替代 SkillInstance 中的 MoveParam1、MoveParam2 等无语义参数
/// </summary>
public class SkillMoveContext
{
    #region 通用参数
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
    #endregion

    #region 扫射类参数
    /// <summary>
    /// 扫射角度
    /// </summary>
    public float SweepAngle { get; set; }
    
    /// <summary>
    /// 扫射方向是否为正向
    /// </summary>
    public bool SweepDirectionPositive { get; set; }
    
    /// <summary>
    /// 扫射最大角度
    /// </summary>
    public float SweepMaxAngle { get; set; } = SkillConstants.DEFAULT_SWEEP_MAX_ANGLE;
    #endregion

    #region 抛物线参数
    /// <summary>
    /// 抛物线进度 (0-1)
    /// </summary>
    public float ArcProgress { get; set; }
    
    /// <summary>
    /// 是否已落地
    /// </summary>
    public bool HasLanded { get; set; }
    
    /// <summary>
    /// 抛物线高度
    /// </summary>
    public float ArcHeight { get; set; } = 5f;
    
    /// <summary>
    /// 起始位置
    /// </summary>
    public Vector3 StartPosition { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3 EndPosition { get; set; }
    #endregion

    #region 路径移动参数
    /// <summary>
    /// 路径是否完成
    /// </summary>
    public bool IsPathComplete { get; set; }
    
    /// <summary>
    /// 上升阶段是否完成（用于火箭类弹道）
    /// </summary>
    public bool IsRiseComplete { get; set; }
    
    /// <summary>
    /// 随机上升高度
    /// </summary>
    public float RandomHeight { get; set; }
    
    /// <summary>
    /// 随机水平偏移
    /// </summary>
    public float RandomHorizontalOffset { get; set; }
    #endregion

    #region 环绕参数
    /// <summary>
    /// 当前环绕角度
    /// </summary>
    public float OrbitAngle { get; set; }
    
    /// <summary>
    /// 环绕半径
    /// </summary>
    public float OrbitRadius { get; set; } = 2f;
    #endregion

    #region 手榴弹反弹参数
    /// <summary>
    /// 当前运动阶段：0=飞行, 1=反弹中, 2=滚动, 3=停止
    /// </summary>
    public int GrenadePhase { get; set; }
    
    /// <summary>
    /// 当前垂直速度（用于模拟重力）
    /// </summary>
    public float VerticalVelocity { get; set; }
    
    /// <summary>
    /// 当前水平速度向量
    /// </summary>
    public Vector3 HorizontalVelocity { get; set; }
    
    /// <summary>
    /// 当前反弹次数
    /// </summary>
    public int BounceCount { get; set; }
    
    /// <summary>
    /// 是否已初始化（避免重复初始化）
    /// </summary>
    public bool IsGrenadeInitialized { get; set; }
    #endregion

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
        
        // 扫射
        SweepAngle = 0;
        SweepDirectionPositive = true;
        
        // 抛物线
        ArcProgress = 0;
        HasLanded = false;
        StartPosition = Vector3.zero;
        EndPosition = Vector3.zero;
        
        // 路径
        IsPathComplete = false;
        IsRiseComplete = false;
        RandomHeight = 0;
        RandomHorizontalOffset = 0;
        
        // 环绕
        OrbitAngle = 0;
        
        // 手榴弹
        GrenadePhase = 0;
        VerticalVelocity = 0;
        HorizontalVelocity = Vector3.zero;
        BounceCount = 0;
        IsGrenadeInitialized = false;
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
    /// 更新持续时间
    /// </summary>
    public void UpdateDuration(float deltaTime)
    {
        Duration += deltaTime;
    }
}
