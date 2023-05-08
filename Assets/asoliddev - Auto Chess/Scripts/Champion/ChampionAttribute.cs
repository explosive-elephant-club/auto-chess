using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttributeEnum
{
    AttackDamage,
    DefenseArmor,
    AttackRange,
    AttackSpeed,
    MoveSpeed,
    MaxHealth,
    MaxMana,
    DodgeChange,
    CritChange,
    CritMultiple
}

public class ChampionAttribute
{
    //基础值
    private float baseValue;
    //线性叠加值
    private List<float> linearValue;
    //倍数叠加值
    private List<float> multipleValue;

    public AttributeEnum attribute;

    public ChampionAttribute(float baseValue, AttributeEnum attribute)
    {
        this.baseValue = baseValue;
        linearValue = new List<float>();
        multipleValue = new List<float>();
        this.attribute = attribute;
    }

    public void ChangeLinear(float value)
    {
        linearValue.Add(value);
    }

    public void ChangeMultiple(float value)
    {
        multipleValue.Add(value);
    }

    public float GetTrueLinearValue()
    {
        float trueLinearValue = 0;
        float trueMultipleValueValue = 1;
        foreach (float value in linearValue)
        {
            trueLinearValue += value;
        }
        foreach (float value in multipleValue)
        {
            trueMultipleValueValue += value;
        }
        return (baseValue + trueLinearValue) * trueMultipleValueValue;
    }

    public float GetTrueMultipleValue()
    {
        float trueLinearValue = 0;
        float trueMultipleValueValue = 1;
        foreach (float value in linearValue)
        {
            trueLinearValue += value;
        }
        foreach (float value in multipleValue)
        {
            trueMultipleValueValue *= value < 1 ? (1 - value) : 0;
        }
        return (baseValue + trueLinearValue) * (1 - trueMultipleValueValue);
    }

}
