using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

using System;

public class ModifyAttributeBuff : Buff
{
    public BuffStateContainer buffStateContainer;

    public ModifyAttributeBuff(BaseBuffData _buffData, GameObject _owner, GameObject _caster = null) :
    base(_buffData, _owner, _caster)
    {
        ModifyAttributeBuffData data = (ModifyAttributeBuffData)buffData;
        buffStateContainer = new BuffStateContainer(data.stateBoolValues);
    }

    public override void BuffActive()
    {
        owner.SendMessage("AddBuffState", this);
        base.BuffActive();
    }

    public override void BuffRemove()
    {
        owner.SendMessage("RemoveBuffState", this);
        base.BuffRemove();
    }
}