using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CombatLevelData
{
    public int level;
    public int cost;
    public int buffID;
}
[Serializable]
public class TradeLevelData
{
    public int level;
    public int cost;
    public int saleCount;
    public int NP;
    public int RP;
    public int SP;
    public int EP;
    public int LP;
}
[Serializable]
public class CommandLevelData
{
    public int level;
    public int cost;
    public int limitMax;
}
[Serializable]
public class LogisticsLevelData
{
    public int level;
    public int cost;
    public float recover;
}


public class GameConfig : CreateSingleton<GameConfig>
{
    ///Maximum time the combat stage can last
    public int combatStageDuration = 60;
    ///base gold value to get after every round
    public int baseGoldIncome = 5;
    public int refreshCost;

    public int[] addSlotCostList;

    [Header("装甲材质")]
    public Material[] mechMaterial1;
    public Material[] mechMaterial2;
    public Material[] mechMaterial3;

    public CombatLevelData[] combatLevelData;
    public TradeLevelData[] tradeLevelData;
    public CommandLevelData[] commandLevelData;
    public LogisticsLevelData[] logisticsLevelData;

    [Header("总价转换伤害")]
    public int[] enemyPunish;


    protected override void InitSingleton()
    { }

    public int GetHPPunish(int cost)
    {
        for (int i = 0; i < enemyPunish.Length; i++)
        {
            if (cost < enemyPunish[i])
            {
                if (i == 0)
                {
                    return i + 1;
                }
                else if (cost >= enemyPunish[i - 1])
                {
                    return i + 1;
                }
            }
        }
        return enemyPunish.Length;
    }

    public CombatLevelData GetCurCombatLevelData()
    {
        return combatLevelData[GameData.Instance.combatLevel - 1];
    }

    public TradeLevelData GetCurTradeLevelData()
    {
        return tradeLevelData[GameData.Instance.combatLevel - 1];
    }

    public CommandLevelData GetCurCommandLevelData()
    {
        return commandLevelData[GameData.Instance.combatLevel - 1];
    }

    public LogisticsLevelData GetCurLogisticsLevelData()
    {
        return logisticsLevelData[GameData.Instance.combatLevel - 1];
    }
}
