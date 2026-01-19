using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls Arrows, Spells movement from point A to B
/// </summary>
public class TrackProjectileEffect : ColProjectileEffect
{
    public float trackDuration = 1;
    protected bool isTrack;


    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        isTrack = false;
    }


    protected override void ParabolaMoving()
    {
        base.ParabolaMoving();

        if (Vector3.Distance(oringinTarget, target.position) <= 3f || curTime > trackDuration)
        {
            isTrack = true;
        }
    }

    protected virtual void TrackMoving()
    {
        Vector3 dir = oringinTarget - transform.position;
        Vector3 lerpedDir = Vector3.Lerp(transform.forward, dir, 0.8f * Time.deltaTime);
        Quaternion rotation = Quaternion.LookRotation(lerpedDir, Vector3.up);
        this.transform.rotation = rotation;

        transform.Translate(transform.forward * speed * Time.fixedDeltaTime, Space.World);
        //PointedAtTarget(target.position + new Vector3(0, 1.5f, 0));
        //transform.position = Vector3.MoveTowards(this.transform.position, target.position + new Vector3(0, 1.5f, 0), speed * Time.deltaTime);
    }

    protected override void OnMovingUpdate()
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
            else if (curTime > duration + 5)
            {
                isMoving = false;
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void OnReached()
    {
        isMoving = false;
        OnHitEffect(target.GetComponent<ChampionController>());
        InstantiateHitEffect(transform.position);
        Destroy(gameObject);
    }

}
