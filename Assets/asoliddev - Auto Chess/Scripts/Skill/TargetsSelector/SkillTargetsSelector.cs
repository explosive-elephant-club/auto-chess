using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SelectorResult
{
    public List<ChampionController> targets;
    public List<GridInfo> mapGrids;
    public SelectorResult(List<ChampionController> _targets, List<GridInfo> _mapGrids)
    {
        targets = _targets;
        mapGrids = _mapGrids;
    }

    public SelectorResult()
    {
        targets = new List<ChampionController>();
        mapGrids = new List<GridInfo>();
    }

    public void Clear()
    {
        targets.Clear();
        mapGrids.Clear();
    }
}

public class SkillTargetsSelector
{
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

    public ChampionController FindTargetBySelectorType(SkillTargetSelectorType skillTargetSelectorType, ChampionManager manager, ChampionController self, int distance)
    {
        ChampionController c = null;
        List<ChampionController> targetList = manager.championsHexaMapArray.FindAll(t => t.isDead == false);
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
                maxValue = targetList.Min(t => t.attributesController.curHealth);
                c = targetList.Where(x => x.attributesController.curHealth == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.MostTeammatesSurrounded:
                maxValue = targetList.Max(t => t.occupyGridInfo.neighbors.FindAll(g => g.occupyChampion.team == self.team).Count);
                c = targetList.Where(x => x.occupyGridInfo.neighbors.FindAll(g => g.occupyChampion.team == self.team).Count == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.MostEnemiesSurrounded:
                maxValue = targetList.Max(t => t.occupyGridInfo.neighbors.FindAll(g => g.occupyChampion.team != self.team).Count);
                c = targetList.Where(x => x.occupyGridInfo.neighbors.FindAll(g => g.occupyChampion.team != self.team).Count == maxValue).FirstOrDefault();
                break;
        }
        return c;
    }

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
                    tempResult.targets = manager.championsHexaMapArray.FindAll(t => t.isDead == false && t.GetDistance(c) <= range);
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
                    tempResult.targets = manager.championsHexaMapArray.FindAll(t => t.isDead == false && t.GetDistance(c) <= range);
                    tempResult.targets.Sort((a, b) => a.GetDistance(c).CompareTo(b.GetDistance(c)));
                }
                break;
            case SkillRangeSelectorType.MapHexInRange:
                tempResult.mapGrids = Map.Instance.GetGridArea(c.occupyGridInfo, range);
                break;
        }
        return tempResult;
    }

    public SelectorResult FindTargets(Skill skill)
    {
        if (skill.skillTargetType != SkillTargetType.Self)
        {
            ChampionManager manager = FindTargetsManagerByType(skill.skillTargetType, skill.owner.team);
            ChampionController c = FindTargetBySelectorType(skill.skillTargetSelectorType, manager, skill.owner, skill.skillData.distance);
            if (c == null)
                return null;
            return FindTargetByRange(c, skill.skillRangeSelectorType, skill.skillData.range, skill.owner.team);
        }
        else
        {
            return new SelectorResult(new List<ChampionController>() { skill.owner }, null);
        }
    }
}
