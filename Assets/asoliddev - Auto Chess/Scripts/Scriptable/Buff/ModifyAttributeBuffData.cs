using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
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
}

[CreateAssetMenu(fileName = "DefaultModifyAttributeBuffData", menuName = "AutoChess/ModifyAttributeBuffData", order = 4)]
public class ModifyAttributeBuffData : BaseBuffData
{
    [Header("状态类")]
    public BuffStateBoolValues stateBoolValues = new BuffStateBoolValues();

    [Header("数值类")]
    [Tooltip("攻击")]
    public int attack;
    [Tooltip("防御")]
    public int defend;
}
