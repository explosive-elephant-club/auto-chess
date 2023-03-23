using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using System;

public class BuffState
{
    BuffStateBoolValues boolValues = new BuffStateBoolValues();
    public Dictionary<string, bool> stateDic;

    public BuffState()
    {
        stateDic = new Dictionary<string, bool>();

        Type type = typeof(BuffStateBoolValues);
        FieldInfo[] fields = type.GetFields();
        foreach (FieldInfo f in fields)
        {
            stateDic.Add(f.Name.ToString(), (bool)f.GetValue(boolValues));
        }
    }
}

public class ModifyAttributeBuff : Buff
{
    public int state;

    public ModifyAttributeBuff(BaseBuffData _buffData, GameObject _owner, GameObject _caster = null) :
    base(_buffData, _owner, _caster)
    {
        GetState();
    }

    public int GetState()
    {
        ModifyAttributeBuffData data = (ModifyAttributeBuffData)buffData;
        state =
        data.stateBoolValues.immovable ? BuffStateByteFormat.immovableState : 0 |
        (data.stateBoolValues.disarm ? BuffStateByteFormat.disarmState : 0) |
        (data.stateBoolValues.silence ? BuffStateByteFormat.silenceState : 0) |
        (data.stateBoolValues.invincible ? BuffStateByteFormat.invincibleState : 0) |
        (data.stateBoolValues.invisible ? BuffStateByteFormat.invisibleState : 0);
        return state;
    }

    public override void BuffActive()
    {

        base.BuffActive();
    }
}