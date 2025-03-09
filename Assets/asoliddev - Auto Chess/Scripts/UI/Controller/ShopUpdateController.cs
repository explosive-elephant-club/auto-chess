using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class ShopUpdateController : BaseControllerUI
{
    public CombatLevelData combatLevelData
    {
        get { return GameConfig.Instance.GetCurCombatLevelData(); }
    }
    public TradeLevelData tradeLevelData
    {
        get { return GameConfig.Instance.GetCurTradeLevelData(); }
    }
    public CommandLevelData commandLevelData
    {
        get { return GameConfig.Instance.GetCurCommandLevelData(); }
    }
    public LogisticsLevelData logisticsLevelData
    {
        get { return GameConfig.Instance.GetCurLogisticsLevelData(); }
    }

    #region 自动绑定
    
    #endregion

    void Start()
    {
    }


    private void OnEnable()
    {
        UpdateUI();
    }

    public override void UpdateUI()
    {
      
    }


}
