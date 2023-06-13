using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillEffect : Projectile
{
    Skill skill;

    public virtual void Init(Skill _skill)
    {
        skill = _skill;
        base.Init(skill.targets[0].transform);
        OnReached = new UnityAction(ReachEffect);
        OnMoving = new UnityAction(OnUpdate);
    }

    public virtual void ReachEffect()
    {
        skill.Effect();
    }

    public virtual void OnUpdate()
    {

    }
}
