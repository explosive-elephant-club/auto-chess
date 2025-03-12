using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DamageType
{
    Physical,
    Pure,
    Fire,
    Ice,
    Lightning,
    Acid
}

public enum AttributeFormat
{
    Int,
    Float2,
    Percentage
}

public class Resistance
{
    public int layerMax;
    public float baseValue;
    public float superposeValue;
    public UnityAction callAction;

    public int curLayer;
    public float curValue;

    float recoverIntervel = 3;
    public Resistance(int _layerMax, float _baseValue, float _superposeValue)
    {
        layerMax = _layerMax;
        baseValue = _baseValue;
        superposeValue = _superposeValue;

        curLayer = 0;
        curValue = 0;
    }

    public void OnGetHit(float dmg)
    {
        recoverIntervel = 3;
        if (curValue + dmg < baseValue + superposeValue * curLayer)
        {
            curValue += dmg;
        }
        else
        {
            curLayer = curLayer < layerMax ? curLayer + 1 : layerMax;
            callAction.Invoke();
            curValue = 0;
        }
    }

    public void Recover()
    {
        if (recoverIntervel > 0)
        {
            recoverIntervel -= Time.deltaTime;
        }
        else
        {
            recoverIntervel = 0;
            if (curValue > 0)
            {
                curValue -= Time.deltaTime * 20f;
            }
            else
            {
                if (curLayer > 0)
                {
                    curLayer--;
                    curValue = baseValue + superposeValue * curLayer;
                }
                else
                {
                    curValue = 0;
                }
            }
        }
    }

    public void Reset()
    {
        curLayer = 0;
        curValue = 0;
        recoverIntervel = 0;
    }
}
public class ModifyValue
{
    public float value;
    public ValueModifySource valueModifySource;
    public ModifyValue(float value, ValueModifySource valueModifySource)
    {
        this.value = value;
        this.valueModifySource = valueModifySource;
    }
}

public class ChampionAttribute
{
    //基础值
    public float baseValue;
    //基础值
    public string attributeName;
    //线性叠加值
    protected List<ModifyValue> linearValue;
    //倍数叠加值
    protected List<ModifyValue> multipleValue;
    //负运算
    public bool isNegative = false;
    //显示类型
    public AttributeFormat attributeFormat;

    public ChampionAttribute(float baseValue, string name, bool _isNegative, AttributeFormat _attributeFormat)
    {
        this.baseValue = baseValue;
        attributeName = name;
        isNegative = _isNegative;
        this.attributeFormat = _attributeFormat;

        linearValue = new List<ModifyValue>();
        multipleValue = new List<ModifyValue>();
    }

    public void AddLinear(float value, ValueModifySource valueModifySource)
    {
        linearValue.Add(new ModifyValue(value, valueModifySource));
    }

    public void RemoveLinear(float value, ValueModifySource valueModifySource)
    {
        linearValue.Remove(new ModifyValue(value, valueModifySource));
    }

    public void AddMultiple(float value, ValueModifySource valueModifySource)
    {
        multipleValue.Add(new ModifyValue(value, valueModifySource));
    }

    public void RemoveMultiple(float value, ValueModifySource valueModifySource)
    {
        multipleValue.Remove(new ModifyValue(value, valueModifySource));
    }

    public float GetTrueValue(float externalValue = 0)
    {
        float trueLinearValue = externalValue;
        float trueMultipleValueValue = 1;
        foreach (ModifyValue modifyValue in linearValue)
        {
            trueLinearValue += modifyValue.value;
        }
        foreach (ModifyValue modifyValue in multipleValue)
        {
            trueMultipleValueValue *= modifyValue.value > -1 ? (1 + modifyValue.value) : 0;
        }
        if (isNegative)
            return 1 - (baseValue + trueLinearValue) * trueMultipleValueValue;
        else
            return (baseValue + trueLinearValue) * trueMultipleValueValue;
    }

    public float GetConstructorModifyValue()
    {
        float trueLinearValue = 0;
        float trueMultipleValueValue = 1;
        foreach (ModifyValue modifyValue in linearValue)
        {
            if (modifyValue.valueModifySource == ValueModifySource.Constructor)
                trueLinearValue += modifyValue.value;
        }
        foreach (ModifyValue modifyValue in multipleValue)
        {
            if (modifyValue.valueModifySource == ValueModifySource.Constructor)
                trueMultipleValueValue *= modifyValue.value > -1 ? (1 + modifyValue.value) : 0;
        }
        if (isNegative)
            return 1 - (baseValue + trueLinearValue) * trueMultipleValueValue;
        else
            return (baseValue + trueLinearValue) * trueMultipleValueValue;
    }

    public float GetTrueValue(float max, float min = 0)
    {
        float noLimitValue = GetTrueValue();
        return Mathf.Min(max, Mathf.Max(min, noLimitValue));
    }

    public string GetColor(float delta)
    {
        string textColor = "white";
        float n = delta;
        if (n != 0)
            if (isNegative)
            {
                textColor = n > 0 ? "#FF7A7A" : "lime";
            }
            else
            {
                textColor = n < 0 ? "#FF7A7A" : "lime";
            }
        return textColor;
    }

    public string GetColor()
    {
        string textColor = "white";
        float n = GetTrueValue() - GetConstructorModifyValue();
        if (n != 0)
            if (isNegative)
            {
                textColor = n > 0 ? "#FF7A7A" : "lime";
            }
            else
            {
                textColor = n < 0 ? "#FF7A7A" : "lime";
            }
        return textColor;
    }
}
