using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillEffect : MonoBehaviour
{
    protected Skill skill;
    //持续时间
    protected float duration;
    //已存在时间
    protected float curTime;

    protected Transform target;
    protected List<ChampionController> hits;


    public GameObject hitParticleInstance;
    public GameObject emitParticleInstance;
    public virtual void Init(Skill _skill, Transform _target)
    {
        skill = _skill;
        target = _target;
        curTime = 0;
        duration = skill.skillData.duration;
        hits = new List<ChampionController>();
    }

    protected virtual void FixedUpdate()
    {
        curTime += Time.fixedDeltaTime;
    }

    public virtual void DestroySelf()
    {
        if (hitParticleInstance != null)
            Destroy(hitParticleInstance);
        if (emitParticleInstance != null)
            Destroy(emitParticleInstance);
        Destroy(gameObject);
    }

    protected virtual void InstantiateEmitEffect()
    {
        if (skill.emitPrefab)
        {
            emitParticleInstance = Instantiate(skill.emitPrefab, transform.position, transform.rotation) as GameObject;
            emitParticleInstance.transform.rotation = transform.rotation;
            Destroy(emitParticleInstance, 1.5f); // Lifetime of muzzle effect.
        }
    }

    protected virtual void InstantiateHitEffect(Vector3 pos)
    {
        if (skill.hitFXPrefab != null)
        {
            hitParticleInstance = Instantiate(skill.hitFXPrefab, pos, Quaternion.FromToRotation(Vector3.up, Vector3.zero)) as GameObject;
            Destroy(hitParticleInstance, 1.5f);
        }

    }

    protected virtual void PointedAtTarget(Vector3 targetPos)
    {
        Vector3 relativePos = targetPos - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        this.transform.rotation = rotation;
    }

    protected virtual void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "Shield")
        {
            ShieldEffect shieldEffect = hit.GetComponent<ShieldEffect>();
            if (shieldEffect.skill.owner.team != skill.owner.team)
            {
                OnCollideShieldBegin(hit);
                return;
            }
        }
        ChampionController c = hit.gameObject.GetComponent<ChampionController>();
        if (c == null)
            return;

        if (skill.skillTargetType == SkillTargetType.Teammate)
        {
            if (c.team == skill.owner.team)
            {
                OnCollideChampionBegin(c);
                InstantiateHitEffect(hit.bounds.ClosestPoint(transform.position));
            }
        }
        else if (skill.skillTargetType == SkillTargetType.Enemy)
        {
            if (c.team != skill.owner.team)
            {
                OnCollideChampionBegin(c);
                InstantiateHitEffect(hit.bounds.ClosestPoint(transform.position));
            }
        }
    }

    protected virtual void OnTriggerExit(Collider hit)
    {
        ChampionController c = hit.gameObject.GetComponent<ChampionController>();
        if (c == null)
            return;

        if (skill.skillTargetType == SkillTargetType.Teammate)
        {
            if (c.team == skill.owner.team)
            {
                OnCollideChampionEnd(c);
            }
        }
        else if (skill.skillTargetType == SkillTargetType.Enemy)
        {
            if (c.team != skill.owner.team)
            {
                OnCollideChampionEnd(c);
            }
        }
    }

    protected virtual void OnCollideShieldBegin(Collider hit)
    {
        InstantiateHitEffect(hit.bounds.ClosestPoint(transform.position));
        ShieldEffect shieldEffect = hit.GetComponent<ShieldEffect>();
        shieldEffect.OnGotHit(skill.owner, skill.skillData.damageData);
        Destroy(gameObject);
    }

    protected virtual void OnCollideChampionBegin(ChampionController c)
    {
        hits.Add(c);
    }

    protected virtual void OnCollideChampionEnd(ChampionController c)
    {
        if (hits.Contains(c))
        {
            hits.Remove(c);
        }
    }

    protected virtual List<ChampionController> GetTargetsInRange(ChampionController c)
    {
        return skill.targetsSelector.FindTargetByRange(c, skill.skillRangeSelectorType, skill.skillData.range, skill.owner.team).targets;
    }

    public string GetParam(string name)
    {
        foreach (var p in skill.skillData.paramValues)
        {
            if (p.name == name)
            {
                return p.value;
            }
        }
        return null;
    }
}
