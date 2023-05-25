using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyExcel;
using ExcelConfig;

/// <summary>
/// Stores basic Game Data
/// </summary>
public class GameData : CreateSingleton<GameData>
{
    ///Store all available champion, all champions must be assigned from the Editor to the Script GameObject
    public Champion[] championsArray;

    ///Store all available championType, all championTypes must be assigned from the Editor to the Script GameObject
    public ChampionType[] championTypesArray;
    public EEDataManager _eeDataManager;

    protected override void InitSingleton()
    {
        _eeDataManager = new EEDataManager();
        _eeDataManager.Load();
    }
}
