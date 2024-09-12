using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTargetDecorator : SkillDecorator
{
    public override void Init()
    {
        skill.InstanceEffectFunc = InstanceEffect;
    }

    public override void InstanceEffect()
    {
        //生成技能特效弹道
        if (skill.effectPrefab != null)
        {
            GameObject obj = GameObject.Instantiate(skill.effectPrefab);
            obj.transform.localPosition = skill.GetCastPoint().position;
            obj.transform.localRotation = skill.GetCastPoint().rotation;

            SkillEffect skillEffect = obj.GetComponent<SkillEffect>();
            skillEffect.Init(skill, skill.selectorResult.targets[Random.Range(0, skill.selectorResult.targets.Count)].transform);
            skill.effectInstances.Add(skillEffect);

        }
        else  //无特效弹道
        {
            skill.DirectEffectFunc();
        }
    }
}
