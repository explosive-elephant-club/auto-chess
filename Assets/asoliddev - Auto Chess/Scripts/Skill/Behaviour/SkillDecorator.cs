using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;
using System;

public class SkillDecorator : Skill
{
    protected Skill skill;
    protected string[] _params;
    public bool hasDecorated = false;
    public Skill Decorate(Skill _skill)
    {
        skill = _skill;
        Init();
        hasDecorated = true;
        return this;
    }

    public virtual void Init() { }

    public virtual void GetParams(string str)
    {
        if (!string.IsNullOrEmpty(str))
            _params = str.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public string GetParam(string name)
    {
        foreach (var p in skill.skillData.paramValues)
        {
            if (p.name == name)
            {
                return p.value;
            }
        }
        return null;
    }
    /*
    public override void Init(SkillData _skillData, ChampionController _owner, ConstructorBase _constructor)
    {
        skill.Init(_skillData, _owner, _constructor);
    }

       public override void TryInstanceEffect()
    {
        skill.TryInstanceEffect();
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
