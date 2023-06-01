using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExcelConfig;
using System.Linq;



public enum SkillTargetType//目标类型
{
    [Tooltip("自身")]
    Self,
    [Tooltip("队友")]
    Teammate,
    [Tooltip("敌人")]
    Enemy
}
public enum SkillRangeSelectorType//范围选择类型
{
    [Tooltip("范围内所有友军")]
    TeammatesInRange,
    [Tooltip("范围内所有敌人")]
    EnemiesInRange,
    [Tooltip("范围内所有棋格")]
    MapHexInRange,
}

public enum SkillTargetSelectorType//目标选择类型
{
    [Tooltip("任一")]
    Any,
    [Tooltip("最近的")]
    Nearest,
    [Tooltip("最远的")]
    Farthest,
    [Tooltip("伤害最高的")]
    HighestDPS,
    [Tooltip("等级最高的")]
    HighestLevel,
    [Tooltip("生命值最多的")]
    MostHP,
    [Tooltip("生命值最少的")]
    LeastHP,
    [Tooltip("周围友军最多的")]
    MostTeammatesSurrounded,
    [Tooltip("周围敌人最多的")]
    MostEnemiesSurrounded,
}

[Serializable]
public class Skill
{
    public SkillData skillData;

    public SkillTargetType skillTargetType;
    public SkillRangeSelectorType skillRangeSelectorType;
    public SkillTargetSelectorType skillTargetSelectorType;

    public ChampionController owner;//技能的拥有者
    public ChampionController caster;//技能的施加者

    public float cdRemain;

    public GameObject effectPrefab;
    public GameObject hitFXPrefab;
    public Sprite icon;

    public SkillBehaviour skillBehaviour;
    public List<ChampionController> targets = new List<ChampionController>();
    public List<GridInfo> mapGrids = new List<GridInfo>();

    ChampionManager manager;

    public Skill(SkillData _skillData, ChampionController _owner, ChampionController _caster)
    {
        skillData = _skillData;

        skillTargetType = (SkillTargetType)Enum.Parse(typeof(SkillTargetType), skillData.skillTargetType);
        skillRangeSelectorType = (SkillRangeSelectorType)Enum.Parse(typeof(SkillRangeSelectorType), skillData.skillRangeSelectorType);
        skillTargetSelectorType = (SkillTargetSelectorType)Enum.Parse(typeof(SkillTargetSelectorType), skillData.skillTargetSelectorType);

        owner = _owner;
        caster = _caster;
        cdRemain = 0;

        effectPrefab = Resources.Load<GameObject>(skillData.effectPrefab);
        hitFXPrefab = Resources.Load<GameObject>(skillData.hitFXPrefab);
        icon = Resources.Load<Sprite>(skillData.icon);

        if (!string.IsNullOrEmpty(skillData.skillBehaviourScriptName))
        {
            try
            {
                Type type = Type.GetType(skillData.skillBehaviourScriptName);
                skillBehaviour = (SkillBehaviour)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Debug.LogError("Create BuffBehaviour instance failed: " + ex.Message);
                skillBehaviour = new SkillBehaviour();
            }
        }
        else
        {
            skillBehaviour = new SkillBehaviour();
        }
    }

    public bool IsPrepared()
    {
        if (cdRemain <= 0 && skillBehaviour.IsPrepared())
        {
            return true;
        }
        return false;
    }

    public bool IsFindTarget()
    {
        targets = new List<ChampionController>();
        mapGrids = new List<GridInfo>();

        FindTargetsByType();
        if (skillTargetType != SkillTargetType.Self)
        {
            targets.Add(FindTargetBySelectorType());
            if (targets.Count == 0)
            {
                return false;
            }
        }
        FindTargetByRange();
        if (targets.Count == 0)
        {
            return false;
        }
        return true;
    }

    public void FindTargetsByType()
    {
        switch (skillTargetType)
        {
            case SkillTargetType.Self:
                targets.Add(owner);
                break;
            case SkillTargetType.Teammate:
                if (owner.team == ChampionTeam.Player)
                {
                    manager = GamePlayController.Instance.ownChampionManager;
                }
                else
                {
                    manager = GamePlayController.Instance.oponentChampionManager;
                }
                break;
            case SkillTargetType.Enemy:
                if (owner.team == ChampionTeam.Player)
                {
                    manager = GamePlayController.Instance.oponentChampionManager;
                }
                else
                {
                    manager = GamePlayController.Instance.ownChampionManager;
                }
                break;
        }
    }

    public ChampionController FindTargetBySelectorType()
    {
        ChampionController c = null;
        List<ChampionController> targetList = manager.championsHexaMapArray.FindAll(t => t.isDead == false);
        targetList = targetList.FindAll(t => t.GetDistance(owner) <= skillData.distance);
        float maxValue = 0;
        switch (skillTargetSelectorType)
        {
            case SkillTargetSelectorType.Any:
                c = manager.FindAnyTargetInRange(owner, skillData.distance);
                break;
            case SkillTargetSelectorType.Nearest:
                c = manager.FindNearestTarget(owner, skillData.distance);
                break;
            case SkillTargetSelectorType.Farthest:
                c = manager.FindFarthestTarget(owner, skillData.distance);
                break;
            case SkillTargetSelectorType.HighestDPS:
                maxValue = targetList.Max(t => t.totalDamage);
                c = targetList.Where(x => x.totalDamage == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.HighestLevel:
                maxValue = targetList.Max(t => t.lvl);
                c = targetList.Where(x => x.lvl == maxValue).FirstOrDefault();
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
                maxValue = targetList.Max(t => t.occupyGridInfo.neighbors.FindAll(g => g.occupyChampion.team == owner.team).Count);
                c = targetList.Where(x => x.occupyGridInfo.neighbors.FindAll(g => g.occupyChampion.team == owner.team).Count == maxValue).FirstOrDefault();
                break;
            case SkillTargetSelectorType.MostEnemiesSurrounded:
                maxValue = targetList.Max(t => t.occupyGridInfo.neighbors.FindAll(g => g.occupyChampion.team != owner.team).Count);
                c = targetList.Where(x => x.occupyGridInfo.neighbors.FindAll(g => g.occupyChampion.team != owner.team).Count == maxValue).FirstOrDefault();
                break;
        }
        return c;
    }

    public void FindTargetByRange()
    {

        ChampionController c = targets[0];
        switch (skillRangeSelectorType)
        {
            case SkillRangeSelectorType.TeammatesInRange:
                if (owner.team == ChampionTeam.Player)
                {
                    manager = GamePlayController.Instance.ownChampionManager;
                }
                else
                {
                    manager = GamePlayController.Instance.oponentChampionManager;
                }
                targets = manager.championsHexaMapArray.FindAll(t => t.isDead == false && t.GetDistance(c) <= skillData.range);
                break;
            case SkillRangeSelectorType.EnemiesInRange:
                if (owner.team == ChampionTeam.Player)
                {
                    manager = GamePlayController.Instance.oponentChampionManager;
                }
                else
                {
                    manager = GamePlayController.Instance.ownChampionManager;
                }
                targets = manager.championsHexaMapArray.FindAll(t => t.isDead == false && t.GetDistance(c) <= skillData.range);
                break;
            case SkillRangeSelectorType.MapHexInRange:
                mapGrids = Map.Instance.GetGridArea(c.occupyGridInfo, skillData.range);
                break;
        }
    }
}
