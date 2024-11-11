using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimDelayEffect : ColProjectileEffect
{
    public float delay = 0;
    public GameObject aimParticlePrefab;
    public GameObject aimParticleInstance;

    float t = 0;
    bool isEmit = false;
    public override void Init(Skill _skill, Transform _target)
    {
        skill = _skill;
        target = _target;
        curTime = 0;
        duration = skill.skillData.duration;
        hits = new List<ChampionController>();
        oringinTarget = target.position + new Vector3(0, 1.5f, 0);

        isMoving = true;
        curHit = hitConsume;
        CaculateInitialVelocity();
        PointedAtTarget(oringinTarget);


        aimParticlePrefab = Resources.Load<GameObject>("Prefab/Projectile/Skill/" + skill.skillData.ID + "/Aim");
        InstantiateAimEffect();


        delay = float.Parse(GetParam("Delay"));
        t = 0;
        isEmit = false;
    }

    protected override void FixedUpdate()
    {
        if (t < delay)
        {
            t += Time.deltaTime;
        }
        else
        {
            if (!isEmit)
            {
                InstantiateEmitEffect();
                isEmit = true;
            }
            base.FixedUpdate();
            OnMovingUpdate();
        }
    }

    protected virtual void InstantiateAimEffect()
    {
        if (aimParticlePrefab)
        {
            emitParticleInstance = Instantiate(aimParticlePrefab, transform.position, transform.rotation) as GameObject;
            emitParticleInstance.transform.rotation = transform.rotation;
            Destroy(emitParticleInstance, 1.5f); // Lifetime of muzzle effect.
        }
    }
}
