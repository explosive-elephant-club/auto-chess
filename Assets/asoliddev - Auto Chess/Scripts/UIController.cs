using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ExcelConfig;


public class UIController : CreateSingleton<UIController>, IGameStage
{
    //是否正在拖拽UI中的物品
    public bool isSlotUIDragged = false;
    //用于遮挡屏幕或暂停画面
    public GameObject mask;
    public ShopGUIController shopController;
    public LevelInfoController levelInfoController;
    public ChampionInfoController championInfoController;
    public InventoryController inventoryController;
    public ConstructorAssembleController constructorAssembleController;
    public GameObject restartButton;
    public PopupController popupController;

    public RectTransform canvasRoot;


    protected override void InitSingleton()
    {
        levelInfoController = ResourceManager.LoadGameObjectResource("UI/Controller/LevelInfoController", canvasRoot).GetComponent<LevelInfoController>();
        shopController = ResourceManager.LoadGameObjectResource("UI/Controller/ShopGUIController", canvasRoot).GetComponent<ShopGUIController>();
        inventoryController = ResourceManager.LoadGameObjectResource("UI/Controller/InventoryController", canvasRoot).GetComponent<InventoryController>();
        championInfoController = ResourceManager.LoadGameObjectResource("UI/Controller/ChampionInfoController", canvasRoot).GetComponent<ChampionInfoController>();
        constructorAssembleController = ResourceManager.LoadGameObjectResource("UI/Controller/ConstructorAssembleController", canvasRoot).GetComponent<ConstructorAssembleController>();
    }

    /// <summary>
    /// 重新开始按钮
    /// </summary>
    public void Restart_Click()
    {
        GamePlayController.Instance.RestartGame();
    }

    public void UpdateUI()
    {
        levelInfoController.UpdateUI();
        championInfoController.UpdateUI();
        constructorAssembleController.UpdateUI();
    }

    /// <summary>
    /// 显示游戏失败的界面
    /// </summary>
    public void ShowLossScreen()
    {
        mask.SetActive(true);
        restartButton.SetActive(true);
    }

    /// <summary>
    /// 显示游戏的主界面
    /// </summary>
    public void ShowGameScreen()
    {
        mask.SetActive(false);
        restartButton.SetActive(false);
    }


    public void OnEnterPreparation()
    {
        levelInfoController.OnEnterPreparation();
        championInfoController.OnEnterPreparation();
        shopController.OnEnterPreparation();
        UpdateUI();
    }

    public void OnUpdatePreparation()
    {

    }
    public void OnLeavePreparation()
    {
        shopController.OnLeavePreparation();
    }

    public void OnEnterCombat()
    {
        levelInfoController.OnEnterCombat();
        championInfoController.OnEnterCombat();

    }
    public void OnUpdateCombat()
    {
        //更新UI上的战斗计时器
        levelInfoController.UpdateCombatTimer((int)(GameConfig.Instance.combatStageDuration - GamePlayController.Instance.timer));

        championInfoController.OnUpdateCombat();

    }
    public void OnLeaveCombat()
    {

    }

    public void OnEnterLoss()
    {
        ShowLossScreen();
    }
    public void OnUpdateLoss()
    {

    }
    public void OnLeaveLoss()
    {

    }
}
