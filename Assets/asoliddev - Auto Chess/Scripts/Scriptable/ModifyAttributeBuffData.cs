using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultModifyAttributeBuffData", menuName = "AutoChess/ModifyAttributeBuffData", order = 4)]
public class ModifyAttributeBuffData : BaseBuffData
{
    [Header("攻击")]
    public int attack;
    [Header("防御")]
    public int defend;
}
