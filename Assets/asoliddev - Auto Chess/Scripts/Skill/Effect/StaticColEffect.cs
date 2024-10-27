using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticColEffect : SkillEffect
{
    public bool isAttach;
    public bool isSpawnOnTarget = false;
    protected List<ChampionController> collidedTargets;
    public float overrideDuration = -1f;
    public float overrideDelay = 0f;
    float t = 0;
    float intervel = 0;
    int effectEffectiveTimes;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        if (isSpawnOnTarget)
        {
            transform.position = target.position;
            if (isAttach)
                transform.parent = target;
        }
        else
        {
            if (isAttach)
                transform.parent = skill.owner.transform;
        }

        collidedTargets = new List<ChampionController>();
        effectEffectiveTimes = int.Parse(GetParam("effectEffectiveTimes"));
        duration = overrideDuration > 0 ? overrideDuration : duration;
        intervel = (duration - overrideDelay) / effectEffectiveTimes;
        InstantiateEmitEffect();
        //PointedAtTarget();
    }


    public virtual void Update()
    {
        if (curTime >= overrideDelay)
            if (t <= 0)
            {
                skill.DirectEffectFunc();
                t = intervel;
            }
            else
            {
                t -= Time.deltaTime;
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

    protected override void OnCollideChampionBegin(ChampionController c, Vector3 colPos)
    {
        if (!hits.Contains(c))
        {
            hits.Add(c);
            InstantiateHitEffect(colPos);
            skill.selectorResult.targets = hits;
        }
    }

    protected override void OnCollideChampionEnd(ChampionController c, Vector3 colPos)
    {
        if (hits.Contains(c))
        {
            hits.Remove(c);
            skill.selectorResult.targets = hits;
        }
    }

}
