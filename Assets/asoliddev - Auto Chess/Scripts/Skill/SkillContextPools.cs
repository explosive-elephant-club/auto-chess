/// <summary>
/// 技能系统 Context 对象池管理器
/// 集中管理各类上下文对象的池化
/// </summary>
public static class SkillContextPools
{
    private const int DEFAULT_PREWARM = 8;
    private const int DEFAULT_MAX_SIZE = 64;

    /// <summary>
    /// HitContext 对象池
    /// </summary>
    public static readonly SkillObjectPool<HitContext> HitContextPool = 
        new(ctx => ctx.Reset(), DEFAULT_PREWARM, DEFAULT_MAX_SIZE);

    /// <summary>
    /// EffectCreateContext 对象池
    /// </summary>
    public static readonly SkillObjectPool<EffectCreateContext> EffectCreatePool = 
        new(ctx => ctx.Reset(), DEFAULT_PREWARM, DEFAULT_MAX_SIZE);

    /// <summary>
    /// DirectEffectContext 对象池
    /// </summary>
    public static readonly SkillObjectPool<DirectEffectContext> DirectEffectPool = 
        new(ctx => ctx.Reset(), DEFAULT_PREWARM, DEFAULT_MAX_SIZE);

    /// <summary>
    /// TargetResult 对象池
    /// </summary>
    public static readonly SkillObjectPool<TargetResult> TargetResultPool = 
        new(r => r.Clear(), DEFAULT_PREWARM, DEFAULT_MAX_SIZE);

    /// <summary>
    /// 清空所有池
    /// </summary>
    public static void ClearAll()
    {
        HitContextPool.Clear();
        EffectCreatePool.Clear();
        DirectEffectPool.Clear();
        TargetResultPool.Clear();
    }
}
