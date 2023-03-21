using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultModifyAttributeBuffData", menuName = "AutoChess/ModifyAttributeBuffData", order = 4)]
public class ModifyAttributeBuffData : BaseBuffData
{
    [Header("状态类")]
    [Tooltip("不可移动")]
    public bool immovable;
    [Tooltip("缴械")]
    public bool disarm;
    [Tooltip("沉默")]
    public bool silence;
    [Tooltip("无敌")]
    public bool invincible;
    [Tooltip("隐身")]
    public bool invisible;

    [Header("数值类")]
    [Tooltip("攻击")]
    public int attack;
    [Tooltip("防御")]
    public int defend;
}
