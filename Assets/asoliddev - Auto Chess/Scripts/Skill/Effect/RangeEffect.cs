using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeEffect : SkillEffect
{
    public bool isAttach;
    public bool isOriginPos;
    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);

        if (skill.skillRangeSelectorType == SkillRangeSelectorType.MapHexInRange)
        {
            transform.position = skill.selectorResult.pos;
        }
        else
        {
            if (isAttach)
            {
                transform.parent = target.transform;
                if (isOriginPos)
                    transform.localPosition = Vector3.zero;
            }
        }



    }

    // Update is called once per frame
    void Update()
    {

    }
}
