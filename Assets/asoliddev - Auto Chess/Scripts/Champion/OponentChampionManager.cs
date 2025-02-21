using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using General;
using System.Diagnostics;

public class OponentChampionManager : ChampionManager
{
    public override void OnEnterPreparation()
    {
        base.OnEnterPreparation();
        currentChampionLimit = 99;
        foreach (var c in LevelManager.Instance.GetEnemyConfigs())
        {
            AddChampionToBattle(c);
        }
        //this.AddChampionToBattle("Enemy/1-1-1");
        //this.AddChampionToBattle(GameData.Instance.championsArray.Find(c => c.ID == 1));
        //this.AddChampionToBattle(ChampionShop.Instance.GetRandomChampionInfo());
    }

    public override void OnLeaveCombat()
    {
        base.OnLeaveCombat();
        GameData.Instance.currentHP -= CalculatePunish();
        UIController.Instance.levelInfo.UpdateUI();
        UnityEngine.Debug.Log("OponentChampionManager Reset");
        Reset();
    }

    public int CalculatePunish()
    {
        int value = 0;
        foreach (var champion in championsBattleArray)
        {
            if (!champion.isDead)
            {
                value += GameConfig.Instance.GetHPPunish(champion.CalculateTotalCost());
            }
        }
        return value;
    }

    public override void OnChampionDeath(ChampionController championController)
    {
        GameData.Instance.currentGold += Mathf.CeilToInt(championController.CalculateTotalCost() * 0.1f);
        UIController.Instance.levelInfo.UpdateUI();
        base.OnChampionDeath(championController);
    }
}
