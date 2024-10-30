using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamEffect : ColProjectileEffect
{
    public TrailRenderer[] trails;
    public GameObject projectile;
    public float scaleDownTime;
    protected override void OnCollideChampionBegin(ChampionController c, Vector3 colPos)
    {
        if (!hits.Contains(c))
        {
            hits.Add(c);
            OnHitEffect(c);
            InstantiateHitEffect(colPos);
            isMoving = false;
            projectile.SetActive(false);
            StartCoroutine(BeamScaleDown());
        }
    }

    protected virtual IEnumerator BeamScaleDown()
    {
        float t = 0;
        while (t < scaleDownTime)
        {
            t += Time.deltaTime;
            Debug.Log("t " + t);
            foreach (var trail in trails)
            {
                Debug.Log("widthMultiplier " + trail.widthMultiplier);
                trail.widthMultiplier -= (Time.deltaTime / scaleDownTime);
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
