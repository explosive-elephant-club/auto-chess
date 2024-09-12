using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColProjectileEffect : SkillEffect
{
    ///初速度
    public float speed;
    //重力加速度
    public float g;
    protected bool isMoving = false;

    //初始速度向量
    protected Vector3 initialVelocity;
    //重力向量
    protected Vector3 Gravity;

    //初始目标点
    protected Vector3 oringinTarget;


    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        oringinTarget = target.position + new Vector3(0, 1.5f, 0);

        isMoving = true;

        CaculateInitialVelocity();

        InstantiateEmitEffect();
        PointedAtTarget(oringinTarget);
    }

    public virtual void CaculateInitialVelocity()
    {
        duration = Vector3.Distance(oringinTarget, transform.position) / speed;
        initialVelocity = new Vector3(
           (oringinTarget.x - transform.position.x) / duration,
           (oringinTarget.y - transform.position.y) / duration - 0.5f * g * duration,
            (oringinTarget.z - transform.position.z) / duration);
        Gravity = Vector3.zero;
    }


    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        OnMovingUpdate();
    }

    protected void PointedAtVelocityDir()
    {
        Vector3 arrowDir = initialVelocity + Gravity;
        Quaternion rotation = Quaternion.LookRotation(arrowDir, Vector3.up);
        this.transform.rotation = rotation;
    }

    protected virtual void ParabolaMoving()
    {
        PointedAtVelocityDir();
        Gravity.y = g * curTime;//v=at
        //模拟位移
        transform.Translate(initialVelocity * Time.fixedDeltaTime, Space.World);
        transform.Translate(Gravity * Time.fixedDeltaTime, Space.World);
    }

    protected virtual void OnMovingUpdate()
    {
        if (isMoving)
        {
            ParabolaMoving();
            if (curTime > duration + 5)
            {
                isMoving = false;
                Destroy(this.gameObject);
            }
        }
    }

    protected override void OnCollideChampionBegin(ChampionController c)
    {
        isMoving = false;
        OnHitEffect(c);
        InstantiateHitEffect(transform.position);
        Destroy(gameObject);
    }

    protected virtual void OnHitEffect(ChampionController c)
    {
        skill.selectorResult.targets = GetTargetsInRange(c);
        skill.DirectEffectFunc();
    }
}
