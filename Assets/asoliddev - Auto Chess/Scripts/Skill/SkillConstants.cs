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
    /// 火箭追踪阶段持续时间（秒）
    /// </summary>
    public const float ROCKET_TRACK_DURATION = 1f;
    
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
}
