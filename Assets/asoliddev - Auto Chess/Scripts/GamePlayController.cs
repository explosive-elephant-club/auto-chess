using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Reflection;
using General;
using ExcelConfig;

public enum GameStage { Preparation, Combat, Loss };

public interface IGameStage
{
    void OnEnterPreparation();
    void OnEnterCombat();
    void OnEnterLoss();
    void OnUpdatePreparation();
    void OnUpdateCombat();
    void OnUpdateLoss();
    void OnLeavePreparation();
    void OnLeaveCombat();
    void OnLeaveLoss();
}

/// <summary>
/// Controlls most of the game logic and player interactions
/// </summary>
public class GamePlayController : CreateSingleton<GamePlayController>, IGameStage
{
    /// <summary>
    /// 当前的游戏阶段，表示游戏当前处于哪个状态（准备、战斗或失败）
    /// </summary>
    public GameStage currentGameStage;
    /// <summary>
    /// 记录上一个游戏阶段
    /// </summary>
    public GameStage lastStage;
    /// <summary>
    /// 用于计时的变量，特别是用于战斗阶段的倒计时
    /// </summary>
    public float timer = 0;
    /// <summary>
    /// 标记游戏初始化是否准备好
    /// </summary>
    private bool isReady = false;

    /// <summary>
    /// 用于状态委托的广播和监听
    /// </summary>
    public EventCenter gameStageEventCenter = new EventCenter();
    /// <summary>
    /// 玩家单位管理器
    /// </summary>
    public ChampionManager ownChampionManager;
    /// <summary>
    /// 敌人单位管理器
    /// </summary>
    public ChampionManager oponentChampionManager;
    /// <summary>
    /// 主镜头控制
    /// </summary>
    public MainCameraController mainCameraController;
    /// <summary>
    /// 单位组装视角镜头控制
    /// </summary>
    public GOToUICameraController _GOToUICameraController;
    /// <summary>
    /// 被选中的单位
    /// </summary>
    public ChampionController pickedChampion = null;

    /// <summary>
    /// 当地图准备好后，设置isReady = true并进入准备阶段
    /// </summary>
    public void OnMapReady()
    {
        isReady = true;
        StageChange(GameStage.Preparation);
        //Time.timeScale = .2f;
    }

    protected override void InitSingleton()
    {
        isReady = false;
        currentGameStage = GameStage.Preparation;
        lastStage = GameStage.Preparation;
    }

    void Start()
    {
        AddStageListener(this);
        AddStageListener(oponentChampionManager);
        AddStageListener(ownChampionManager);
        AddStageListener(UIController.Instance);
    }

    /// Update is called once per frame
    void Update()
    {
        if (isReady)
            gameStageEventCenter.Broadcast("OnUpdate" + currentGameStage.ToString());
    }

    /// <summary>
    /// 计算玩家每回合应该获得的金币，收入包括基础金币和利息金币
    /// </summary>
    private int CalculateIncome()
    {
        int income = 0;

        //banked gold
        int bank = (int)(GameData.Instance.currentGold / 10);

        income += GameConfig.Instance.baseGoldIncome;
        income += bank;

        return income;
    }

    /// <summary>
    /// 重置整个游戏
    /// </summary>
    public void RestartGame()
    {
        ownChampionManager.Reset();
        oponentChampionManager.Reset();
        GameData.Instance.currentHP = 100;
        GameData.Instance.currentGold = 0;
        StageChange(GameStage.Preparation);

        UIController.Instance.ShowGameScreen();
    }


    /// <summary>
    /// 结束当前回合，减少计时器，提前结束战斗阶段
    /// </summary>
    public void EndRound()
    {
        timer = GameConfig.Instance.combatStageDuration - 3; //reduce timer so game ends fast
    }


    /// <summary>
    /// 获取单位的羁绊buff
    /// </summary>
    /// <param name="constructorData">单位数据</param>
    public List<ConstructorBonusType> GetAllChampionTypes(ConstructorBaseData constructorData)
    {
        List<ConstructorBonusType> types = new List<ConstructorBonusType>();
        types.Add(GameExcelConfig.Instance._eeDataManager.Get<ConstructorBonusType>(constructorData.property1));
        types.Add(GameExcelConfig.Instance._eeDataManager.Get<ConstructorBonusType>(constructorData.property2));
        types.Add(GameExcelConfig.Instance._eeDataManager.Get<ConstructorBonusType>(constructorData.property3));
        return types;
    }

    /// <summary>
    /// 事件中心通过字符串监听不同阶段的事件
    /// </summary>
    /// <param name="gameStageClass">实现IGameStage的类</param>
    public void AddStageListener(IGameStage gameStageClass)
    {
        gameStageEventCenter.AddListener("OnEnterPreparation", gameStageClass.OnEnterPreparation);
        gameStageEventCenter.AddListener("OnEnterCombat", gameStageClass.OnEnterCombat);
        gameStageEventCenter.AddListener("OnEnterLoss", gameStageClass.OnEnterLoss);
        gameStageEventCenter.AddListener("OnUpdatePreparation", gameStageClass.OnUpdatePreparation);
        gameStageEventCenter.AddListener("OnUpdateCombat", gameStageClass.OnUpdateCombat);
        gameStageEventCenter.AddListener("OnUpdateLoss", gameStageClass.OnUpdateLoss);
        gameStageEventCenter.AddListener("OnLeavePreparation", gameStageClass.OnLeavePreparation);
        gameStageEventCenter.AddListener("OnLeaveCombat", gameStageClass.OnLeaveCombat);
        gameStageEventCenter.AddListener("OnLeaveLoss", gameStageClass.OnLeaveLoss);
    }

