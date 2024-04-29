using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

public class SkillDecorator : Skill
{
    protected Skill skill;
    public bool hasDecorated = false;
    public Skill Decorate(Skill _skill)
    {
        skill = _skill;
        Inherit();
        hasDecorated = true;
        return this;
    }

    public void Inherit()
    {
        skillData = skill.skillData;
        curTime = skill.curTime;
        intervalTime = skill.intervalTime;
        skillTargetType = skill.skillTargetType;
        skillRangeSelectorType = skill.skillRangeSelectorType;
        skillTargetSelectorType = skill.skillTargetSelectorType;
        owner = skill.owner;
        constructor = skill.constructor;
        countRemain = skill.countRemain;
        emitPrefab = skill.emitPrefab;
        effectPrefab = skill.effectPrefab;
        hitFXPrefab = skill.hitFXPrefab;
        icon = skill.icon;
        targets = skill.targets;
        mapGrids = skill.mapGrids;
        state = skill.state;
        effectInstances = skill.effectInstances;
        skillController = skill.skillController;
        skillDecorators = skill.skillDecorators;
        manager = skill.manager;
        curCastPointIndex = skill.curCastPointIndex;
    }


    public override void TryInstanceEffect()
    {
        skill.TryInstanceEffect();
    }




    /*
    public override void Init(SkillData _skillData, ChampionController _owner, ConstructorBase _constructor)
    {
        skill.Init(_skillData, _owner, _constructor);
    }


     public override void OnCastingUpdate()
    {
        skill.OnCastingUpdate();
    }

    public override bool IsFindTarget()
    {
        return skill.IsFindTarget();
    }

    public override void FindTargetsByType()
    {
        skill.FindTargetsByType();
    }

    public override ChampionController FindTargetBySelectorType()
    {
        return skill.FindTargetBySelectorType();
    }

  public override void Cast()
    {
        skill.Cast();
    }

   

    public override void Effect()
    {
        skill.Effect();
    }

    public override void AddBuffToTarget(ChampionController target)
    {
        skill.AddBuffToTarget(target);
    }

    public override void AddDMGToTarget(ChampionController target)
    {
        skill.AddDMGToTarget(target);
    }

    public override void OnCastingUpdate()
    {
        skill.OnCastingUpdate();
    }

    public override bool isFinish()
    {
        return skill.isFinish();
    }

    public override void DestroyEffect()
    {
        skill.DestroyEffect();
    }

    public override void OnFinish()
    {
        skill.OnFinish();
    }


    public override void Reset()
    {
        skill.Reset();
    }

    public override void PlayCastAnim()
    {

        skill.PlayCastAnim();
    }

    public override void PlayEndAnim()
    {
        skill.PlayEndAnim();
    }*/

}
