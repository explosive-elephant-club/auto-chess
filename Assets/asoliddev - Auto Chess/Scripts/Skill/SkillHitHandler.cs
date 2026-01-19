using System.Collections.Generic;
using ExcelConfig;
using UnityEngine;

/// <summary>
/// 技能命中处理器
/// 负责处理技能命中时的伤害、Buff和特效
/// </summary>
public class SkillHitHandler
{
    private readonly SkillVFXLoader _vfxLoader = SkillVFXLoader.Instance;

    /// <summary>
    /// 处理技能命中效果
    /// </summary>
    /// <param name="context">命中上下文</param>
    public void HandleHit(HitContext context)
    {
        // 生成命中特效
        InstantiateHitEffect(context);
        
        if (context.OnlyEffect)
            return;

        // 施加Buff
        ApplyBuffs(context);
        
        // 造成伤害
        ApplyDamage(context);
    }

    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    public void ApplyDamage(HitContext context)
    {
        if (context.Target == null || context.Target.isDead)
            return;

        if (context.SkillData.damageData == null || 
            context.SkillData.damageData.Length == 0 ||
            string.IsNullOrEmpty(context.SkillData.damageData[0].type))
            return;

        context.Caster.championCombatController.TakeDamage(context.Target, context.SkillData.damageData);
    }

    /// <summary>
    /// 对目标施加Buff
    /// </summary>
    public void ApplyBuffs(HitContext context)
    {
        if (context.Target == null || context.Target.isDead)
            return;

        if (context.SkillData.addBuffs == null)
            return;

        foreach (int buffId in context.SkillData.addBuffs)
        {
            if (buffId != 0)
            {
                context.Target.buffController.AddBuff(buffId, context.Caster);
            }
        }
    }

    /// <summary>
    /// 实例化命中特效
    /// </summary>
    public GameObject InstantiateHitEffect(HitContext context)
    {
        var hitPrefab = _vfxLoader.GetHitPrefab(context.SkillData.ID);
        if (hitPrefab == null)
            return null;

        Vector3 hitPosition = context.HitPosition;
        Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, Vector3.zero);
        
        GameObject hitInstance = Object.Instantiate(hitPrefab, hitPosition, hitRotation);
        Object.Destroy(hitInstance, context.HitEffectDuration);
        
        return hitInstance;
    }

    /// <summary>
    /// 对目标列表造成直接效果（伤害+Buff+特效）
    /// </summary>
    public void ApplyDirectEffect(DirectEffectContext context)
    {
        if (context.Targets == null || context.Targets.Count == 0)
            return;

        foreach (var target in context.Targets)
        {
            if (target == null || target.isDead)
                continue;

            var hitContext = new HitContext
            {
                Caster = context.Caster,
                Target = target,
                SkillData = context.SkillData,
                HitPosition = target.transform.position,
                HitEffectDuration = context.HitEffectDuration,
                OnlyEffect = false
            };

            HandleHit(hitContext);
        }
    }
}

/// <summary>
/// 命中上下文
/// </summary>
public class HitContext
{
    /// <summary>
    /// 施法者
    /// </summary>
    public ChampionController Caster { get; set; }
    
    /// <summary>
    /// 被命中目标
    /// </summary>
    public ChampionController Target { get; set; }
    
    /// <summary>
    /// 技能数据
    /// </summary>
    public SkillData SkillData { get; set; }
    
    /// <summary>
    /// 命中位置
    /// </summary>
    public Vector3 HitPosition { get; set; }
    
    /// <summary>
    /// 命中特效持续时间
    /// </summary>
    public float HitEffectDuration { get; set; } = SkillConstants.DEFAULT_HIT_EFFECT_DURATION;
    
    /// <summary>
    /// 是否只播放特效（不造成伤害和Buff）
    /// </summary>
    public bool OnlyEffect { get; set; }
    
    /// <summary>
    /// 碰撞体（用于计算精确命中位置）
    /// </summary>
    public Collider HitCollider { get; set; }
}

/// <summary>
/// 直接效果上下文
/// </summary>
public class DirectEffectContext
{
    /// <summary>
    /// 施法者
    /// </summary>
    public ChampionController Caster { get; set; }
    
    /// <summary>
    /// 目标列表
    /// </summary>
    public List<ChampionController> Targets { get; set; }
    
    /// <summary>
    /// 技能数据
    /// </summary>
    public SkillData SkillData { get; set; }
    
    /// <summary>
    /// 命中特效持续时间
    /// </summary>
    public float HitEffectDuration { get; set; } = SkillConstants.DEFAULT_HIT_EFFECT_DURATION;
}
