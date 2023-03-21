using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffActiveMode
{
    [Tooltip("持久型")]
    Always,
    [Tooltip("每秒触发")]
    PerSecond,
    [Tooltip("攻击前触发")]
    BeforeAttack,
    [Tooltip("攻击后触发")]
    AfterAttack,
    [Tooltip("受击前触发")]
    BeforeHit,
    [Tooltip("受击后触发")]
    AfterHit
}
public enum BuffStackMode
{
    [Tooltip("持续时长叠加")]
    Time,
    [Tooltip("层数叠加")]
    Layer
}

[CreateAssetMenu(fileName = "DefaultBaseBuffData", menuName = "AutoChess/BaseBuffData", order = 3)]
public class BaseBuffData : ScriptableObject
{
    //配置项
    [Header("ID")]
    public string buffID;
    [Header("显示名称")]
    public string displayName;
    [Header("持续时间")]
    public float duration;
    [Header("不可移动")]
    public bool immovable;
    [Header("缴械")]
    public bool disarm;
    [Header("沉默")]
    public bool silence;
    [Header("无敌")]
    public bool invincible;
    [Header("隐身")]
    public bool invisible;
    [Header("触发模式")]
    public BuffStackMode stackMode;
    [Header("是否强制设置Caster为空")]
    public bool bNoCaster;
    [Header("叠加模式")]
    public BuffActiveMode activeMode;
    [Header("自定行为脚本")]
    public string buffBehaviourScriptName;



    //内在私有变量
    [HideInInspector]
    public GameObject caster;//Buff的施加者
    //private Skill ability; //Buff是由哪个技能创建
    [HideInInspector]
    public int layer = 1;//叠加层数

}
