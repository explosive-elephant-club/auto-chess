using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffActiveMode
{
    [Tooltip("永久型")]
    Always,
    [Tooltip("添加时触发")]
    Add,
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
    [Tooltip("ID")]
    public string buffID;
    [Tooltip("显示名称")]
    public string displayName;
    [Tooltip("持续时间")]
    public float duration;

    [Tooltip("触发模式")]
    public BuffStackMode stackMode;
    [Tooltip("是否强制设置Caster为空")]
    public bool bNoCaster;
    [Tooltip("叠加模式")]
    public BuffActiveMode activeMode;
    [Tooltip("自定行为脚本")]
    public string buffBehaviourScriptName;





}
