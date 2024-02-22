using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using General;


public class OwnChampionManager : ChampionManager
{
    public CommandLevelData commandLevelData
    {
        get { return GameConfig.Instance.commandLevelData[GameData.Instance.commandLevel - 1]; }
    }
    public override void OnEnterPreparation()
    {
        base.OnEnterPreparation();
        currentChampionLimit = commandLevelData.limitMax;
        UIController.Instance.levelInfo.UpdateUI();
        this.AddChampionToBattle("Enemy/1-5");
        //this.AddChampionToBattle(ChampionShop.Instance.GetRandomChampionInfo());
    }


}
