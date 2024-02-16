using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls Arrows, Spells movement from point A to B
/// </summary>
public class TrackProjectileEffect : SkillEffect
{
    ///初速度
    public float speed;
    //重力加速度
    public float g;
    ///开始追踪目标的时长
    public float trackDuration = 1;

    public GameObject impactParticle;
    public GameObject projectileParticle;
    public GameObject muzzleParticle;

    public GameObject impactParticleInstance;
    public GameObject projectileParticleInstance;
    public GameObject muzzleParticleInstance;
    protected Transform target;
    protected bool isMoving = false;

    //代表从起点出发到目标点经过的时长
    protected float time = 0;
    //初始速度向量
    protected Vector3 dir;
    //重力向量
    protected Vector3 Gravity;


    //初始目标点
    protected Vector3 oringinTarget;

    protected bool isTrack;


    public override void Init(Skill _skill)
    {
        base.Init(_skill);
        target = skill.targets[0].transform;
        oringinTarget = target.position + new Vector3(0, 1.5f, 0);

        isMoving = true;

        time = Vector3.Distance(oringinTarget, transform.position) / speed;
        dir = new Vector3(
           (oringinTarget.x - transform.position.x) / time,
           (oringinTarget.y - transform.position.y) / time - 0.5f * g * time,
            (oringinTarget.z - transform.position.z) / time);
        Gravity = Vector3.zero;


        isTrack = false;

        InstantiateEffect();
        PointedAtTarget();
    }

    protected void InstantiateEffect()
    {
        projectileParticleInstance = Instantiate(projectileParticle, transform.position, transform.rotation) as GameObject;
        projectileParticleInstance.transform.parent = transform;
        if (muzzleParticle)
        {
            muzzleParticleInstance = Instantiate(muzzleParticle, transform.position, transform.rotation) as GameObject;
            muzzleParticleInstance.transform.rotation = transform.rotation;
            Destroy(muzzleParticleInstance, 1.5f); // Lifetime of muzzle effect.
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        OnMovingUpdate();
    }

    protected virtual void PointedAtTarget()
    {
        Vector3 relativePos = target.position + new Vector3(0, 1.5f, 0) - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        this.transform.rotation = rotation;
    }

    protected void PointedAtVelocityDir()
    {
        Vector3 arrowDir = dir + Gravity;
        Quaternion rotation = Quaternion.LookRotation(arrowDir, Vector3.up);
        this.transform.rotation = rotation;
    }

    protected virtual void ParabolaMoving()
    {
        PointedAtVelocityDir();
        Gravity.y = g * curTime;//v=at
        //模拟位移
        transform.Translate(dir * Time.fixedDeltaTime, Space.World);
        transform.Translate(Gravity * Time.fixedDeltaTime, Space.World);

        if (Vector3.Distance(oringinTarget, target.position) <= 3f || curTime > trackDuration)
        {
            isTrack = true;
        }
    }

    protected void TrackMoving()
    {
        PointedAtTarget();
        transform.position = Vector3.MoveTowards(this.transform.position, target.position + new Vector3(0, 1.5f, 0), speed * Time.deltaTime);
    }

    protected virtual void OnMovingUpdate()
    {
        if (isMoving)
        {
            if (target == null)
            {
                Destroy(this.gameObject);
                return;
            }
            if (!isTrack)
            {
                ParabolaMoving();
            }
            else
            {
                TrackMoving();
            }

            float distance = Vector3.Distance(this.transform.position, target.position + new Vector3(0, 1.5f, 0));
            if (distance < 0.2f)
            {
                this.transform.parent = target;
                isMoving = false;
                OnReached();
            }
            else if (curTime > time + 2)
            {
                isMoving = false;
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void OnReached()
    {
        skill.Effect();
        if (impactParticle != null)
            impactParticleInstance = Instantiate(impactParticle, transform.position, Quaternion.FromToRotation(Vector3.up, Vector3.zero)) as GameObject;
        Destroy(projectileParticleInstance, 3f);
        if (impactParticleInstance != null)
            Destroy(impactParticleInstance, 5f);
        DestroySelf();
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
