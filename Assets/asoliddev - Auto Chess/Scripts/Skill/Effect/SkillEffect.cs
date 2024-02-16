using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillEffect : MonoBehaviour
{
    protected Skill skill;
    //飞行时间
    protected float curTime;
    public virtual void Init(Skill _skill)
    {
        skill = _skill;
        curTime = 0;
    }

    protected virtual void FixedUpdate()
    {
        curTime += Time.fixedDeltaTime;
    }
    
    public virtual void DestroySelf()
    {
        Destroy(gameObject);
    }
    
}
