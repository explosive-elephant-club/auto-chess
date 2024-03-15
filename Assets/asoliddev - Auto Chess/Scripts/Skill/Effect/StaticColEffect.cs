using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticColEffect : SkillEffect
{
    public GameObject impactParticle;
    public GameObject projectileParticle;
    public GameObject muzzleParticle;

    public GameObject impactParticleInstance;
    public GameObject projectileParticleInstance;
    public GameObject muzzleParticleInstance;

    protected List<ChampionController> collidedTargets;

    public override void Init(Skill _skill)
    {
        base.Init(_skill);
        collidedTargets = new List<ChampionController>();

        InstantiateProjectileEffect();
        //PointedAtTarget();
    }

    protected void InstantiateProjectileEffect()
    {
        projectileParticleInstance = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticleInstance.transform.parent = transform;
        if (muzzleParticle)
        {
            muzzleParticleInstance = Instantiate(muzzleParticle, transform.position, transform.rotation) as GameObject;
            muzzleParticleInstance.transform.rotation = transform.rotation * Quaternion.Euler(180, 0, 0);
            Destroy(muzzleParticleInstance, 1.5f); // Lifetime of muzzle effect.
        }
    }

    protected void PointedAtTarget()
    {
        Vector3 relativePos = skill.targets[0].transform.position + new Vector3(0, 1.5f, 0) - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        this.transform.rotation = rotation;
    }

    protected void InstantiateImpactEffect(Vector3 pos)
    {
        if (impactParticle != null)
            impactParticleInstance = Instantiate(impactParticle, pos, Quaternion.FromToRotation(Vector3.up, Vector3.zero)) as GameObject;
        Destroy(impactParticleInstance, 1f);
    }


    void Update()
    {
        if (curTime >= skill.skillData.duration)
        {
            DestroySelf();
        }
    }


    void OnTriggerEnter(Collider hit)
    {
        ChampionController c = hit.gameObject.GetComponent<ChampionController>();
        if (c == null)
            return;
        if (!collidedTargets.Contains(c))
        {
            if (skill.skillTargetType == SkillTargetType.Teammate)
            {
                if (c.team == skill.owner.team)
                {
                    collidedTargets.Add(c);
                    skill.targets = collidedTargets;
                    InstantiateImpactEffect(hit.transform.position);
                }
            }
            else if (skill.skillTargetType == SkillTargetType.Enemy)
            {
                if (c.team != skill.owner.team)
                {
                    Debug.Log("Add Target");
                    collidedTargets.Add(c);
                    skill.targets = collidedTargets;
                    InstantiateImpactEffect(hit.transform.position);
                }
            }
        }
    }

    void OnTriggerExit(Collider hit)
    {
        ChampionController c = hit.gameObject.GetComponent<ChampionController>();
        if (collidedTargets.Contains(c))
        {
            Debug.Log("Remove Target");
            collidedTargets.Remove(c);
            skill.targets = collidedTargets;
        }
    }

    public override void DestroySelf()
    {
        Destroy(projectileParticleInstance);
        if (impactParticleInstance != null)
            Destroy(impactParticleInstance);
        if (muzzleParticleInstance != null)
            Destroy(muzzleParticleInstance);
        base.DestroySelf();

    }
}
