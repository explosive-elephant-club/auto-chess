using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

/// <summary>
/// 技能效果工厂
/// 负责创建技能效果实例（投射物、发射特效等）
/// 使用 SkillVFXPool 管理特效的池化
/// </summary>
public class SkillEffectFactory
{
    private readonly SkillVFXLoader _vfxLoader = SkillVFXLoader.Instance;
    private readonly SkillVFXPool _vfxPool = SkillVFXPool.Instance;

    /// <summary>
    /// 创建技能投射物实例
    /// 投射物由 SkillStateMachine 管理生命周期，使用池化创建
    /// </summary>
    /// <param name="context">创建上下文</param>
    /// <returns>技能实例组件</returns>
    public SkillInstance CreateProjectile(EffectCreateContext context)
    {
        // 计算生成位置和旋转
        Vector3 spawnPosition = context.UseOwnerPosition 
            ? context.Owner.transform.position 
            : context.CastPoint.position;
            
        Quaternion spawnRotation = context.UseOwnerPosition 
            ? context.Owner.transform.rotation 
            : context.CastPoint.rotation;

        // 从池中获取或创建新实例
        var obj = _vfxPool.Get(context.SkillData.ID, SkillVFXPool.VFXType.Effect, spawnPosition, spawnRotation);
        if (obj == null)
        {
            Debug.LogWarning($"Effect prefab not found for skill {context.SkillData.ID}");
            return null;
        }
        
        // 获取或添加 SkillInstance 组件
        var skillInstance = obj.GetComponent<SkillInstance>();
        if (skillInstance == null)
        {
            skillInstance = obj.AddComponent<SkillInstance>();
        }

        // 记录技能ID用于归还池
        skillInstance.SkillId = context.SkillData.ID;

        return skillInstance;
    }

    /// <summary>
    /// 归还投射物到池
    /// </summary>
    public void ReturnProjectile(int skillId, GameObject obj)
    {
        _vfxPool.Return(skillId, SkillVFXPool.VFXType.Effect, obj);
    }

    /// <summary>
    /// 创建发射特效（使用池化）
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="duration">持续时间</param>
    /// <returns>发射特效实例</returns>
    public GameObject CreateEmitEffect(int skillId, Vector3 position, Quaternion rotation, float duration = SkillConstants.DEFAULT_EMIT_EFFECT_DURATION)
    {
        var emitInstance = _vfxPool.Get(skillId, SkillVFXPool.VFXType.Emit, position, rotation);
        if (emitInstance == null)
            return null;

        // 延迟归还到池
        _vfxPool.ReturnDelayed(skillId, SkillVFXPool.VFXType.Emit, emitInstance, duration);
        
        return emitInstance;
    }

    /// <summary>
    /// 创建命中特效（使用池化）
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">旋转</param>
    /// <param name="duration">持续时间</param>
    /// <returns>命中特效实例</returns>
    public GameObject CreateHitEffect(int skillId, Vector3 position, Quaternion rotation, float duration = SkillConstants.DEFAULT_HIT_EFFECT_DURATION)
    {
        var hitInstance = _vfxPool.Get(skillId, SkillVFXPool.VFXType.Hit, position, rotation);
        if (hitInstance == null)
            return null;

        // 延迟归还到池
        _vfxPool.ReturnDelayed(skillId, SkillVFXPool.VFXType.Hit, hitInstance, duration);
        
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
            // 使用对象池获取上下文
            var instanceContext = SkillContextPools.EffectCreatePool.Get();
            instanceContext.SkillData = context.SkillData;
            instanceContext.Owner = context.Owner;
            instanceContext.Target = context.Target;
            instanceContext.CastPoint = castPoint;
            instanceContext.UseOwnerPosition = context.UseOwnerPosition;
            
            var instance = CreateProjectile(instanceContext);
            SkillContextPools.EffectCreatePool.Return(instanceContext);
            
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

    /// <summary>
    /// 重置上下文，用于对象池归还
    /// </summary>
    public void Reset()
    {
        SkillData = null;
        Owner = null;
        Target = null;
        CastPoint = null;
        UseOwnerPosition = false;
        ExecutionContext = null;
    }
}
