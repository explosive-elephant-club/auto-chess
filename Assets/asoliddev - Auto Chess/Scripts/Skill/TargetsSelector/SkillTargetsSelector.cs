using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// 技能选择目标的类型
/// </summary>
public enum SkillTargetType
{

    //自身
    Self,
    //队友
    Teammate,
    //敌人
    Enemy
}
/// <summary>
/// 表示技能选择范围的方式
/// </summary>
public enum SkillRangeSelectorType
{
    //自定义
    Custom,
    //范围内所有友军
    TeammatesInRange,
    //范围内所有敌人
    EnemiesInRange,
    //范围内所有棋格
    MapHexInRange,
}
/// <summary>
/// 具体的目标选择策略
/// </summary>
public enum SkillTargetSelectorType
{
    //自定义
    Custom,
    //任一
    Any,
    //最近的
    Nearest,
    //最远的
    Farthest,
    //伤害最高的
    HighestDPS,
    //等级最高的
    HighestLevel,
    //生命值最多的
    MostHP,
    //生命值最少的
    LeastHP,
    //周围友军最多的
    MostTeammatesSurrounded,
    //周围敌人最多的
    MostEnemiesSurrounded,
}
/// <summary>
/// 技能选中的结果
/// </summary>
public class SelectorResult
{
    /// <summary>
    /// 被选中的单位
    /// </summary>
    public List<ChampionController> targets;
    /// <summary>
    /// 被选中的位置
    /// </summary>
    public Vector3 pos;
    public SelectorResult(List<ChampionController> _targets, Vector3 _pos)
    {
        targets = _targets;
        pos = _pos;
    }

    public SelectorResult()
    {
        targets = new List<ChampionController>();
        pos = Vector3.zero;
    }

    public void Clear()
    {
        if (targets != null)
            targets.Clear();
        pos = Vector3.zero;
    }
}

