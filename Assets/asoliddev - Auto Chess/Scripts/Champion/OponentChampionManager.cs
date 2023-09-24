using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using General;

public class OponentChampionManager : ChampionManager
{
    public override void OnEnterPreparation()
    {
        base.OnEnterPreparation();
        this.AddChampionToBattle("Champion");
        //this.AddChampionToBattle(GameData.Instance.championsArray.Find(c => c.ID == 1));
        //this.AddChampionToBattle(ChampionShop.Instance.GetRandomChampionInfo());
    }
}
