using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverShield : VoidShieldEffect
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
            ShieldRecover();
        }
    }

    void ShieldRecover()
    {
        int recoverValue = (int)(maxMech * .1f);
        if (curMech < maxMech)
        {
            if (curMech + recoverValue > maxMech)
            {
                curMech = maxMech;
            }
            else
            {
                curMech = curMech + recoverValue;
            }
        }
    }
}
