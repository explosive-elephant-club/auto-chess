using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeEffect : ColProjectileEffect
{
    public float explosionRadius = 5;
    // Start is called before the first frame update
    protected override void OnMovingUpdate()
    {
        if (isMoving)
        {
            ParabolaMoving();
            if (curTime > duration + .1f)
            {
                isMoving = false;
                InstantiateHitEffect(transform.position + Vector3.up * 0.5f);
                Explosion();
            }
        }
        else
        {
            if (curTime > duration + .3f)
            {
                skill.DirectEffectFunc();
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void Explosion()
    {
        Debug.Log("OnCollideChampionBegin Explosion");
        SphereCollider col = GetComponent<SphereCollider>();
        col.radius = explosionRadius;
    }

    protected override void OnCollideChampionBegin(ChampionController c, Vector3 colPos)
    {
        if (!hits.Contains(c))
        {
            hits.Add(c);
            if (isMoving)
            {
                isMoving = false;
                Explosion();
                InstantiateHitEffect(colPos);
                curTime = duration + .5f;
            }
        }
    }
}
