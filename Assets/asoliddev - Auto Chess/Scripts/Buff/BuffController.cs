using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuffController : MonoBehaviour
{
    [SerializeField]
    public List<Buff> buffList = new List<Buff>();
    public EventCenter eventCenter = new EventCenter();

    //添加一个buff
    public void AddBuff(BaseBuffData buffData, GameObject _caster = null)
    {
        Buff buff = new Buff(buffData, gameObject, _caster);
        BuffSuperposeCheck(buff);
        buffList.Add(buff);
        AddBuffListener(buff);
        buff.buffBehaviour.onBuffStart();
    }

    //移除一个buff
    public void RemoveBuff(Buff buff)
    {
        buff.buffBehaviour.onBuffRemove();
        buffList.Remove(buff);
        buff.buffBehaviour.onBuffDestroy();
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
                    buff.buffBehaviour.onBuffRefresh();
                    buffList.Remove(b);
                    return;
                }
            }
        }
        return;
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

    //注册监听事件
    public void AddBuffListener(Buff buff)
    {
        if (buff.buffData.activeMode != BuffActiveMode.Interval)
        {
            if (buff.buffData.activeMode == BuffActiveMode.Always)
            {
                buff.BuffActive();
            }
            else
            {
                eventCenter.AddListener(buff.buffData.activeMode.ToString(), buff.BuffActive);
            }
        }
    }

}
