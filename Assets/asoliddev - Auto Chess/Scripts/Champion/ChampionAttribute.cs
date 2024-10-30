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

public class ChampionAttribute
{
    //基础值
    public float baseValue;
    //基础值
    public string attributeName;
    //线性叠加值
    protected List<float> linearValue;
    //倍数叠加值
    protected List<float> multipleValue;

    public ChampionAttribute(float baseValue, string name)
    {
        this.baseValue = baseValue;
        attributeName = name;
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

    public float GetTrueValue(float externalValue = 0)
    {
        float trueLinearValue = externalValue;
        float trueMultipleValueValue = 1;
        foreach (float value in linearValue)
        {
            trueLinearValue += value;
        }
        foreach (float value in multipleValue)
        {
            trueMultipleValueValue *= value > -1 ? (1 + value) : 0;
        }
        return (baseValue + trueLinearValue) * trueMultipleValueValue;
    }

    public float GetTrueValue(float max, float min = 0)
    {
        float noLimitValue = GetTrueValue();
        return Mathf.Min(max, Mathf.Max(min, noLimitValue));
    }
}
