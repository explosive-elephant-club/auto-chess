using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

public class GameData : CreateSingleton<GameData>
{
    public int currentHP = 100;
    public int currentGold = 0;
    public List<ChampionController> championInventoryArray;
    public List<ChampionController> championsHexaMapArray;
    public List<ConstructorBaseData> allInventoryConstructors;
    public int constructsOnSaleLimit = 3;

    public List<ConstructorBaseData> constructsOnSale;
    protected override void InitSingleton()
    { }
}
