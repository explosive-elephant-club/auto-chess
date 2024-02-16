using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousCastBehaviour : SkillBehaviour
{
    public override void OnCast(Transform[] castPoints, int pointIndex)
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
                        c.enablePlayNewSkillAnim = false;
                        c.animator.SetTrigger(animTrigger.trigger);
                    }
                }
            }
        }
        if (skill.skillData.duration <= 0)
            skill.OnFinish();
        else
            skill.InstanceEffect();
    }

    public override void OnCastingUpdate()
    {
        skill.curTime += Time.deltaTime;
        skill.intervalTime += Time.deltaTime;

        if (skill.intervalTime >= skill.skillData.interval)
        {
            skill.intervalTime = 0;
            skill.InstanceEffect();
        }

        if (skill.curTime >= skill.skillData.duration)
        {
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
}
