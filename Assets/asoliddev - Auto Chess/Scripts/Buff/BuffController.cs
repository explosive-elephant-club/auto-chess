using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;


[Serializable]
public class BuffState
{
    public string name;
    public int byteFormat;
    public bool state;
}

public interface AddBuffInterface
{
    void AddBuff(BaseBuffData buffData, GameObject _caster = null);
}

[Serializable]
public class BuffStateContainer
{
    public int allSuperposStates = 0;

    [SerializeField]
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

            if (b.state)
                allSuperposStates = allSuperposStates | b.byteFormat;
            states.Add(b);
        }
    }

    public void CalculateState(int _states = 0)
    {
        allSuperposStates = allSuperposStates | _states;
        foreach (BuffState b in states)
        {
            b.state = (allSuperposStates & b.byteFormat) == 0 ? false : true;

        }
    }

    public bool GetState(string name)
    {
        return states.Find(s => s.name == name).state;
    }
}
public class BuffController : MonoBehaviour, AddBuffInterface
{
    [SerializeField]
    public List<Buff> buffList = new List<Buff>();
    public EventCenter eventCenter = new EventCenter();

    [SerializeField]
    public BuffStateContainer buffStateContainer = new BuffStateContainer();

    //添加一个buff
    public void AddBuff(BaseBuffData buffData, GameObject _caster = null)
    {
        Buff buff;
        if ((buffData as ModifyAttributeBuffData) != null)
        {
            buff = new ModifyAttributeBuff(buffData, gameObject, _caster);
        }
        else
        {
            buff = new Buff(buffData, gameObject, _caster);
        }
        BuffSuperposeCheck(buff);
        buffList.Add(buff);
        buff.BuffStart();
    }

    //添加一个子Buff
    public void AddSubBuff(AddSubBuff subBuff)
    {
        switch (subBuff.targetType)
        {
            case AddBuffTargetType.Self:
                AddBuff(subBuff.buffData, gameObject);
                break;
            case AddBuffTargetType.Teammate:
                break;
            case AddBuffTargetType.Enemy:
                ChampionController target = gameObject.GetComponent<ChampionController>().target;
                if (target != null)
                    target.gameObject.GetComponent<AddBuffInterface>().AddBuff(subBuff.buffData, gameObject);
                break;
            case AddBuffTargetType.Enemies:
                break;

        }
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
        for (int i = 0; i < buffList.Count; i++)
        {
            RemoveBuff(buffList[i]);
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

    //计算所有Buff的State的叠加
    public void CalculateAllBuffState()
    {
        BuffStateContainer tempBuffStateContainer = new BuffStateContainer();
        foreach (Buff b in buffList)
        {
            if ((b.buffData as ModifyAttributeBuffData) != null)
            {
                ModifyAttributeBuff modifyBuff = b as ModifyAttributeBuff;
                tempBuffStateContainer.allSuperposStates |= modifyBuff.buffStateContainer.allSuperposStates;

            }
        }
        tempBuffStateContainer.CalculateState();
        buffStateContainer = tempBuffStateContainer;
    }

    private void Update()
    {
        if (buffList.Count > 0)
        {
            for (int i = 0; i < buffList.Count; i++)
            {
                buffList[i].TimerTick();
            }
        }
    }

}
