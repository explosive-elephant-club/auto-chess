using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBehaviour : MonoBehaviour
{
    public Skill skill;

    public virtual void Init(Skill _skill)
    {
        skill = _skill;
    }

    public virtual bool IsPrepared()
    {
        return true;
    }

    public virtual ChampionController FindTargetBySelectorType()
    {
        return null;
    }

    public virtual void FindTargetByRange()
    {
    }


    public virtual void OnCast(Transform castPoint)
    {
    }

    public void OnEffect()
    {

    }

    public virtual void OnCastingUpdate()
    {

    }

    public virtual void OnFinish()
    {
    }
}
