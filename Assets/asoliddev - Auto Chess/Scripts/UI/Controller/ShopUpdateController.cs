using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class LevelUpdateMenu
{
    public Button levelUpBtn;
    public Text levelUpCost;
    public Text level;
    public Text info;
}

public class ShopUpdateController : BaseControllerUI
{
    public LevelUpdateMenu combatLevelUpMenu;
    public LevelUpdateMenu tradeLevelUpMenu;
    public LevelUpdateMenu commandLevelUpMenu;
    public LevelUpdateMenu logisticsLevelUpMenu;

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
        combatLevelUpMenu.levelUpBtn.onClick.AddListener(OnCombatLevelUpBtnClick);
        tradeLevelUpMenu.levelUpBtn.onClick.AddListener(OnTradeLevelUpBtnClick);
        commandLevelUpMenu.levelUpBtn.onClick.AddListener(OnCommandLevelUpBtnClick);
        logisticsLevelUpMenu.levelUpBtn.onClick.AddListener(OnLogisticsLevelUpBtnClick);
    }

    void OnCombatLevelUpBtnClick()
    {
        if (GameData.Instance.combatLevel < 5 && GameData.Instance.currentGold >= combatLevelData.cost)
        {
            GameData.Instance.currentGold -= combatLevelData.cost;
            UIController.Instance.levelInfo.UpdateUI();
            GameData.Instance.combatLevel++;
            UpdateMenu(combatLevelUpMenu, GameData.Instance.combatLevel, combatLevelData.cost);
        }
    }
    void OnTradeLevelUpBtnClick()
    {
        if (GameData.Instance.tradeLevel < 5 && GameData.Instance.currentGold >= tradeLevelData.cost)
        {
            GameData.Instance.currentGold -= tradeLevelData.cost;
            GameData.Instance.tradeLevel++;
            UIController.Instance.shopController.shopConstructController.AddShopSlot();
            UIController.Instance.UpdateUI();
            UpdateMenu(tradeLevelUpMenu, GameData.Instance.tradeLevel, tradeLevelData.cost);
        }
    }
    void OnCommandLevelUpBtnClick()
    {
        if (GameData.Instance.commandLevel < 5 && GameData.Instance.currentGold >= commandLevelData.cost)
        {
            GameData.Instance.currentGold -= commandLevelData.cost;
            GamePlayController.Instance.ownChampionManager.currentChampionLimit = commandLevelData.limitMax;
            GameData.Instance.commandLevel++;
            UIController.Instance.UpdateUI();
            UpdateMenu(commandLevelUpMenu, GameData.Instance.commandLevel, commandLevelData.cost);
        }
    }
    void OnLogisticsLevelUpBtnClick()
    {
        if (GameData.Instance.logisticsLevel < 5 && GameData.Instance.currentGold >= logisticsLevelData.cost)
        {
            GameData.Instance.currentGold -= logisticsLevelData.cost;
            UIController.Instance.levelInfo.UpdateUI();
            GameData.Instance.logisticsLevel++;
            UpdateMenu(logisticsLevelUpMenu, GameData.Instance.logisticsLevel, logisticsLevelData.cost);
        }
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        UpdateMenu(combatLevelUpMenu, GameData.Instance.combatLevel, combatLevelData.cost);
        UpdateMenu(tradeLevelUpMenu, GameData.Instance.tradeLevel, tradeLevelData.cost);
        UpdateMenu(commandLevelUpMenu, GameData.Instance.commandLevel, commandLevelData.cost);
        UpdateMenu(logisticsLevelUpMenu, GameData.Instance.logisticsLevel, logisticsLevelData.cost);
    }

    void UpdateMenu(LevelUpdateMenu menu, int level, int cost)
    {
        if (level < 5)
        {
            menu.levelUpBtn.gameObject.SetActive(true);
            menu.levelUpCost.text = cost.ToString();
        }
        else
        {
            menu.levelUpBtn.gameObject.SetActive(false);
        }

        menu.level.text = level.ToString();
    }

}
