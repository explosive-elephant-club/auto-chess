using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnBuffBehaviour : BuffBehaviour
{
    int maxLayer = 10;
    int baseDmg = 3;
    int superposeDmg = 2;

    public override void BuffStart()
    {

    }
    public override void BuffActive()
    {

        int dmg = 0;
        if (buff.curLayer <= maxLayer)
        {
            dmg = baseDmg + buff.curLayer * superposeDmg;
        }
        else
        {
            dmg = baseDmg + maxLayer * superposeDmg;
        }
        Debug.Log("BurnBuffBehaviour BuffActive " + dmg);
        buff.owner.OnGotHit(dmg, DamageType.Pure);
    }
}
