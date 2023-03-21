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

public class Buff : buffEvent
{
    public BaseBuffData buffData;

    public GameObject owner;//Buff的拥有者
    public GameObject caster;//Buff的施加者
    //private Skill ability; //Buff是由哪个技能创建
    public int layer = 1;//叠加层数
    public float curTime = 0;//目前时长

    BuffBehaviour buffBehaviour;

    public Buff(BaseBuffData _buffData, GameObject _owner, GameObject _caster = null)
    {
        buffData = _buffData;
        owner = _owner;
        caster = buffData.bNoCaster ? null : _caster;

        onBuffAwake();
    }

    public void onBuffAwake()
    {
        if (buffData.buffBehaviourScriptName != "")
        {
            Type type = Type.GetType(buffData.buffBehaviourScriptName);
            buffBehaviour = (BuffBehaviour)owner.transform.Find("BuffBehaviour").gameObject.AddComponent(type);
            buffBehaviour.onBuffAwake();
        }
    }

    public void onBuffStart()
    {
        if (buffBehaviour)
        {
            buffBehaviour.onBuffAwake();
        }
    }
    public void onBuffRefresh()
    {
        if (buffBehaviour)
        {
            buffBehaviour.onBuffRefresh();
        }
    }
    public void onBuffRemove()
    {
        if (buffBehaviour)
        {
            buffBehaviour.onBuffRemove();
        }
    }
    public void onBuffDestroy()
    {
        if (buffBehaviour)
        {
            buffBehaviour.onBuffDestroy();
        }
    }
    public void onBuffInterval()
    {
        if (buffBehaviour)
        {
            buffBehaviour.onBuffInterval();
        }
    }
}
