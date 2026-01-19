using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalovEmitADecorator : SkillDecorator
{
    public int effectCount;
    public int salovRange;
    public override void Init()
    {
        effectCount = int.Parse(GetParam("effectCount"));
        salovRange = int.Parse(GetParam("salovRange"));
        skill.InstanceEffectFunc = InstanceEffect;
    }

    public override void InstanceEffect()
    {
        SelectorResult t = skill.targetsSelector.FindTargetByRange
            (skill.selectorResult.targets[0], skill.skillRangeSelectorType, salovRange, skill.owner.team);

        int countMax = effectCount > t.targets.Count ? effectCount : t.targets.Count;
        //生成技能特效弹道
        if (skill.effectPrefab != null)
        {
            for (int i = 0; i < countMax; i++)
            {
                GameObject obj = GameObject.Instantiate(skill.effectPrefab);
                obj.transform.position = skill.GetCastPoint().position;
                obj.transform.rotation = skill.GetCastPoint().rotation;

                SkillEffect skillEffect = obj.GetComponent<SkillEffect>();
                skillEffect.Init(skill, t.targets[i].transform);
                skill.effectInstances.Add(skillEffect);
            }
        }
        else  //无特效弹道
        {
            skill.selectorResult = t;
            skill.DirectEffectFunc();
        }
    }
}
