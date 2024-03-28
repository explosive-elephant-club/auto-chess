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
    protected Transform target;
    protected bool isMoving = false;


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

        duration = Vector3.Distance(oringinTarget, transform.position) / speed;
        dir = new Vector3(
           (oringinTarget.x - transform.position.x) / duration,
           (oringinTarget.y - transform.position.y) / duration - 0.5f * g * duration,
            (oringinTarget.z - transform.position.z) / duration);
        Gravity = Vector3.zero;


        isTrack = false;

        InstantiateEmitEffect();
        PointedAtTarget(target.position + new Vector3(0, 1.5f, 0));
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        OnMovingUpdate();
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
        PointedAtTarget(target.position + new Vector3(0, 1.5f, 0));
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
            else if (curTime > duration + 2)
            {
                isMoving = false;
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void OnReached()
    {
        skill.Effect();
        InstantiateHitEffect(transform.position);
        DestroySelf();
    }

}
