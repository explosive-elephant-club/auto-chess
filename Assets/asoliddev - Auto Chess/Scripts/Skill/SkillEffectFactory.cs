using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

/// <summary>
/// 技能效果工厂
/// 负责创建技能效果实例（投射物、发射特效等）
/// </summary>
public class SkillEffectFactory
{
    private readonly SkillVFXLoader _vfxLoader = SkillVFXLoader.Instance;

    /// <summary>
    /// 创建技能投射物实例
    /// </summary>
    /// <param name="context">创建上下文</param>
    /// <returns>技能实例组件</returns>
    public SkillInstance CreateProjectile(EffectCreateContext context)
    {
        var effectPrefab = _vfxLoader.GetEffectPrefab(context.SkillData.ID);
        if (effectPrefab == null)
        {
            Debug.LogWarning($"Effect prefab not found for skill {context.SkillData.ID}");
            return null;
        }

        // 计算生成位置和旋转
        Vector3 spawnPosition = context.UseOwnerPosition 
            ? context.Owner.transform.position 
            : context.CastPoint.position;
            
        Quaternion spawnRotation = context.UseOwnerPosition 
            ? context.Owner.transform.rotation 
            : context.CastPoint.rotation;

        // 实例化
        var obj = Object.Instantiate(effectPrefab, spawnPosition, spawnRotation);
        
        // 获取或添加 SkillInstance 组件
        var skillInstance = obj.GetComponent<SkillInstance>();
        if (skillInstance == null)
        {
            skillInstance = obj.AddComponent<SkillInstance>();
        }

        return skillInstance;
    }

    /// <summary>
    /// 创建发射特效
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="duration">持续时间</param>
    /// <returns>发射特效实例</returns>
    public GameObject CreateEmitEffect(int skillId, Vector3 position, Quaternion rotation, float duration = SkillConstants.DEFAULT_EMIT_EFFECT_DURATION)
    {
        var emitPrefab = _vfxLoader.GetEmitPrefab(skillId);
        if (emitPrefab == null)
            return null;

        var emitInstance = Object.Instantiate(emitPrefab, position, rotation);
        Object.Destroy(emitInstance, duration);
        
        return emitInstance;
    }

    /// <summary>
    /// 创建命中特效
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="duration">持续时间</param>
    /// <returns>命中特效实例</returns>
    public GameObject CreateHitEffect(int skillId, Vector3 position, Quaternion rotation, float duration = SkillConstants.DEFAULT_HIT_EFFECT_DURATION)
    {
        var hitPrefab = _vfxLoader.GetHitPrefab(skillId);
        if (hitPrefab == null)
            return null;

        var hitInstance = Object.Instantiate(hitPrefab, position, rotation);
        Object.Destroy(hitInstance, duration);
        
        return hitInstance;
    }

    /// <summary>
    /// 批量创建投射物（用于多发射点的技能）
    /// </summary>
    /// <param name="context">创建上下文</param>
    /// <param name="castPoints">发射点列表</param>
    /// <returns>技能实例列表</returns>
    public List<SkillInstance> CreateMultipleProjectiles(EffectCreateContext context, Transform[] castPoints)
    {
        var instances = new List<SkillInstance>();
        
        foreach (var castPoint in castPoints)
        {
            var instanceContext = new EffectCreateContext
            {
                SkillData = context.SkillData,
                Owner = context.Owner,
                Target = context.Target,
                CastPoint = castPoint,
                UseOwnerPosition = context.UseOwnerPosition
            };
            
            var instance = CreateProjectile(instanceContext);
            if (instance != null)
            {
                instances.Add(instance);
            }
        }
        
        return instances;
    }
}

/// <summary>
/// 效果创建上下文
/// </summary>
public class EffectCreateContext
{
    /// <summary>
    /// 技能数据
    /// </summary>
    public SkillData SkillData { get; set; }
    
    /// <summary>
    /// 技能拥有者
    /// </summary>
    public ChampionController Owner { get; set; }
    
    /// <summary>
    /// 目标
    /// </summary>
    public ChampionController Target { get; set; }
    
    /// <summary>
    /// 发射点
    /// </summary>
    public Transform CastPoint { get; set; }
    
    /// <summary>
    /// 是否使用拥有者位置（而非发射点位置）
    /// </summary>
    public bool UseOwnerPosition { get; set; }
    
    /// <summary>
    /// 技能执行上下文（用于初始化 SkillInstance）
    /// </summary>
    public ISkillExecutionContext ExecutionContext { get; set; }
}
