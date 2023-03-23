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

//状态类的二进制存储
public class BuffStateByteFormat
{
    public static int immovableState = 1 << 1;
    public static int disarmState = 1 << 2;
    public static int silenceState = 1 << 3;
    public static int invincibleState = 1 << 4;
    public static int invisibleState = 1 << 5;
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
