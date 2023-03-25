using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

[Serializable]
public class AddSubBuff
{
    public BaseBuffData buffData;
    public AddBuffTargetType targetType;
}


[CreateAssetMenu(fileName = "DefaultBaseBuffData", menuName = "AutoChess/BaseBuffData", order = 3)]
[Serializable]
public class BaseBuffData : ScriptableObject
{
    //配置项
    [Tooltip("ID")]
    public string buffID;
    [Tooltip("显示名称")]
    public string displayName;
    [Tooltip("持续时间")]
    public float duration;
    [Tooltip("触发间隔")]
    public float intervalTime;
    [Tooltip("层数")]
    public int layer;

    [Tooltip("是否强制设置Caster为空")]
    public bool bNoCaster;

    [Tooltip("触发模式")]
    public BuffActiveMode activeMode;
    [Tooltip("消失模式")]
    public BuffConsumeMode consumeMode;
    [Tooltip("叠加模式")]
    public BuffSuperposeMode superposeMode;
    [Tooltip("自定行为脚本")]
    public string buffBehaviourScriptName;
    [Tooltip("触发时添加的Buff")]
    [SerializeField]
    public AddSubBuff[] addBuffs;



}
