using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using ExcelConfig;


[Serializable]
public class BuffState
{
    public string name;
    public int byteFormat;
    public bool state;
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
public class BuffController : MonoBehaviour
{
    [SerializeField]
    public List<Buff> buffList = new List<Buff>();
    public EventCenter eventCenter = new EventCenter();

    [SerializeField]
    public BuffStateContainer buffStateContainer = new BuffStateContainer();

    ChampionController championController;

    private void Start()
    {
        championController = gameObject.GetComponent<ChampionController>();
    }

    //添加一个buff
    public void AddBuff(int id, ChampionController _caster = null)
    {
        AddBuff(GameData.Instance.baseBuffsArray.Find(b => b.ID == id), _caster);
    }

    public void AddBuff(BaseBuffData buffData, ChampionController _caster = null)
    {
        Buff buff;
        if (GameData.Instance.modifyAttributeBuffsArray.Exists(b => b.ID == buffData.ID))
        {
            ModifyAttributeBuffData mBuffData = GameData.Instance.modifyAttributeBuffsArray.Find(b => b.ID == buffData.ID);
            buff = new ModifyAttributeBuff(buffData, mBuffData, championController, _caster);
        }

        else
        {
            buff = new Buff(buffData, championController, _caster);
        }
        BuffSuperposeCheck(buff);
        buffList.Add(buff);
        buff.BuffStart();
    }

    //添加一个子Buff
    public void AddSubBuff(int id, AddBuffTargetType targetType)
    {
        switch (targetType)
        {
            case AddBuffTargetType.Self:
                AddBuff(id, championController);
                break;
            case AddBuffTargetType.Teammate:
                List<ChampionController> teammates;
                if (championController.team == ChampionTeam.Player)
                {
                    teammates = GamePlayController.Instance.ownChampionManager.championsHexaMapArray;
                }
                else
                {
                    teammates = GamePlayController.Instance.oponentChampionManager.championsHexaMapArray;
                }
                foreach (ChampionController c in teammates)
                {
                    c.gameObject.GetComponent<BuffController>().AddBuff(id, championController);
                }
                break;
            case AddBuffTargetType.Enemy:
                ChampionController target = gameObject.GetComponent<ChampionController>().target;
                if (target != null)
                    target.gameObject.GetComponent<BuffController>().AddBuff(id, championController);
                break;
            case AddBuffTargetType.Enemies:
                List<ChampionController> enemies;
                if (championController.team == ChampionTeam.Player)
                {
                    enemies = GamePlayController.Instance.oponentChampionManager.championsHexaMapArray;
                }
                else
                {
                    enemies = GamePlayController.Instance.ownChampionManager.championsHexaMapArray;
                }
                foreach (ChampionController c in enemies)
                {
                    c.gameObject.GetComponent<BuffController>().AddBuff(id, championController);
                }
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
        if (buff.superposeMode != BuffSuperposeMode.None)
        {
            foreach (Buff b in buffList)
            {
                if (b.buffData.ID == buff.buffData.ID)
                {
                    buff.Superpose(b);
                    buff.BuffRefresh();
                    RemoveBuff(b);
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
            if (GameData.Instance.modifyAttributeBuffsArray.Exists(b1 => b1.ID == b.buffData.ID))
            {
                ModifyAttributeBuff modifyBuff = b as ModifyAttributeBuff;
                tempBuffStateContainer.allSuperposStates |= modifyBuff.buffStateContainer.allSuperposStates;

            }
        }
        tempBuffStateContainer.CalculateState();
        buffStateContainer = tempBuffStateContainer;
    }

    public void OnUpdateCombat()
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
