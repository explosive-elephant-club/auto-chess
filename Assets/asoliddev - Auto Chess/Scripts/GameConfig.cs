using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : CreateSingleton<GameConfig>
{
    ///Maximum time the combat stage can last
    public int combatStageDuration = 60;
    ///base gold value to get after every round
    public int baseGoldIncome = 5;
    public int refreshCost;

    public int[] levelUpCostList;
    public int[] addSlotCostList;
    protected override void InitSingleton()
    { }
}
