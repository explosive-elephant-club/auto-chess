using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

interface buffEvent
{
    void onBuffAwake();//Buff在实例化之后，生效之前
    void onBuffStart();//当Buff生效时（加入到Buff容器后）
    void onBuffRefresh();//当Buff添加时存在相同类型且Caster相等的时候，Buff执行刷新流程
    void onBuffRemove();//当Buff销毁前（还未从Buff容器中移除）
    void onBuffDestroy();//当Buff销毁后（已从Buff容器中移除）
    void onBuffInterval();//当计时器每次触发

}

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

    public Buff(BaseBuffData _buffData, GameObject _owner, GameObject _caster = null)
    {
        buffData = _buffData;
        owner = _owner;
        caster = buffData.bNoCaster ? null : _caster;
        curLayer = buffData.layer;
        curTime = buffData.duration;
        intervalTimer = 0;


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
        
        buffBehaviour.onBuffAwake();
        if (buffData.activeMode != BuffActiveMode.Interval && buffData.activeMode != BuffActiveMode.Always)
        {
            EventCenter.AddListener("BuffEvent" + buffData.activeMode.ToString(), BuffActive);
        }
    }


    //叠加合并
    public void Superpose(Buff buff)
    {
        switch (buffData.superposeMode)
        {
            case BuffSuperposeMode.Time:
                curTime += buff.curTime;
                break;
            case BuffSuperposeMode.Layer:
                curLayer += buff.curLayer;
                break;
            case BuffSuperposeMode.None:
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
                buffBehaviour.onBuffInterval();
                //触发Buff
            }
        }
    }

    //buff触发
    public void BuffActive()
    {
        if (buffData.consumeMode == BuffConsumeMode.Active)
        {
            owner.SendMessage("RemoveBuff", this);
        }
    }
}