using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColProjectileEffect : TrackProjectileEffect
{
    protected override void OnMovingUpdate()
    {
        if (isMoving)
        {
            ParabolaMoving();
            if (curTime > duration + 5)
            {
                isMoving = false;
                Destroy(this.gameObject);
            }
        }
    }

    protected override void OnCollideChampionBegin(ChampionController c)
    {
        OnEffect(c);
    }

    public virtual void OnEffect(ChampionController target)
    {
        if (!target.isDead)
        {
            //buff
            foreach (int buff_ID in skill.skillData.addBuffs)
            {
                if (buff_ID != 0)
                    target.buffController.AddBuff(buff_ID, skill.owner);
            }
            //伤害
            if (!string.IsNullOrEmpty(skill.skillData.damageData[0].type))
            {
                skill.owner.TakeDamage(target, skill.skillData.damageData);
            }
        }
        Destroy(gameObject);
    }
}
