using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class BuffState
{
    public string name;
    public int byteFormat;
    public bool state;
}

public class BuffStateContainer
{
    public int allSuperposStates = 0;
    public List<BuffState> states;

    public BuffStateContainer(BuffStateBoolValues _boolValues = null)
    {
        Type type = typeof(BuffStateBoolValues);
        FieldInfo[] fields = type.GetFields();

        states = new List<BuffState>();

        for (int i = 0; i < fields.Length; i++)
        {
            BuffState b = new BuffState();
            b.name = fields[i].Name.ToString();
            b.byteFormat = 1 << i;
            b.state = _boolValues == null ? false : (bool)fields[i].GetValue(_boolValues);
            states.Add(b);
        }
        CalculateState(0);
    }

    public void CalculateState(int _states = 0)
    {
        allSuperposStates = allSuperposStates | _states;
        foreach (BuffState b in states)
        {
            b.state = (b.byteFormat & allSuperposStates) == 0 ? false : true;
        }
    }

    public bool GetState(string name)
    {
        return states.Find(s => s.name == name).state;
    }
}
public class BuffController : MonoBehaviour
{
    [SerializeField]
    public List<Buff> buffList = new List<Buff>();
    public EventCenter eventCenter = new EventCenter();
    public BuffStateContainer buffStateContainer = new BuffStateContainer();

    //添加一个buff
    public void AddBuff(BaseBuffData buffData, GameObject _caster = null)
    {
        Buff buff = new Buff(buffData, gameObject, _caster);
        BuffSuperposeCheck(buff);
        buffList.Add(buff);
        buff.BuffStart();
    }

    //移除一个buff
    public void RemoveBuff(Buff buff)
    {
        buff.BuffRemove();
        buffList.Remove(buff);
        buff.BuffDestroy();
    }

    //移除所有buff
    public void RemoveAllBuff()
    {
        foreach (Buff b in buffList)
        {
            RemoveBuff(b);
        }
    }

    //检查并合并相同buff
    public void BuffSuperposeCheck(Buff buff)
    {
        if (buff.buffData.superposeMode != BuffSuperposeMode.None)
        {
            foreach (Buff b in buffList)
            {
                if (b.buffData.buffID == buff.buffData.buffID)
                {
                    buff.Superpose(b);
                    buff.BuffRefresh();
                    buffList.Remove(b);
                    return;
                }
            }
        }
        return;
    }

    //新增Buff的State的叠加
    public void AddBuffState(ModifyAttributeBuff _buff)
    {
        buffStateContainer.CalculateState(_buff.buffStateContainer.allSuperposStates);
    }

    //移除Buff的State的叠加
    public void RemoveBuffState(ModifyAttributeBuff _buff)
    {
        buffStateContainer.allSuperposStates =
        buffStateContainer.allSuperposStates ^
        _buff.buffStateContainer.allSuperposStates;
        buffStateContainer.CalculateState();
    }

    private void Update()
    {
        if (buffList.Count > 0)
        {
            foreach (Buff b in buffList)
            {
                b.TimerTick();
            }
        }
    }

}
