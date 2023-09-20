using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using General;


public class OwnChampionManager : ChampionManager
{
    public override void OnEnterPreparation()
    {
        base.OnEnterPreparation();
        this.AddChampionToBattle("Champion");
        //this.AddChampionToBattle(ChampionShop.Instance.GetRandomChampionInfo());
    }
}