    /// <summary>
    /// 事件中心通过字符串解绑不同阶段的事件
    /// </summary>
    /// <param name="gameStageClass">实现IGameStage的类</param>
    public void RemoveStageListener(IGameStage gameStageClass)
    {
        gameStageEventCenter.AddListener("OnEnterPreparation", gameStageClass.OnEnterPreparation);
        gameStageEventCenter.AddListener("OnEnterCombat", gameStageClass.OnEnterCombat);
        gameStageEventCenter.AddListener("OnEnterLoss", gameStageClass.OnEnterLoss);
        gameStageEventCenter.AddListener("OnUpdatePreparation", gameStageClass.OnUpdatePreparation);
        gameStageEventCenter.AddListener("OnUpdateCombat", gameStageClass.OnUpdateCombat);
        gameStageEventCenter.AddListener("OnUpdateLoss", gameStageClass.OnUpdateLoss);
        gameStageEventCenter.AddListener("OnLeavePreparation", gameStageClass.OnLeavePreparation);
        gameStageEventCenter.AddListener("OnLeaveCombat", gameStageClass.OnLeaveCombat);
        gameStageEventCenter.AddListener("OnLeaveLoss", gameStageClass.OnLeaveLoss);
    }

    /// <summary>
    /// 当游戏阶段发生变化时，广播当前阶段的离开事件并进入下一个阶段
    /// </summary>
    /// <param name="nextStage">下一个阶段</param>
    public void StageChange(GameStage nextStage)
    {
        gameStageEventCenter.Broadcast("OnLeave" + currentGameStage.ToString());
        lastStage = currentGameStage;
        currentGameStage = nextStage;
        gameStageEventCenter.Broadcast("OnEnter" + currentGameStage.ToString());
    }

    /// <summary>
    /// 更新玩家选中的单位，设置UI层级，切换摄像机视角
    /// </summary>
    /// <param name="championCtrl">单位控制脚本</param>
    public void SetPickedChampion(ChampionController championCtrl)
    {
        if (pickedChampion != null)
        {
            foreach (Transform tran in pickedChampion.GetChassisConstructor().GetComponentsInChildren<Transform>())
            {
                tran.gameObject.layer = 0;
            }
        }
        pickedChampion = championCtrl;
        if (pickedChampion != null)
        {
            foreach (Transform tran in pickedChampion.GetChassisConstructor().GetComponentsInChildren<Transform>())
            {
                tran.gameObject.layer = 9;
            }
            _GOToUICameraController.ResetCam(pickedChampion.transform);
        }


    }

    /// <summary>
    /// 用于处理玩家选择单位的逻辑，检测鼠标点击并更新选中的单位
    /// </summary>
    public void PickChampion()
    {
        if (InputController.Instance.ui == null)
        {
            UIController.Instance.isSlotUIDragged = false;
            //get trigger info
            ChampionController championCtrl = InputController.Instance.champion;
            //if mouse cursor on trigger
            if (championCtrl != null)
            {
                SetPickedChampion(championCtrl);
                UIController.Instance.championInfoController.UpdateUI();
                UIController.Instance.constructorAssembleController.UpdateUI();
                return;

            }
            SetPickedChampion(null);
            UIController.Instance.championInfoController.UpdateUI();
            UIController.Instance.constructorAssembleController.UpdateUI();
        }
        else
        {
            UIController.Instance.isSlotUIDragged = true;
        }
    }

    /// <summary>
    /// 获取部件的图标
    /// </summary>
    /// <param name="constructorBaseData">部件的数据</param>
    /// <returns></returns>
    public string GetConstructorIconPath(ConstructorBaseData constructorBaseData)
    {
        string iconPath = constructorBaseData.prefab.Substring(0, constructorBaseData.prefab.IndexOf(constructorBaseData.type));
        iconPath = "Prefab/Constructor/" + iconPath + constructorBaseData.type + "/Icon/";
        string namePath = constructorBaseData.prefab.Substring(constructorBaseData.prefab.IndexOf(constructorBaseData.type) + constructorBaseData.type.Length + 1);
        return iconPath + namePath;
    }

    #region StageFuncs
    public void OnEnterPreparation()
    {
        //增加金币
        GameData.Instance.currentGold += CalculateIncome();

        //并检查玩家的血量是否为零。如果玩家血量为零，则进入失败阶段
        if (GameData.Instance.currentHP <= 0)
        {
            currentGameStage = GameStage.Loss;
            StageChange(GameStage.Loss);
        }

    }
    public void OnUpdatePreparation()
    {
        //监听鼠标点击来选择单位，并启动单位的拖拽操作
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PickChampion();
            GamePlayController.Instance.ownChampionManager.StartDrag();
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            GamePlayController.Instance.ownChampionManager.StopDrag();
        }
    }
    public void OnLeavePreparation()
    {
        //重置计时器
        timer = 0;
    }

    public void OnEnterCombat()
    {
    }
    
    public void OnUpdateCombat()
    {
        //更新时间
        timer += Time.deltaTime;
        

        //如果计时器到达最大战斗时长，重新进入准备阶段
        if (timer > GameConfig.Instance.combatStageDuration)
        {
            StageChange(GameStage.Preparation);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PickChampion();
        }

    }
    public void OnLeaveCombat()
    {
        timer = 0;
        //增加地图等级
        GameData.Instance.mapLevel++;
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
    #endregion












}
