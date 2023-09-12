using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExcelConfig;
using System.Linq;
using UnityEngine.Events;


public enum SkillTargetType//目标类型
{

    //自身
    Self,
    //队友
    Teammate,
    //敌人
    Enemy
}
public enum SkillRangeSelectorType//范围选择类型
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

public enum SkillTargetSelectorType//目标选择类型
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

public enum SkillState
{
    Disable,
    Casting,
    CD
}

[Serializable]
public class Skill
{
    public SkillData skillData;

    public SkillTargetType skillTargetType;
    public SkillRangeSelectorType skillRangeSelectorType;
    public SkillTargetSelectorType skillTargetSelectorType;

    public ChampionController owner;//技能的拥有者
    public ConstructorBase constructor;//技能的载体


    public float cdRemain;
    public float countRemain;

    public GameObject effectPrefab;
    public GameObject hitFXPrefab;
    public Sprite icon;

    public List<ChampionController> targets = new List<ChampionController>();
    public List<GridInfo> mapGrids = new List<GridInfo>();

    public SkillState state;

    public SkillEffect effectScript;
    public SkillController skillController;
    public SkillBehaviour skillBehaviour;

    public ChampionManager manager;


    public Skill(SkillData _skillData, ChampionController _owner, ConstructorBase _constructor)
    {
        skillData = _skillData;

        skillTargetType = (SkillTargetType)Enum.Parse(typeof(SkillTargetType), skillData.skillTargetType);
        skillRangeSelectorType = (SkillRangeSelectorType)Enum.Parse(typeof(SkillRangeSelectorType), skillData.skillRangeSelectorType);
        skillTargetSelectorType = (SkillTargetSelectorType)Enum.Parse(typeof(SkillTargetSelectorType), skillData.skillTargetSelectorType);

        owner = _owner;
        constructor = _constructor;
        cdRemain = 0;
        countRemain = skillData.count;

        if (!string.IsNullOrEmpty(skillData.effectPrefab))
        {
            effectPrefab = Resources.Load<GameObject>(skillData.effectPrefab);
        }


        if (!string.IsNullOrEmpty(skillData.hitFXPrefab))
            hitFXPrefab = Resources.Load<GameObject>(skillData.hitFXPrefab);
        icon = Resources.Load<Sprite>(skillData.icon);

        if (!string.IsNullOrEmpty(skillData.skillBehaviourScriptName))
        {
            try
            {
                Type type = Type.GetType(skillData.skillBehaviourScriptName);
                skillBehaviour = (SkillBehaviour)Activator.CreateInstance(type);
                skillBehaviour.Init(this);
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

        state = SkillState.Disable;

        skillController = owner.skillController;
    }

    public bool IsPrepared()
    {
        Debug.Log("cdRemain " + cdRemain);
        Debug.Log(skillBehaviour.IsPrepared());
        if (countRemain > 0 && cdRemain <= 0 && skillBehaviour.IsPrepared())
        {
            Debug.Log("Prepared");
            return IsFindTarget();
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
            if (targets.Count == 0 || targets[0] == null)
            {
                return false;
            }
        }
        FindTargetByRange();
        if (targets.Count == 0 && mapGrids.Count == 0)
        {
            return false;
        }
        Debug.Log("FindTarget");
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
            case SkillTargetSelectorType.Custom:
                c = skillBehaviour.FindTargetBySelectorType();
                break;
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
            case SkillRangeSelectorType.Custom:
                skillBehaviour.FindTargetByRange();
                break;
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

    public void Cast()
    {
        state = SkillState.Casting;
        skillController.curCastingSkill = this;
        cdRemain = skillData.cd;
        countRemain -= 1;

        skillBehaviour.OnCast(constructor.skillCastPoint);
    }

    public void InstanceEffect()
    {
        //生成技能特效弹道
        if (effectPrefab != null)
        {
            GameObject effectInstance = GameObject.Instantiate(effectPrefab);
            effectInstance.transform.position = constructor.skillCastPoint.position;
            effectScript = effectInstance.GetComponent<SkillEffect>();
            effectScript.Init(this);
        }
        else  //无特效弹道
        {
            Effect();
        }
    }

    public void Effect()
    {
        skillBehaviour.OnEffect();
    }

    public void OnCastingUpdate()
    {
        skillBehaviour.OnCastingUpdate();
    }

    public void OnFinish()
    {
        state = SkillState.CD;
        skillBehaviour.OnFinish();
        skillController.curCastingSkill = null;
    }

    //计时器触发
    public void CDTick()
    {
        cdRemain -= Time.deltaTime;
    }

    public void Reset()
    {
        countRemain = skillData.count;
        cdRemain = 0;
    }
}
