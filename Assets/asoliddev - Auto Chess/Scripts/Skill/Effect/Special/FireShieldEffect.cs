using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireShieldEffect : VoidShieldEffect
{
    float t = 0;
    float intervel = 1;

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
            AddFireDmg();
        }
    }

    void AddFireDmg()
    {
        List<ChampionController> targets =
                        skill.targetsSelector.FindTargetByRange(skill.owner, skill.skillRangeSelectorType, skill.skillData.range, skill.owner.team).targets;
        foreach (var target in targets)
        {
            skill.AddDMGToTarget(target);
        }
    }

}
