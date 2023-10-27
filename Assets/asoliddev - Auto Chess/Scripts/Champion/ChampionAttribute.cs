using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Physical,
    Pure,
    Fire,
    Ice,
    Lightning,
    Acid
}

public class ChampionAttribute
{
    //基础值
    public float baseValue;
    //线性叠加值
    protected List<float> linearValue;
    //倍数叠加值
    protected List<float> multipleValue;

    public ChampionAttribute(float baseValue)
    {
        this.baseValue = baseValue;
        linearValue = new List<float>();
        multipleValue = new List<float>();
    }

    public void AddLinear(float value)
    {
        linearValue.Add(value);
    }

    public void RemoveLinear(float value)
    {
        linearValue.Remove(value);
    }

    public void AddMultiple(float value)
    {
        multipleValue.Add(value);
    }

    public void RemoveMultiple(float value)
    {
        multipleValue.Remove(value);
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

    public float GetTrueLinearValue(float baseValue2, float correction = 1)
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
        return (baseValue * correction + baseValue2 + trueLinearValue * correction) * trueMultipleValueValue;
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
