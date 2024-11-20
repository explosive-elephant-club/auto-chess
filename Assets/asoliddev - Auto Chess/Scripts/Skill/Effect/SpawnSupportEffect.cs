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
        GridInfo grid = Map.Instance.GetEmptySlot(skill.owner.occupyGridInfo, 2);
        transform.position = grid.transform.position + new Vector3(0, 2, 0);
        InstantiateEmitEffect();
        if (grid != null)
            skill.manager.AddSupportChampionToBattle(grid, supportName, skillIDs);
        Destroy(gameObject, destroyDelay);
    }
}
