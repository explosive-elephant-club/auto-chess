using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

[System.Serializable]
public class BuffCountRequire
{
    public int count;
    public BaseBuffData buff;
}

/// <summary>
/// Controls the bonuses to get when have enough champions of the same type
/// </summary>
[System.Serializable]
public class ChampionBonus
{
    [SerializeField]
    public List<BuffCountRequire> bonusCountRequireAndBuff = new List<BuffCountRequire>();


    //查找count对应的Buff
    public BaseBuffData GetBuffBonus(int count)
    {
        BaseBuffData buff = null;
        foreach (BuffCountRequire m in bonusCountRequireAndBuff)
        {
            if (count >= m.count)
            {
                buff = m.buff;
            }
            else
            {
                break;
            }
        }
        return buff;
    }
}
