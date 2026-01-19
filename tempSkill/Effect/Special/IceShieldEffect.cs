using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceShieldEffect : VoidShieldEffect
{
    float t = 0;
    float intervel = 0.9f;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
    }

    // Update is called once per frame
    void Update()
    {
        if (t < intervel)
        {
            t += Time.deltaTime;
        }
        else
        {
            t = 0;
            AddDebuff();
        }
    }

    void AddDebuff()
    {
        List<ChampionController> targets =
               skill.targetsSelector.FindTargetByRange(skill.owner, skill.skillRangeSelectorType, skill.skillData.range, skill.owner.team).targets;
        foreach (var target in targets)
        {
            skill.AddBuffToTarget(target);
        }
    }

}
