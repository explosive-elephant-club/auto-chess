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

public class Skill
{
    public SkillData skillData;

    public float curTime = 0;
    public float intervalTime = 0;
    public float curIntervalTime = 0;
    public float curEffectCount = 0;

    public SkillTargetType skillTargetType;
    public SkillRangeSelectorType skillRangeSelectorType;
    public SkillTargetSelectorType skillTargetSelectorType;

    public ChampionController owner;//技能的拥有者
    public ConstructorBase constructor;//技能的载体
    public int countRemain;

    public GameObject emitPrefab;
    public GameObject effectPrefab;
    public GameObject hitFXPrefab;
    public Sprite icon;

    public SkillTargetsSelector targetsSelector;
    public SelectorResult selectorResult;

    public SkillState state;

    public List<SkillEffect> effectInstances = new List<SkillEffect>();
    public SkillController skillController;
    public List<SkillDecorator> skillDecorators = new List<SkillDecorator>();

    public ChampionManager manager;


    public int curCastPointIndex;


    public Func<bool> IsFindTargetFunc;
    public Action CastFunc;
    public Action EffectFunc;
    public Action DirectEffectFunc;
    public Action InstanceEffectFunc;
    public Action<ChampionController> AddBuffToTargetFunc;
    public Action<ChampionController> AddDMGToTargetFunc;
    public Action OnCastingUpdateFunc;
    public Func<bool> IsFinishFunc;
    public Action DestroyEffectFunc;
    public Action OnFinishFunc;
    public Action ResetFunc;
    public Action PlayCastAnimFunc;
    public Action PlayEndAnimFunc;

    public void Init(SkillData _skillData, ChampionController _owner, ConstructorBase _constructor)
    {
        skillData = _skillData;

        skillTargetType = (SkillTargetType)Enum.Parse(typeof(SkillTargetType), skillData.skillTargetType);
        skillRangeSelectorType = (SkillRangeSelectorType)Enum.Parse(typeof(SkillRangeSelectorType), skillData.skillRangeSelectorType);
        skillTargetSelectorType = (SkillTargetSelectorType)Enum.Parse(typeof(SkillTargetSelectorType), skillData.skillTargetSelectorType);

        owner = _owner;
        constructor = _constructor;
        intervalTime = skillData.duration / skillData.effectiveTimes;
        countRemain = skillData.usableCount;
        curEffectCount = 0;
        curCastPointIndex = 0;

        selectorResult = new SelectorResult();
        targetsSelector = new SkillTargetsSelector();

        if (!string.IsNullOrEmpty(skillData.emitFXPrefab))
            emitPrefab = Resources.Load<GameObject>("Prefab/Projectile/Skill/Emit/" + skillData.emitFXPrefab);
        if (!string.IsNullOrEmpty(skillData.effectPrefab))
        {
            effectPrefab = Resources.Load<GameObject>("Prefab/Projectile/Skill/Effect/" + skillData.effectPrefab);
        }
        if (!string.IsNullOrEmpty(skillData.hitFXPrefab))
            hitFXPrefab = Resources.Load<GameObject>("Prefab/Projectile/Skill/Hit/" + skillData.hitFXPrefab);

        icon = Resources.Load<Sprite>(skillData.icon);

        state = SkillState.Disable;
        skillController = owner.skillController;

        if (!string.IsNullOrEmpty(skillData.skillDecorators[0]))
        {
            foreach (var d in skillData.skillDecorators)
            {
                GetDecorator(d);
            }
        }

        BindFunc();
    }

    public void BindFunc()
    {
        IsFindTargetFunc = IsFindTarget;

        CastFunc = Cast;
        EffectFunc = Effect;
        DirectEffectFunc = DirectEffect;
        InstanceEffectFunc = InstanceEffect;
        AddBuffToTargetFunc = AddBuffToTarget;
        AddDMGToTargetFunc = AddDMGToTarget;
        OnCastingUpdateFunc = OnCastingUpdate;
        IsFinishFunc = IsFinish;
        DestroyEffectFunc = DestroyEffect;
        OnFinishFunc = OnFinish;
        ResetFunc = Reset;
        PlayCastAnimFunc = PlayCastAnim;
        PlayEndAnimFunc = PlayEndAnim;
    }

    public bool IsAvailable()
    {
        if (countRemain > 0 || countRemain == -1)
            if (owner.attributesController.curMana >= skillData.manaCost)
            {
                return true;
            }
        return false;
    }

    public bool IsPrepared()
    {
        if (IsAvailable())
            return IsFindTargetFunc();
        return false;
    }

    public virtual bool IsFindTarget()
    {
        if (skillTargetType != SkillTargetType.Self)
        {
            ChampionManager manager = targetsSelector.FindTargetsManagerByType(skillTargetType, owner.team);
            ChampionController c = targetsSelector.FindTargetBySelectorType(skillTargetSelectorType, manager, owner, skillData.distance);
            if (c == null)
                return false;
            selectorResult = targetsSelector.FindTargetByRange(c, skillRangeSelectorType, skillData.range, owner.team);
            return true;
        }
        else
        {
            selectorResult = new SelectorResult(new List<ChampionController>() { owner }, null);
            return true;
        }
    }

