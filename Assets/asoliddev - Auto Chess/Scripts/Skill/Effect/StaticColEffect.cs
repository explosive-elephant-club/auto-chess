using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticColEffect : SkillEffect
{
    protected List<ChampionController> collidedTargets;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        collidedTargets = new List<ChampionController>();

        InstantiateEmitEffect();
        //PointedAtTarget();
    }


    void Update()
    {
        if (curTime >= duration)
        {
            DestroySelf();
        }
    }


    protected override void OnCollideChampionBegin(ChampionController c)
    {
        collidedTargets.Add(c);
        skill.targets = collidedTargets;
    }

    protected override void OnCollideChampionEnd(ChampionController c)
    {
        if (collidedTargets.Contains(c))
        {
            collidedTargets.Remove(c);
            skill.targets = collidedTargets;
        }
    }

}
