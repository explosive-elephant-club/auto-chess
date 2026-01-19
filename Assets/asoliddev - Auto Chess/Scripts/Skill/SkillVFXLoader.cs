using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VFX资源包，包含技能的所有特效预制体
/// </summary>
public class VFXBundle
{
    public GameObject EmitPrefab { get; set; }
    public GameObject EffectPrefab { get; set; }
    public GameObject HitPrefab { get; set; }
    
    public bool IsLoaded => EmitPrefab != null || EffectPrefab != null || HitPrefab != null;
}

/// <summary>
/// 技能VFX资源预加载和缓存管理器
/// 负责加载、缓存和提供技能特效预制体
/// </summary>
public class SkillVFXLoader
{
    private static SkillVFXLoader _instance;
    public static SkillVFXLoader Instance => _instance ??= new SkillVFXLoader();

    private readonly Dictionary<int, VFXBundle> _cache = new();

    /// <summary>
    /// 战斗前预加载指定技能的VFX资源
    /// </summary>
    /// <param name="skillIds">需要预加载的技能ID列表</param>
    public void PreloadForBattle(IEnumerable<int> skillIds)
    {
        foreach (var skillId in skillIds)
        {
            LoadVFXBundle(skillId);
        }
    }

    /// <summary>
    /// 获取技能的VFX资源包
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>VFX资源包</returns>
    public VFXBundle GetVFXBundle(int skillId)
    {
        if (!_cache.TryGetValue(skillId, out var bundle))
        {
            bundle = LoadVFXBundle(skillId);
        }
        return bundle;
    }

    /// <summary>
    /// 获取发射特效预制体
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>发射特效预制体</returns>
    public GameObject GetEmitPrefab(int skillId)
    {
        return GetVFXBundle(skillId)?.EmitPrefab;
    }

    /// <summary>
    /// 获取效果特效预制体（投射物）
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>效果特效预制体</returns>
    public GameObject GetEffectPrefab(int skillId)
    {
        return GetVFXBundle(skillId)?.EffectPrefab;
    }

    /// <summary>
    /// 获取命中特效预制体
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>命中特效预制体</returns>
    public GameObject GetHitPrefab(int skillId)
    {
        return GetVFXBundle(skillId)?.HitPrefab;
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }

    /// <summary>
    /// 清除指定技能的缓存
    /// </summary>
    /// <param name="skillId">技能ID</param>
    public void ClearCache(int skillId)
    {
        _cache.Remove(skillId);
    }

    /// <summary>
    /// 加载VFX资源包
    /// </summary>
    private VFXBundle LoadVFXBundle(int skillId)
    {
        var path = SkillHelper.GetVFXPath(skillId);
        var bundle = new VFXBundle
        {
            EmitPrefab = Resources.Load<GameObject>(path + "Emit"),
            EffectPrefab = Resources.Load<GameObject>(path + "Effect"),
            HitPrefab = Resources.Load<GameObject>(path + "Hit")
        };
        _cache[skillId] = bundle;
        return bundle;
    }

    /// <summary>
    /// 检查指定技能的VFX是否已缓存
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否已缓存</returns>
    public bool IsCached(int skillId)
    {
        return _cache.ContainsKey(skillId);
    }
}
