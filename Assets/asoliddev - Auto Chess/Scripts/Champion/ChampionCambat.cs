using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls a single champion combat
/// </summary>
public class ChampionCambat
{
    ChampionController championController;
    public ChampionCambat(ChampionController _championController)
    {
        championController = _championController;
        
    }

}