using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using General;


public class OwnChampionManager : ChampionManager
{
    public CommandLevelData commandLevelData
    {
        get { return GameConfig.Instance.GetCurCommandLevelData(); }
    }
    public override void OnEnterPreparation()
    {
        base.OnEnterPreparation();
        currentChampionLimit = commandLevelData.limitMax;
        this.AddChampionToBattle("Enemy/1-4");
        //this.AddChampionToBattle(ChampionShop.Instance.GetRandomChampionInfo());
    }

    public override void OnLeaveCombat()
    {
        base.OnLeaveCombat();
        foreach (var champion in championsBattleArray)
        {
            if (champion.unitType == ChampionUnitType.Main)
                champion.Reset();
            else
                champion.DestroySelf();
        }
    }

}
