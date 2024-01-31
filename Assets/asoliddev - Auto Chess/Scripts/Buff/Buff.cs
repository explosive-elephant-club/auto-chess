using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using ExcelConfig;

public enum BuffActiveMode
{
    [Tooltip("永久型")]
    Always,
    [Tooltip("定时触发")]
    Interval,
    [Tooltip("攻击前触发")]
    BeforeAttack,
    [Tooltip("攻击后触发")]
    AfterAttack,
    [Tooltip("施法前触发")]
    BeforeCast,
    [Tooltip("施法后触发")]
    AfterCast,
    [Tooltip("受击前触发")]
    BeforeHit,
    [Tooltip("受击后触发")]
    AfterHit,
}

public enum BuffConsumeMode
{
    [Tooltip("不消失")]
    None,
    [Tooltip("触发时消失")]
    Active,
    [Tooltip("持续时间结束后消失")]
    AfterDuration,
}

public enum BuffSuperposeMode
{
    [Tooltip("不叠加")]
    None,
    [Tooltip("覆盖")]
    Cover,
    [Tooltip("持续时长叠加")]
    Time,
    [Tooltip("层数叠加")]
    Layer
}

public enum AddBuffTargetType
{
    [Tooltip("自身")]
    Self,
    [Tooltip("队友")]
    Teammate,
    [Tooltip("敌人")]
    Enemy,
    [Tooltip("敌人群")]
    Enemies
}

public class BuffStateBoolValues
{
    [Tooltip("不可移动")]
    public bool immovable = false;
    [Tooltip("缴械")]
    public bool disarm = false;
    [Tooltip("沉默")]
    public bool silence = false;
    [Tooltip("无敌")]
    public bool invincible = false;
    [Tooltip("隐身")]
    public bool invisible = false;

    public BuffStateBoolValues(bool _immovable, bool _disarm, bool _silence, bool _invincible, bool _invisible)
    {
        immovable = _immovable;
        disarm = _disarm;
        silence = _silence;
        invincible = _invincible;
        invisible = _invisible;
    }
}

[Serializable]
public class Buff
{
    public BaseBuffData buffData;

    public BuffActiveMode activeMode;
    public BuffConsumeMode consumeMode;
    public BuffSuperposeMode superposeMode;

    public ChampionController owner;//Buff的拥有者
    public ChampionController caster;//Buff的施加者
    //private Skill ability; //Buff是由哪个技能创建
    public int curLayer = 1;//目前叠加层数
    public float curTime = 0;//目前时长
    public float intervalTimer = 0;//计时器

    public BuffBehaviour buffBehaviour;

    public BuffController buffController;

    public Buff(BaseBuffData _buffData, ChampionController _owner, ChampionController _caster = null)
    {
        buffData = _buffData;

        activeMode = (BuffActiveMode)Enum.Parse(typeof(BuffActiveMode), buffData.activeMode);
        consumeMode = (BuffConsumeMode)Enum.Parse(typeof(BuffConsumeMode), buffData.consumeMode);
        superposeMode = (BuffSuperposeMode)Enum.Parse(typeof(BuffSuperposeMode), buffData.superposeMode);

        owner = _owner;
        caster = buffData.bNoCaster ? null : _caster;
        curLayer = buffData.layer;
        curTime = buffData.duration;
        intervalTimer = 0;

        buffController = owner.buffController;
        if (!string.IsNullOrEmpty(buffData.buffBehaviourScriptName))
        {
            try
            {
                Type type = Type.GetType(buffData.buffBehaviourScriptName);
                buffBehaviour = (BuffBehaviour)Activator.CreateInstance(type);
                buffBehaviour.Init(this);
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
    public virtual void Superpose(Buff buff)
    {
        switch (superposeMode)
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
        if (consumeMode == BuffConsumeMode.AfterDuration)
        {
            curTime -= Time.deltaTime;
            if (curTime <= 0)
            {
                owner.SendMessage("RemoveBuff", this);
            }
        }

        if (activeMode == BuffActiveMode.Interval)
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
        if (activeMode != BuffActiveMode.Interval)
        {
            if (activeMode == BuffActiveMode.Always)
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
        foreach (BaseBuffData.addBuffsClass b in buffData.addBuffs)
        {
            if (b.buff_ID != 0)
            {
                int n = UnityEngine.Random.Range(0, 100);
                if (n < b.odds * 100)
                {
                    AddBuffTargetType targetType = (AddBuffTargetType)Enum.Parse(typeof(AddBuffTargetType), b.targetType);
                    buffController.AddSubBuff(b.buff_ID, targetType);
                }

            }

        }
        if (consumeMode == BuffConsumeMode.Active)
        {
            buffController.RemoveBuff(this);
        }
    }
}