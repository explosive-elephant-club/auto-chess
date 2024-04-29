using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousEmitDecorator : SkillDecorator
{
    public override void OnCastingUpdate()
    {
        curTime += Time.deltaTime;
        intervalTime += Time.deltaTime;

        if (intervalTime >= skillData.interval)
        {
            intervalTime = 0;
            TryInstanceEffect();
        }

        if (isFinish())
        {
            OnFinish();
        }
    }
}
