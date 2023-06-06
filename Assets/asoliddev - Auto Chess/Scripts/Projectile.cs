using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls Arrows, Spells movement from point A to B
/// </summary>
public class Projectile : MonoBehaviour
{
    ///初速度
    public float speed;

    //重力加速度
    public float g;

    ///Time to wait destroying this projectile after reached the target
    public float trackDuration = 5;

    public UnityAction OnReached;
    public UnityAction OnMoving;

    protected Transform target;
    protected bool isMoving = false;

    //代表从起点出发到目标点经过的时长
    protected float time = 0;
    //初始速度向量
    protected Vector3 dir;
    //重力向量
    protected Vector3 Gravity;
    //飞行时间
    protected float curTime;

    //初始目标点
    protected Vector3 oringinTarget;

    protected bool isTrack;

    /// <summary>
    /// Called when projectile created
    /// </summary>
    /// <param name="_target"></param>
    public virtual void Init(Transform _target)
    {
        target = _target;
        oringinTarget = target.position + new Vector3(0, 1.5f, 0);

        isMoving = true;

        time = Vector3.Distance(oringinTarget, transform.position) / speed;
        dir = new Vector3(
           (oringinTarget.x - transform.position.x) / time,
           (oringinTarget.y - transform.position.y) / time - 0.5f * g * time,
            (oringinTarget.z - transform.position.z) / time);
        Gravity = Vector3.zero;
        curTime = 0;

        isTrack = false;
        PointedAtTarget();
    }

    /// Update is called once per frame
    void Update()
    {
        OnMovingUpdate();
    }

    protected void PointedAtTarget()
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

    protected void ParabolaMoving()
    {
        PointedAtVelocityDir();
        Gravity.y = g * (curTime += Time.fixedDeltaTime);//v=at
        //模拟位移
        transform.Translate(dir * Time.fixedDeltaTime, Space.World);
        transform.Translate(Gravity * Time.fixedDeltaTime, Space.World);

        if (Vector3.Distance(oringinTarget, target.position) <= 0.2f)
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
            if (OnMoving != null)
                OnMoving.Invoke();

            float distance = Vector3.Distance(this.transform.position, target.position + new Vector3(0, 1.5f, 0));
            if (distance < 0.2f)
            {
                if (OnReached != null)
                    OnReached.Invoke();
                this.transform.parent = target;
                isMoving = false;
                Destroy(this.gameObject, .5f);
            }
            else if (curTime > time + trackDuration)
            {
                isMoving = false;
                Destroy(this.gameObject);
            }
        }
    }
}
