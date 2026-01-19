using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 目标查找结果
/// </summary>
public class TargetResult
{
    /// <summary>
    /// 是否找到有效目标
    /// </summary>
    public bool HasValidTarget { get; set; }
    
    /// <summary>
    /// 主目标
    /// </summary>
    public ChampionController PrimaryTarget { get; set; }
    
    /// <summary>
    /// 所有目标列表
    /// </summary>
    public List<ChampionController> Targets { get; set; }
    
    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3 TargetPosition { get; set; }

    public TargetResult()
    {
        HasValidTarget = false;
        Targets = new List<ChampionController>();
        TargetPosition = Vector3.zero;
    }

    public void Clear()
    {
        HasValidTarget = false;
        PrimaryTarget = null;
        Targets?.Clear();
        TargetPosition = Vector3.zero;
    }
}

/// <summary>
/// 统一的目标查找器
/// 合并 SkillBase.IsFindTarget() 和 SkillBase.FindAvailableTarget() 的逻辑
/// </summary>
public class SkillTargetFinder
{
    private readonly SkillTargetsSelector _selector = new();
    private readonly TargetResult _cachedResult = new();

    /// <summary>
    /// 查找技能目标
    /// </summary>
    /// <param name="runtime">技能运行时数据</param>
    /// <param name="isIdleFindTarget">是否是 idle状态寻找target, 如果是需要把distance拉到最大, 因为需要移动到目标点</param>
    /// <returns>目标查找结果</returns>
    public TargetResult FindTargets(SkillRuntime runtime, bool isIdleFindTarget)
    {
        _cachedResult.Clear();

        if (runtime.SkillTargetType == SkillTargetType.Self)
        {
            _cachedResult.HasValidTarget = true;
            _cachedResult.PrimaryTarget = runtime.Owner;
            _cachedResult.Targets.Add(runtime.Owner);
            return _cachedResult;
        }

        // 查找目标管理器
        ChampionManager manager = _selector.FindTargetsManagerByType(
            runtime.SkillTargetType, 
            runtime.Owner.team
        );

        // 根据选择器类型查找主目标
        ChampionController primaryTarget = _selector.FindTargetBySelectorType(
            runtime.SkillTargetSelectorType,
            manager,
            runtime.Owner,
            isIdleFindTarget ? SkillConstants.Max_FindTargetDistance : runtime.Distance
        );

        if (primaryTarget == null)
        {
            _cachedResult.HasValidTarget = false;
            return _cachedResult;
        }

        // 扩展到范围内所有目标
        SelectorResult rangeResult = _selector.FindTargetByRange(
            primaryTarget,
            runtime.SkillRangeSelectorType,
            runtime.Range,
            runtime.Owner.team
        );

        _cachedResult.HasValidTarget = true;
        _cachedResult.PrimaryTarget = primaryTarget;
        _cachedResult.Targets = rangeResult.targets;
        _cachedResult.TargetPosition = rangeResult.pos;

        return _cachedResult;
    }

    /// <summary>
    /// 检查是否有有效目标
    /// </summary>
    /// <param name="runtime">技能运行时数据</param>
    /// <returns>是否有有效目标</returns>
    public bool HasValidTarget(SkillRuntime runtime)
    {
        return FindTargets(runtime, false).HasValidTarget;
    }

    /// <summary>
    /// 查找可用目标（用于兼容旧接口）
    /// </summary>
    /// <param name="runtime">技能运行时数据</param>
    /// <param name="isIdleFindTarget">是否是 idle状态寻找target, 如果是需要把distance拉到最大, 因为需要移动到目标点</param>
    /// <returns>主目标</returns>
    public ChampionController FindAvailableTarget(SkillRuntime runtime, bool isIdleFindTarget = false)
    {
        var result = FindTargets(runtime, isIdleFindTarget);
        return result.HasValidTarget && result.Targets.Count > 0 
            ? result.Targets[0] 
            : null;
    }

    /// <summary>
    /// 重新获取目标（用于追踪类技能）
    /// </summary>
    /// <param name="runtime">技能运行时数据</param>
    /// <param name="isIdleFindTarget">是否是 idle状态寻找target, 如果是需要把distance拉到最大, 因为需要移动到目标点</param>
    /// <returns>目标Transform</returns>
    public Transform ReGetTarget(SkillRuntime runtime, bool isIdleFindTarget = false)
    {
        var result = FindTargets(runtime, isIdleFindTarget);
        return result.HasValidTarget && result.Targets.Count > 0 
            ? result.Targets[0].transform 
            : null;
    }
}
