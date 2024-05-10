using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticColEffect : SkillEffect
{
    protected List<ChampionController> collidedTargets;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        transform.parent = skill.owner.transform;
        collidedTargets = new List<ChampionController>();

        InstantiateEmitEffect();
        //PointedAtTarget();
    }


    void Update()
    {
        if (curTime >= duration)
        {
            DestroySelf();
        }
    }
    protected override void OnCollideChampionBegin(ChampionController c)
    {
        base.OnCollideChampionBegin(c);
        skill.selectorResult.targets = hits;
    }

    protected override void OnCollideChampionEnd(ChampionController c)
    {
        base.OnCollideChampionEnd(c);
        skill.selectorResult.targets = hits;
    }

}
