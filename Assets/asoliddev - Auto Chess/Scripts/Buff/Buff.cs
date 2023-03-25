using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class Buff
{
    public BaseBuffData buffData;

    public GameObject owner;//Buff的拥有者
    public GameObject caster;//Buff的施加者
    //private Skill ability; //Buff是由哪个技能创建
    public int curLayer = 1;//目前叠加层数
    public float curTime = 0;//目前时长
    public float intervalTimer = 0;//计时器

    public BuffBehaviour buffBehaviour;

    public BuffController buffController;

    public Buff(BaseBuffData _buffData, GameObject _owner, GameObject _caster = null)
    {
        buffData = _buffData;
        owner = _owner;
        caster = buffData.bNoCaster ? null : _caster;
        curLayer = buffData.layer;
        curTime = buffData.duration;
        intervalTimer = 0;

        buffController = owner.GetComponent<BuffController>();
        if (!string.IsNullOrEmpty(buffData.buffBehaviourScriptName))
        {
            try
            {
                Type type = Type.GetType(buffData.buffBehaviourScriptName);
                buffBehaviour = (BuffBehaviour)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Debug.LogError("Create BuffBehaviour instance failed: " + ex.Message);
                buffBehaviour = new BuffBehaviour();
            }
        }
        else
        {
            buffBehaviour = new BuffBehaviour();
        }
        BuffAwake();
    }

    //叠加合并
    public void Superpose(Buff buff)
    {
        switch (buffData.superposeMode)
        {
            case BuffSuperposeMode.None:
                break;
            case BuffSuperposeMode.Cover:
                break;
            case BuffSuperposeMode.Time:
                curTime += buff.curTime;
                break;
            case BuffSuperposeMode.Layer:
                curLayer += buff.curLayer;
                break;

        }
    }

    //计时器触发
    public void TimerTick()
    {
        if (buffData.consumeMode == BuffConsumeMode.AfterDuration)
        {
            curTime -= Time.deltaTime;
            if (curTime <= 0)
            {
                owner.SendMessage("RemoveBuff", this);
            }
        }

        if (buffData.activeMode == BuffActiveMode.Interval)
        {
            intervalTimer += Time.deltaTime;
            if (intervalTimer >= buffData.intervalTime)
            {
                intervalTimer = 0;
                buffBehaviour.BuffInterval();
                //触发Buff
            }
        }
    }

    public virtual void BuffAwake()
    {
        buffBehaviour.BuffAwake();
    }
    public virtual void BuffStart()
    {
        BuffController _controller = owner.GetComponent<BuffController>();
        if (buffData.activeMode != BuffActiveMode.Interval)
        {
            if (buffData.activeMode == BuffActiveMode.Always)
            {
                BuffActive();
            }
            else
            {
                _controller.eventCenter.AddListener(buffData.activeMode.ToString(), BuffActive);
            }
        }
        buffBehaviour.BuffStart();
    }
    public virtual void BuffRefresh()
    {
        buffBehaviour.BuffRefresh();
    }
    public virtual void BuffRemove()
    {
        buffBehaviour.BuffRemove();
    }
    public virtual void BuffDestroy()
    {
        buffBehaviour.BuffDestroy();
    }
    public virtual void BuffInterval()
    {
        buffBehaviour.BuffInterval();
    }
    //buff触发
    public virtual void BuffActive()
    {
        buffBehaviour.BuffActive();
        foreach (AddSubBuff b in buffData.addBuffs)
        {
            buffController.AddSubBuff(b);
        }
        if (buffData.consumeMode == BuffConsumeMode.Active)
        {
            buffController.RemoveBuff(this);
        }
    }
}