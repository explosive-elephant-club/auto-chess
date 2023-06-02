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

    public virtual void OnCast(Transform castPoint)
    {
    }


    public virtual void OnFinish()
    {
    }
}
