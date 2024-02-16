using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillBehaviour
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


    public virtual void OnCast(Transform[] castPoints, int pointIndex)
    {
        skill.InstanceEffect();

        //绑定动画结束时间
        if (!string.IsNullOrEmpty(skill.skillData.skillAnimTrigger[0].constructorType))
        {
            /* skill.constructor.onSkillAnimEffect = new UnityAction(() =>
             {
                 skill.InstanceEffect();
             });
             skill.constructor.onSkillAnimFinish = new UnityAction(() =>
             {
                 skill.OnFinish();
             });
             */
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
        if (skill.skillData.duration <= 0)
            skill.OnFinish();
        else
            skill.Effect();

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
        skill.curTime += Time.deltaTime;
        skill.intervalTime += Time.deltaTime;

        if (skill.targets.Count == 0)
            return;

        if (skill.intervalTime >= skill.skillData.interval)
        {
            skill.intervalTime = 0;
            skill.Effect();
        }

        if (skill.curTime >= skill.skillData.duration)
        {
            DestroyEffect();
            skill.OnFinish();
        }

        if (skill.targets[0] == null)
        {
            DestroyEffect();
            skill.OnFinish();
        }
        else if (skill.targets[0].isDead)
        {
            DestroyEffect();
            skill.OnFinish();
        }
    }

    public virtual void DestroyEffect()
    {
        if (skill.effectScript != null)
        {
            skill.effectScript.DestroySelf();
        }
    }

    public virtual void OnFinish()
    {
        //绑定动画结束时间
        if (!string.IsNullOrEmpty(skill.skillData.skillAnimTrigger[0].constructorType))
        {
            //触发动画
            foreach (var animTrigger in skill.skillData.skillAnimTrigger)
            {
                foreach (var c in skill.constructor.GetAllParentConstructors(true))
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

    bool HasParameter(Animator animator, string parameterName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == parameterName)
                return true;
        }

        return false;
    }
}