    public virtual void Cast()
    {
        state = SkillState.Casting;
        owner.buffController.eventCenter.Broadcast(BuffActiveMode.BeforeCast.ToString());

        curTime = 0;
        curIntervalTime = 0;
        curEffectCount = 0;

        owner.attributesController.curMana -= skillData.manaCost;

        if (countRemain != -1)
            countRemain -= 1;

        PlayCastAnimFunc();

        owner.buffController.eventCenter.Broadcast(BuffActiveMode.AfterCast.ToString());
    }


    public virtual void InstanceEffect()
    {
        curCastPointIndex = (curCastPointIndex + 1) % constructor.skillCastPoints.Length;

        GameObject obj = GameObject.Instantiate(effectPrefab);
        //obj.transform.parent = GetCastPoint();
        obj.transform.position = GetCastPoint().position;
        obj.transform.rotation = GetCastPoint().rotation;

        SkillEffect skillEffect = obj.GetComponent<SkillEffect>();
        skillEffect.Init(this, selectorResult.targets[0].transform);
        effectInstances.Add(skillEffect);
    }

    public Transform GetCastPoint()
    {
        return constructor.skillCastPoints[curCastPointIndex];
    }

    public virtual void Effect()
    {
        //生成技能特效弹道
        if (effectPrefab != null)
        {
            InstanceEffectFunc();
        }
        else  //无特效弹道
        {
            DirectEffectFunc();
        }
    }

    public virtual void DirectEffect()
    {
        if (selectorResult.targets.Count == 0)
            return;
        if (selectorResult.targets != null)
            foreach (ChampionController C in selectorResult.targets)
            {
                if (!C.isDead)
                {
                    AddBuffToTargetFunc(C);
                    AddDMGToTargetFunc(C);
                }
            }
        if (selectorResult.mapGrids != null)
            foreach (GridInfo G in selectorResult.mapGrids)
            {
                G.ApplyEffect(skillData.hexEffectPrefab);
            }
    }

    public virtual void AddBuffToTarget(ChampionController target)
    {
        foreach (int buff_ID in skillData.addBuffs)
        {
            if (buff_ID != 0)
                target.buffController.AddBuff(buff_ID, owner);
        }
    }

    public virtual void AddDMGToTarget(ChampionController target)
    {
        if (!string.IsNullOrEmpty(skillData.damageData[0].type))
        {
            owner.TakeDamage(target, skillData.damageData);
        }
    }

    public virtual void OnCastingUpdate()
    {
        curTime += Time.deltaTime;
        curIntervalTime -= Time.deltaTime;

        if (IsFinishFunc())
        {
            OnFinishFunc();
        }

        if (curIntervalTime <= 0 && curEffectCount < skillData.effectiveTimes)
        {
            curIntervalTime = intervalTime;
            curEffectCount++;
            EffectFunc();
        }
    }

    public virtual bool IsFinish()
    {
        return (curTime >= skillData.duration) || (selectorResult.targets[0] == null ? selectorResult.targets[0].isDead : false);
    }

    public virtual void DestroyEffect()
    {
        if (effectInstances.Count > 0)
        {
            foreach (var e in effectInstances)
            {
                if (e != null)
                    e.DestroySelf();
            }
            effectInstances.Clear();
        }
    }

    public virtual void OnFinish()
    {
        state = SkillState.CD;
        PlayEndAnimFunc();
    }


    public virtual void Reset()
    {
        state = SkillState.CD;
        curTime = 0;
        curIntervalTime = 0;
        curEffectCount = 0;
        selectorResult.Clear();
        countRemain = skillData.usableCount;
    }

    public virtual void PlayCastAnim()
    {

        if (!string.IsNullOrEmpty(skillData.skillAnimTrigger[0].constructorType))
        {
            //触发动画
            foreach (var animTrigger in skillData.skillAnimTrigger)
            {
                foreach (var c in constructor.GetAllParentConstructors(true))
                {
                    if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                    {
                        c.enablePlayNewSkillAnim = false;
                        c.animator.SetTrigger(animTrigger.trigger);
                    }
                }
            }
        }
    }

    public virtual void PlayEndAnim()
    {
        //绑定动画结束时间
        if (!string.IsNullOrEmpty(skillData.skillAnimTrigger[0].constructorType))
        {
            //触发动画
            foreach (var animTrigger in skillData.skillAnimTrigger)
            {
                foreach (var c in constructor.GetAllParentConstructors(true))
                {
                    if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                    {
                        if (HasParameter(c.animator, "EndCasting"))
                            c.animator.SetTrigger("EndCasting");
                    }
                }
            }
        }
    }

    protected bool HasParameter(Animator animator, string parameterName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == parameterName)
                return true;
        }

        return false;
    }

    public void GetDecorator(string decoratorName)
    {
        Type type = Type.GetType(decoratorName);
        SkillDecorator skillDecorator = (SkillDecorator)Activator.CreateInstance(type);
        skillDecorators.Add(skillDecorator);
    }
}