/// <summary>
/// 目标选择器
/// </summary>
public class SkillTargetsSelector
{
    /// <summary>
    /// 根据技能选择目标类型来选择管理该类型单位的管理器
    /// </summary>
    /// <param name="skillTargetType">能选择目标类型</param>
    /// <param name="selfTeam">技能来自于哪只队伍</param>
    /// <returns>该类型单位的管理器</returns>
    public ChampionManager FindTargetsManagerByType(SkillTargetType skillTargetType, ChampionTeam selfTeam)
    {
        switch (skillTargetType)
        {
            case SkillTargetType.Self:
                return GamePlayController.Instance.ownChampionManager;
            case SkillTargetType.Teammate:
                if (selfTeam == ChampionTeam.Player)
                {
                    return GamePlayController.Instance.ownChampionManager;
                }
                else
                {
                    return GamePlayController.Instance.oponentChampionManager;
                }
            case SkillTargetType.Enemy:
                if (selfTeam == ChampionTeam.Player)
                {
                    return GamePlayController.Instance.oponentChampionManager;
                }
                else
                {
                    return GamePlayController.Instance.ownChampionManager;
                }
        }
        return null;
    }
    /// <summary>
    /// 根据不同策略选择目标
    /// </summary>
    /// <param name="skillTargetSelectorType">选择策略</param>
    /// <param name="manager">被选择单位的管理器</param>
    /// <param name="self">释放技能的单位</param>
    /// <param name="distance">选择的最远范围</param>
    /// <returns>选择的结果</returns>
    public ChampionController FindTargetBySelectorType(SkillTargetSelectorType skillTargetSelectorType, ChampionManager manager, ChampionController self, int distance)
    {
        ChampionController c = null;
        List<ChampionController> targetList = manager.championsBattleArray.FindAll(t => t.isDead == false);
        targetList = targetList.FindAll(t => t.GetDistance(self) <= (int)self.attributesController.addRange.GetTrueValue() + distance);
        float maxValue = 0;
        switch (skillTargetSelectorType)
        {
            case SkillTargetSelectorType.Any:
                c = manager.FindAnyTargetInRange(self, (int)self.attributesController.addRange.GetTrueValue() + distance);
                break;
            case SkillTargetSelectorType.Nearest:
                c = manager.FindNearestTarget(self, (int)self.attributesController.addRange.GetTrueValue() + distance);
                break;
            case SkillTargetSelectorType.Farthest:
                c = manager.FindFarthestTarget(self, (int)self.attributesController.addRange.GetTrueValue() + distance);
                break;
            case SkillTargetSelectorType.HighestDPS:
                maxValue = targetList.Max(t => t.totalDamage);
                c = targetList.Where(x => x.totalDamage == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.HighestLevel:
                //maxValue = targetList.Max(t => t.lvl);
                //c = targetList.Where(x => x.lvl == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.MostHP:
                maxValue = targetList.Max(t => t.attributesController.curHealth);
                c = targetList.Where(x => x.attributesController.curHealth == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.LeastHP:
                maxValue = targetList.Min(t => (t.attributesController.curHealth / t.attributesController.maxHealth.GetTrueValue()));
                c = targetList.Where(x => (x.attributesController.curHealth / x.attributesController.maxHealth.GetTrueValue()) == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.MostTeammatesSurrounded:
                maxValue = targetList.Max(t => t.GetTeammateNeighbors().Count);
                c = targetList.Where(x => x.GetTeammateNeighbors().Count == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.MostEnemiesSurrounded:
                maxValue = targetList.Max(t => t.GetEnemyNeighbors().Count);
                c = targetList.Where(x => x.GetEnemyNeighbors().Count == maxValue).FirstOrDefault();
                break;
        }
        return c;
    }
    /// <summary>
    /// 选定目标后，在一定范围内进一步筛选队友或敌人
    /// </summary>
    /// <param name="c">选定的目标</param>
    /// <param name="skillRangeSelectorType">选择范围的方式</param>
    /// <param name="range">范围距离</param>
    /// <param name="selfTeam">释放技能的单位自身队伍</param>
    /// <returns></returns>
    public SelectorResult FindTargetByRange(ChampionController c, SkillRangeSelectorType skillRangeSelectorType, int range, ChampionTeam selfTeam)
    {
        SelectorResult tempResult = new SelectorResult();
        tempResult.targets.Add(c);
        ChampionManager manager;

        switch (skillRangeSelectorType)
        {
            case SkillRangeSelectorType.TeammatesInRange:
                if (range > 1)
                {
                    if (selfTeam == ChampionTeam.Player)
                    {
                        manager = GamePlayController.Instance.ownChampionManager;
                    }
                    else
                    {
                        manager = GamePlayController.Instance.oponentChampionManager;
                    }
                    tempResult.targets = manager.championsBattleArray.FindAll(t => t.isDead == false && t.GetDistance(c) <= range);
                    tempResult.targets.Sort((a, b) => a.GetDistance(c).CompareTo(b.GetDistance(c)));
                }

                break;
            case SkillRangeSelectorType.EnemiesInRange:
                if (range > 1)
                {
                    if (selfTeam == ChampionTeam.Player)
                    {
                        manager = GamePlayController.Instance.oponentChampionManager;
                    }
                    else
                    {
                        manager = GamePlayController.Instance.ownChampionManager;
                    }
                    tempResult.targets = manager.championsBattleArray.FindAll(t => t.isDead == false && t.GetDistance(c) <= range);
                    tempResult.targets.Sort((a, b) => a.GetDistance(c).CompareTo(b.GetDistance(c)));
                }
                break;
            case SkillRangeSelectorType.MapHexInRange:
                //tempResult.mapGrids = Map.Instance.GetGridArea(c.occupyGridInfo, range);
                break;
        }
        return tempResult;
    }
    /// <summary>
    /// 选择目标
    /// </summary>
    /// <param name="skill">技能</param>
    /// <returns>选择的结果</returns>
    public SelectorResult FindTargets(Skill skill)
    {
        //如果技能目标不是自己
        if (skill.skillTargetType != SkillTargetType.Self)
        {
            //先确定管理器
            ChampionManager manager = FindTargetsManagerByType(skill.skillTargetType, skill.owner.team);
            //然后通过管理器筛选目标
            ChampionController c = FindTargetBySelectorType(skill.skillTargetSelectorType, manager, skill.owner, skill.skillData.distance);
            if (c == null)
                return null;
            //扩展到范围的所有目标
            return FindTargetByRange(c, skill.skillRangeSelectorType, skill.skillData.range, skill.owner.team);
        }
        //如果技能目标是自己，则直接返回自己
        else
        {
            return new SelectorResult(new List<ChampionController>() { skill.owner }, Vector3.zero);
        }
    }
}
