using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;
using System;

/// <summary>
/// 技能修饰器
/// </summary>
public class SkillDecorator : Skill
{
    protected Skill skill;
    /// <summary>
    /// 是否已经修饰过技能
    /// </summary>
    public bool hasDecorated = false;

    /// <summary>
    /// 修饰目标技能
    /// </summary>
    /// <param name="_skill">目标技能</param>
    /// <returns>修饰过的技能</returns>
    public Skill Decorate(Skill _skill)
    {
        skill = _skill;
        Init();
        hasDecorated = true;
        return this;
    }

    public virtual void Init() { }
    
    /// <summary>
    /// 通过变量的名称在配置数据中获取变量的值
    /// </summary>
    /// <param name="name">变量的名称</param>
    /// <returns>变量的值</returns>
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
