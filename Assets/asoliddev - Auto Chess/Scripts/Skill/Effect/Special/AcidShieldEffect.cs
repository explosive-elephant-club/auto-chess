using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

public class AcidShieldEffect : VoidShieldEffect
{
    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);

    }

    public override void BrokenDestroy()
    {
        base.BrokenDestroy();
        List<ChampionController> targets =
                        skill.targetsSelector.FindTargetByRange(skill.owner, skill.skillRangeSelectorType, skill.skillData.range, skill.owner.team).targets;
        SkillData.damageDataClass damageData = new SkillData.damageDataClass();
        damageData.dmg = (int)(maxMech / targets.Count);
        damageData.correction = 2;
        damageData.type = "Acid";
        skill.skillData.damageData[0] = damageData;
        foreach (var target in targets)
        {
            skill.AddDMGToTarget(target);
        }
    }
}
