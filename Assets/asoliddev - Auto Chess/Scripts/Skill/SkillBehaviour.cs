using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBehaviour
{
    public virtual bool IsPrepared()
    {
        return true;
    }

    public virtual bool IsFindTarget()
    {
        return true;
    }
}
