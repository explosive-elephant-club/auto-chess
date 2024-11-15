using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultProjectileEffect : ColProjectileEffect
{
    public int catapultRange = 2;
    public bool isBacktrack = false;
    protected int curCatapult = 0;
    bool isFirst = true;
    List<ChampionController> catapultedTargets = new List<ChampionController>();
    protected override void OnCollideChampionBegin(ChampionController c, Vector3 colPos)
    {
        if (!hits.Contains(c))
        {
            hits.Add(c);
            OnHitEffect(c);
            InstantiateHitEffect(colPos);
            curHit--;
            if (curHit <= 0)
            {
                isMoving = false;
                Destroy(gameObject, destroyDelay);
            }
            else
            {
                ChooseNewTarget();
            }
        }
    }

    void ChooseNewTarget()
    {
        if (isBacktrack || isFirst)
        {
            isFirst = false;
            catapultedTargets.Add(target.GetComponent<ChampionController>());
        }

        List<ChampionController> targets =
                       skill.targetsSelector.FindTargetByRange(skill.owner, skill.skillRangeSelectorType, catapultRange, skill.owner.team).targets;
        if (targets.Count > 0)
        {
            int i = 0;
            int count = 0;
            do
            {
                count++;
                i = Random.Range(0, targets.Count);
            } while (!catapultedTargets.Contains(targets[i]) || count > targets.Count);
            oringinTarget = targets[i].transform.position + new Vector3(0, 1.5f, 0);
            CaculateInitialVelocity();
            PointedAtTarget(oringinTarget);
        }
        else
        {
            curHit = 0;
            isMoving = false;
            Destroy(gameObject, destroyDelay);
        }


    }
}
