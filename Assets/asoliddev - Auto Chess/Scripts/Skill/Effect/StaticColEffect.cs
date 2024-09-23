using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticColEffect : SkillEffect
{
    public bool isAttach;
    protected List<ChampionController> collidedTargets;

    float t = 0;
    float intervel = 0;
    int effectEffectiveTimes;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        if (isAttach)
            transform.parent = skill.owner.transform;
        collidedTargets = new List<ChampionController>();
        effectEffectiveTimes = int.Parse(GetParam("effectEffectiveTimes"));
        intervel = duration / effectEffectiveTimes;
        InstantiateEmitEffect();
        //PointedAtTarget();
    }


    void Update()
    {
        if (t < intervel)
        {
            t += Time.deltaTime;
        }
        else
        {
            t = 0;
            skill.DirectEffectFunc();
        }
        if (curTime >= duration)
        {
            Destroy(gameObject);
        }
    }

    protected override void OnCollideShieldBegin(Collider hit)
    {
        InstantiateHitEffect(hit.bounds.ClosestPoint(transform.position));
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
