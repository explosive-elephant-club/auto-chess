using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using ExcelConfig;
using System;
using General;

public class ValueOperation
{
    public float value;
    public string valueName;
    UnityAction operate;
    UnityAction reset;

    public ValueOperation(string code)
    {
        string[] element = code.Split(' ');
        valueName = element[0];
        value = float.Parse(element[2]);
        if (valueName == "curHealth" || valueName == "curMana")
        {
            float attribute = (float)GeneralMethod.GetValueByName("ChampionAttributesController", valueName);
            switch (element[1])
            {
                case "+":
                    operate = new UnityAction(() =>
                    {
                        attribute += value;
                    });
                    break;
                case "*":
                    operate = new UnityAction(() =>
                    {
                        attribute *= value;
                    });
                    break;
            }
        }
        else
        {
            ChampionAttribute attribute = (ChampionAttribute)GeneralMethod.GetValueByName("ChampionAttributesController", valueName);
            switch (element[1])
            {
                case "+":
                    operate = new UnityAction(() =>
                    {
                        attribute.AddLinear(value);
                    });
                    reset = new UnityAction(() =>
                    {
                        attribute.RemoveLinear(value);
                    });
                    break;
                case "*":
                    operate = new UnityAction(() =>
                    {
                        attribute.AddMultiple(value);
                    });
                    reset = new UnityAction(() =>
                    {
                        attribute.RemoveMultiple(value);
                    });
                    break;
            }
        }

    }
}

public class ModifyAttributeBuff : Buff
{
    public ModifyAttributeBuffData modifyAttributeData;
    public BuffStateContainer buffStateContainer;
    public ValueOperation[] valueOperations;

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
        if (!string.IsNullOrEmpty(modifyAttributeData.valueChanges[0]))
        {
            valueOperations = new ValueOperation[modifyAttributeData.valueChanges.Length];
            for (int i = 0; i < valueOperations.Length; i++)
            {
                valueOperations[i] = new ValueOperation(modifyAttributeData.valueChanges[i]);
            }
        }
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