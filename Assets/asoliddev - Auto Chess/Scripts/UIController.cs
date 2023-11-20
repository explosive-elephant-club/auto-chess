using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ExcelConfig;

public enum ViewMode
{
    Battle,
    Logistics
}

public class UIController : CreateSingleton<UIController>
{
    public ViewMode viewMode;
    public bool isSlotUIDragged = false;

    public GameObject mask;
    public ShopGUIController shop;
    public LevelInfoController levelInfo;
    public ChampionInfoController championInfoController;
    public InventoryController inventoryController;
    public ConstructorAssembleController constructorAssembleController;
    public GameObject restartButton;
    public GameObject bonusContainer;
    public GameObject bonusUIPrefab;

    public Dictionary<string, CallBack> gameStageActions = new Dictionary<string, CallBack>();

    protected override void InitSingleton()
    {
        InitStageDic();
        viewMode = ViewMode.Battle;
    }

    public void InitStageDic()
    {
        gameStageActions.Add("OnEnterPreparation", OnEnterPreparation);
        gameStageActions.Add("OnEnterCombat", OnEnterCombat);
        gameStageActions.Add("OnEnterLoss", OnEnterLoss);
        gameStageActions.Add("OnUpdatePreparation", OnUpdatePreparation);
        gameStageActions.Add("OnUpdateCombat", OnUpdateCombat);
        gameStageActions.Add("OnUpdateLoss", OnUpdateLoss);
        gameStageActions.Add("OnLeavePreparation", OnLeavePreparation);
        gameStageActions.Add("OnLeaveCombat", OnLeaveCombat);
        gameStageActions.Add("OnLeaveLoss", OnLeaveLoss);
    }

    /// <summary>
    /// Called when restart button clicked on UI
    /// </summary>
    public void Restart_Click()
    {
        GamePlayController.Instance.RestartGame();
    }

    public void UpdateUI()
    {
        levelInfo.UpdateUI();
        championInfoController.UpdateUI();
        UIController.Instance.constructorAssembleController.UpdateUI();
    }

    public void ShowLossScreen()
    {
        mask.SetActive(true);
        restartButton.SetActive(true);
    }

    public void ShowGameScreen()
    {
        mask.SetActive(false);
        restartButton.SetActive(false);
    }

    public void ChangeViewMode()
    {
        if (viewMode == ViewMode.Battle)
        {
            OnEnterLogisticsViewMode();
        }
        else
        {
            OnEnterBattleViewMode();
        }
    }

    public void OnEnterBattleViewMode()
    {
        inventoryController.SetUIActive(false);
        if (viewMode != ViewMode.Battle)
        {
            viewMode = ViewMode.Battle;
            GamePlayController.Instance.cameraController.OnEnterBattleViewMode();
        }
    }

    public void OnEnterLogisticsViewMode()
    {
        inventoryController.SetUIActive(true);
        if (viewMode != ViewMode.Logistics)
        {
            viewMode = ViewMode.Logistics;
            GamePlayController.Instance.cameraController.OnEnterLogisticsViewMode();
        }
    }


    public void OnEnterPreparation()
    {
        OnEnterBattleViewMode();
        championInfoController.OnEnterPreparation();
        UpdateUI();
    }

    public void OnUpdatePreparation()
    {
        if (Input.GetKeyUp(InputController.Instance.viewModeSwitchKey))
        {
            ChangeViewMode();
        }
    }
    public void OnLeavePreparation()
    {

    }

    public void OnEnterCombat()
    {
        OnEnterBattleViewMode();
        championInfoController.OnEnterCombat();
    }
    public void OnUpdateCombat()
    {

    }
    public void OnLeaveCombat()
    {

    }

    public void OnEnterLoss()
    {

    }
    public void OnUpdateLoss()
    {

    }
    public void OnLeaveLoss()
    {

    }
}
