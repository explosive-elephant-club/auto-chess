/// <summary>
/// 技能系统常量定义
/// 集中管理魔法数字，便于调整和维护
/// </summary>
public static class SkillConstants
{
    public const int Max_FindTargetDistance = 60; 
    #region 特效持续时间
    /// <summary>
    /// 默认命中特效持续时间（秒）
    /// </summary>
    public const float DEFAULT_HIT_EFFECT_DURATION = 1.5f;
    
    /// <summary>
    /// 默认发射特效持续时间（秒）
    /// </summary>
    public const float DEFAULT_EMIT_EFFECT_DURATION = 1.5f;
    #endregion

    #region 扫射参数
    /// <summary>
    /// 默认扫射最大角度（度）
    /// </summary>
    public const float DEFAULT_SWEEP_MAX_ANGLE = 30f;
    #endregion

    #region 角度
    /// <summary>
    /// 完整圆周角度
    /// </summary>
    public const float FULL_ROTATION_ANGLE = 360f;
    #endregion

    #region 投射物参数
    /// <summary>
    /// 默认投射物最大飞行时间（秒）
    /// </summary>
    public const float DEFAULT_MAX_PROJECTILE_DURATION = 5f;
    
    /// <summary>
    /// 火箭类弹道随机高度最小值
    /// </summary>
    public const float ROCKET_MIN_HEIGHT = 3f;
    
    /// <summary>
    /// 火箭类弹道随机高度最大值
    /// </summary>
    public const float ROCKET_MAX_HEIGHT = 7f;
    
    /// <summary>
    /// 火箭类弹道随机水平偏移最小值
    /// </summary>
    public const float ROCKET_MIN_HORIZONTAL_OFFSET = -3f;
    
    /// <summary>
    /// 火箭类弹道随机水平偏移最大值
    /// </summary>
    public const float ROCKET_MAX_HORIZONTAL_OFFSET = 3f;
    
    /// <summary>
    /// 火箭上升阶段持续时间（秒）
    /// </summary>
    public const float ROCKET_RISE_DURATION = 0.5f;
    
    /// <summary>
    /// 火箭追踪阶段持续时间（秒）（旧的固定路径模式，保留备用）
    /// </summary>
    public const float ROCKET_TRACK_DURATION = 1f;
    
    /// <summary>
    /// 火箭追踪阶段转向速度（度/秒），值越大转向越灵敏
    /// </summary>
    public const float ROCKET_TRACKING_TURN_SPEED = 300f;
    
    /// <summary>
    /// 火箭追踪阶段速度倍率（基于基础移动速度的乘数）
    /// </summary>
    public const float ROCKET_TRACKING_SPEED_MULTIPLIER = 2f;
    
    /// <summary>
    /// 贝塞尔曲线控制点1高度偏移
    /// </summary>
    public const float BEZIER_CONTROL_POINT_1_HEIGHT = 5f;
    
    /// <summary>
    /// 贝塞尔曲线控制点2高度偏移
    /// </summary>
    public const float BEZIER_CONTROL_POINT_2_HEIGHT = 2f;
    
    /// <summary>
    /// 贝塞尔曲线采样点数
    /// </summary>
    public const int BEZIER_SAMPLE_POINTS = 5;
    #endregion

    #region 伤害间隔
    /// <summary>
    /// 持续伤害间隔（秒）
    /// </summary>
    public const float DAMAGE_INTERVAL = 1f;
    #endregion

    #region 缩放参数
    /// <summary>
    /// 移动缩放速率
    /// </summary>
    public const float MOVE_SCALE_RATE = 0.3f;
    #endregion

    #region 手榴弹抛物线参数
    /// <summary>
    /// 重力加速度
    /// </summary>
    public const float GRENADE_GRAVITY = 15f;
    
    /// <summary>
    /// 初始垂直速度
    /// </summary>
    public const float GRENADE_INITIAL_VERTICAL_SPEED = 8f;
    
    /// <summary>
    /// 地面高度
    /// </summary>
    public const float GRENADE_GROUND_Y = 0.1f;
    
    /// <summary>
    /// 反弹时垂直速度保留系数（0-1）
    /// </summary>
    public const float GRENADE_BOUNCE_DAMPING = 0.5f;
    
    /// <summary>
    /// 反弹时水平速度衰减系数
    /// </summary>
    public const float GRENADE_HORIZONTAL_DAMPING = 0.7f;
    
    /// <summary>
    /// 停止反弹的最小垂直速度阈值
    /// </summary>
    public const float GRENADE_MIN_BOUNCE_VELOCITY = 1f;
    
    /// <summary>
    /// 滚动摩擦系数（每秒衰减比例）
    /// </summary>
    public const float GRENADE_ROLL_FRICTION = 3f;
    
    /// <summary>
    /// 停止滚动的最小速度阈值
    /// </summary>
    public const float GRENADE_MIN_ROLL_SPEED = 0.1f;
    #endregion
}
