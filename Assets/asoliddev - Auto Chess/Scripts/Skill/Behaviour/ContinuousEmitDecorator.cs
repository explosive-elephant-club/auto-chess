using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousEmitDecorator : SkillDecorator
{
    public override void Init()
    {
        skill.OnCastingUpdateFunc = OnCastingUpdate;
    }

    public override void OnCastingUpdate()
    {
        skill.curTime += Time.deltaTime;
        skill.intervalTime += Time.deltaTime;

        if (skill.IsFinishFunc())
        {
            skill.OnFinishFunc();
        }

        if (skill.intervalTime >= skill.skillData.interval)
        {
            skill.intervalTime = 0;
            skill.TryInstanceEffectFunc();
        }
    }
}
