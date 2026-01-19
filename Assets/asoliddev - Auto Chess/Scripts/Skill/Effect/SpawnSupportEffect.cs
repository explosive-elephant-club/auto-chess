using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnSupportEffect : SkillEffect
{
    public string supportName;
    public int[] skillIDs;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        InstanceSupport();
    }

    protected virtual void InstanceSupport()
    {
        Vector3 pos = Vector3.zero;
        if (skill.owner.container.GetEmptyPos(5, out pos))
        {
            transform.position = pos + new Vector3(0, 2, 0);
            InstantiateEmitEffect();
            skill.manager.AddSupportChampionToBattle(pos, supportName, skillIDs);
        }
        Destroy(gameObject, destroyDelay);
    }
}
