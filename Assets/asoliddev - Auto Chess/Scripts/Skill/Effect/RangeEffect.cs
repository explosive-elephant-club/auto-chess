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
        Transform t;
        if (skill.skillRangeSelectorType == SkillRangeSelectorType.MapHexInRange)
        {
            t = skill.selectorResult.mapGrids[0].transform;
        }
        else
        {
            t = target;
        }

        if (isAttach)
        {
            transform.parent = t.transform;
            if (isOriginPos)
                transform.localPosition = Vector3.zero;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
