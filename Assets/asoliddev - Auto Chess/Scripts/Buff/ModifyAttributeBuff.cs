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
    public UnityAction operate;
    public UnityAction reset;

    public ValueOperation(string code, ChampionAttributesController attributesController)
    {
        string[] element = code.Split(' ');
        valueName = element[0];
        value = float.Parse(element[2]);

        ChampionAttribute attribute = (ChampionAttribute)GeneralMethod.GetValueByName(attributesController, valueName);
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
            case "-":
                operate = new UnityAction(() =>
                {
                    attribute.AddLinear(-value);
                });
                reset = new UnityAction(() =>
                {
                    attribute.RemoveLinear(-value);
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

public class ModifyAttributeBuff : Buff
{
    public ModifyAttributeBuffData modifyAttributeData;
    public BuffStateContainer buffStateContainer;
    public List<ValueOperation> valueOperations = new List<ValueOperation>();

    public ModifyAttributeBuff(BaseBuffData _buffData, ModifyAttributeBuffData _modifyAttributeData, ChampionController _owner, ChampionController _caster = null) :
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
            //valueOperations = new ValueOperation[modifyAttributeData.valueChanges.Length];
            for (int i = 0; i < valueOperations.Count; i++)
            {
                valueOperations.Add(new ValueOperation(modifyAttributeData.valueChanges[i],
                    _owner.attributesController));
            }
        }
    }

    public override void BuffActive()
    {
        buffController.AddBuffState(this);

        foreach (ValueOperation operation in valueOperations)
        {
            operation.operate.Invoke();
        }
        base.BuffActive();
    }

    public override void BuffDestroy()
    {
        buffController.CalculateAllBuffState();
        foreach (ValueOperation operation in valueOperations)
        {
            operation.reset.Invoke();
        }
        base.BuffDestroy();
    }
}