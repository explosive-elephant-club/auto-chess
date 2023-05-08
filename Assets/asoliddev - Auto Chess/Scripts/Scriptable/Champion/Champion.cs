using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores all the stats and information of a champion character
/// </summary>
[CreateAssetMenu(fileName = "DefaultChampion", menuName = "AutoChess/Champion", order = 1)]
public class Champion : ScriptableObject
{
    ///Physical champion Prefab to create in the game
    public GameObject prefab;

    ///Projectile prefab to create when champion is attacking
    public GameObject attackProjectile;

    ///The champion name displayed on the UI frames
    public string uiname;

    ///The buy gold cost of the champion from the shop
    public int cost;

    ///The type of the champion
    public ChampionType type1;

    ///The type of the champion
    public ChampionType type2;


    [Header("1级属性")]
    public ChampionAttributeData level1_Attribute;

    [Header("2级属性")]
    public ChampionAttributeData level2_Attribute;

    [Header("3级属性")]
    public ChampionAttributeData level3_Attribute;
}


