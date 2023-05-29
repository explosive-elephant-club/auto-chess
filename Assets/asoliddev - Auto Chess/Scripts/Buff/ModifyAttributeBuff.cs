using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using ExcelConfig;

using System;

public class ModifyAttributeBuff : Buff
{
    public ModifyAttributeBuffData modifyAttributeData;
    public BuffStateContainer buffStateContainer;

    public ModifyAttributeBuff(BaseBuffData _buffData, ModifyAttributeBuffData _modifyAttributeData, GameObject _owner, GameObject _caster = null) :
    base(_buffData, _owner, _caster)
    {
        modifyAttributeData = _modifyAttributeData;
        buffStateContainer = new BuffStateContainer(
            new BuffStateBoolValues(modifyAttributeData.immovable,
                modifyAttributeData.disarm,
                modifyAttributeData.silence,
                modifyAttributeData.invincible,
                modifyAttributeData.invisible)
            );
    }

    public override void BuffActive()
    {
        buffController.AddBuffState(this);
        base.BuffActive();
    }

    public override void BuffDestroy()
    {
        buffController.CalculateAllBuffState();
        base.BuffDestroy();
    }
}