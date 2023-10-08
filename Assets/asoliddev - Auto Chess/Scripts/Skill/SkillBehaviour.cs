using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillBehaviour : MonoBehaviour
{
    public Skill skill;

    public virtual void Init(Skill _skill)
    {
        skill = _skill;
    }

    public virtual bool IsPrepared()
    {
        return true;
    }

    public virtual ChampionController FindTargetBySelectorType()
    {
        return null;
    }

    public virtual void FindTargetByRange()
    {
    }


    public virtual void OnCast(Transform castPoint)
    {

        //绑定动画结束时间
        if (!string.IsNullOrEmpty(skill.skillData.skillAnimTrigger[0].constructorType))
        {
            skill.constructor.onSkillAnimEffect = new UnityAction(() =>
            {
                skill.InstanceEffect();
            });
            skill.constructor.onSkillAnimFinish = new UnityAction(() =>
            {
                skill.OnFinish();
            });
            //触发动画
            foreach (var animTrigger in skill.skillData.skillAnimTrigger)
            {
                foreach (var c in skill.constructor.GetAllParentConstructors(true))
                {
                    if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                    {
                        c.enablePlayNewSkillAnim = false;
                        c.animator.SetTrigger(animTrigger.trigger);
                    }
                }
            }

        }
        else//直接结束
        {
            skill.InstanceEffect();
            skill.OnFinish();
        }
    }

    public virtual void OnEffect()
    {
        foreach (ChampionController C in skill.targets)
        {
            if (!C.isDead)
            {
                //buff
                foreach (int buff_ID in skill.skillData.addBuffs)
                {
                    if (buff_ID != 0)
                        C.buffController.AddBuff(buff_ID, skill.owner);
                }
                //伤害
                if (!string.IsNullOrEmpty(skill.skillData.damageData[0].type))
                {
                    skill.owner.TakeDamage(C, skill.skillData.damageData);
                }
            }
        }
        foreach (GridInfo G in skill.mapGrids)
        {
            G.ApplyEffect(skill.skillData.hexEffectPrefab);
        }
    }

    public virtual void OnCastingUpdate()
    {

    }

    public virtual void OnFinish()
    {
    }
}
