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
    //public Champion[] championsArray;

    public List<ChampionBaseData> championsArray;

    public List<BaseBuffData> baseBuffsArray;
    public List<ModifyAttributeBuffData> modifyAttributeBuffsArray;

    public List<SkillData> skillDatasArray;
    public EEDataManager _eeDataManager;

    protected override void InitSingleton()
    {
        _eeDataManager = new EEDataManager();
        _eeDataManager.Load();

        championsArray = _eeDataManager.GetList<ChampionBaseData>();
        baseBuffsArray = _eeDataManager.GetList<BaseBuffData>();
        modifyAttributeBuffsArray = _eeDataManager.GetList<ModifyAttributeBuffData>();

        skillDatasArray = _eeDataManager.GetList<SkillData>();
    }
}
