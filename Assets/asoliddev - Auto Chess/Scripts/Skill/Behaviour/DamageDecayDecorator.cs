using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDecayDecorator : SkillDecorator
{
    public float damageDecay;
    public override void Init()
    {
        damageDecay = float.Parse(GetParam("damageDecay"));
        skill.AddDMGToTargetFunc = AddDMGToTarget;
    }


    public override void AddDMGToTarget(ChampionController target)
    {
        if (!string.IsNullOrEmpty(skill.skillData.damageData[0].type))
        {
            if (skill.selectorResult.targets[0] == target)
                skill.owner.TakeDamage(target, skill.skillData.damageData, 1);
            else
            {
                int dis = skill.selectorResult.targets[0].GetDistance(target);
                skill.owner.TakeDamage(target, skill.skillData.damageData, Mathf.Pow(damageDecay, dis));
            }
        }
    }
}
