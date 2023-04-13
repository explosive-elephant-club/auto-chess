using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using General;

public enum GameStage { Preparation, Combat, Loss };

/// <summary>
/// Controlls most of the game logic and player interactions
/// </summary>
public class GamePlayController : CreateSingleton<GamePlayController>, GameStageInterface
{
    public GameStage currentGameStage;
    public GameStage lastStage;
    private float timer = 0;

    private bool isReady = false;


    ///The time available to place champions
    public int PreparationStageDuration = 16;
    ///Maximum time the combat stage can last
    public int CombatStageDuration = 60;
    ///base gold value to get after every round
    public int baseGoldIncome = 5;


    public int currentGold = 0;
    [HideInInspector]
    public int currentHP = 100;
    [HideInInspector]
    public int timerDisplay = 0;

    public EventCenter eventCenter = new EventCenter();
    public OwnChampionManager ownChampionManager;
    public OponentChampionManager oponentChampionManager;

    public void OnMapReady()
    {
        isReady = true;
        StageChange(GameStage.Preparation);
    }

    protected override void InitSingleton()
    {
        isReady = false;
        currentGameStage = GameStage.Preparation;
        lastStage = GameStage.Preparation;
    }

    void Start()
    {
        StageStateAddListener(GetComponent<GameStageInterface>());
        StageStateAddListener(oponentChampionManager.GetComponent<GameStageInterface>());
        StageStateAddListener(ownChampionManager.GetComponent<GameStageInterface>());
        UIController.Instance.UpdateUI();
    }

    /// Update is called once per frame
    void Update()
    {
        if (isReady)
            eventCenter.Broadcast("OnUpdate" + currentGameStage.ToString());
    }


    /// <summary>
    /// Adds champion from shop to inventory
    /// </summary>
    public bool BuyChampionFromShop(Champion champion)
    {
        if (currentGold < champion.cost)
            return false;


        if (ownChampionManager.AddChampionToInventory(champion))
        {
            currentGold -= champion.cost;
            UIController.Instance.UpdateUI();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the number of gold we should recieve
    /// </summary>
    /// <returns></returns>
    private int CalculateIncome()
    {
        int income = 0;

        //banked gold
        int bank = (int)(currentGold / 10);

        income += baseGoldIncome;
        income += bank;

        return income;
    }

    /// <summary>
    /// Incrases the available champion slots by 1
    /// </summary>
    public void Buylvl()
    {
        //return if we dont have enough gold
        if (currentGold < 4)
            return;

        if (ownChampionManager.currentChampionLimit < 9)
        {
            //incrase champion limit
            ownChampionManager.currentChampionLimit++;

            //decrase gold
            currentGold -= 4;

            //update ui
            UIController.Instance.UpdateUI();

        }

    }

    /// <summary>
    /// Called when round was lost
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        UIController.Instance.UpdateUI();

    }

    /// <summary>
    /// Called when Game was lost
    /// </summary>
    public void RestartGame()
    {
        ownChampionManager.Reset();
        oponentChampionManager.Reset();
        currentHP = 100;
        currentGold = 0;
        StageChange(GameStage.Preparation);

        UIController.Instance.ShowGameScreen();
    }


    /// <summary>
    /// Ends the round
    /// </summary>
    public void EndRound()
    {
        timer = CombatStageDuration - 3; //reduce timer so game ends fast
    }

    //不同状态stage命令模式绑定事件
    public void StageStateAddListener(GameStageInterface gameStageFunc)
    {
        eventCenter.AddListener("OnEnterPreparation", gameStageFunc.OnEnterPreparation);
        eventCenter.AddListener("OnEnterCombat", gameStageFunc.OnEnterCombat);
        eventCenter.AddListener("OnEnterLoss", gameStageFunc.OnEnterLoss);
        eventCenter.AddListener("OnUpdatePreparation", gameStageFunc.OnUpdatePreparation);
        eventCenter.AddListener("OnUpdateCombat", gameStageFunc.OnUpdateCombat);
        eventCenter.AddListener("OnUpdateLoss", gameStageFunc.OnUpdateLoss);
        eventCenter.AddListener("OnLeavePreparation", gameStageFunc.OnLeavePreparation);
        eventCenter.AddListener("OnLeaveCombat", gameStageFunc.OnLeaveCombat);
        eventCenter.AddListener("OnLeaveLoss", gameStageFunc.OnLeaveLoss);
    }

    public void StageStateRemoveListener(GameStageInterface gameStageFunc)
    {
        eventCenter.RemoveListener("OnEnterPreparation", gameStageFunc.OnEnterPreparation);
        eventCenter.RemoveListener("OnEnterCombat", gameStageFunc.OnEnterCombat);
        eventCenter.RemoveListener("OnEnterLoss", gameStageFunc.OnEnterLoss);
        eventCenter.RemoveListener("OnUpdatePreparation", gameStageFunc.OnUpdatePreparation);
        eventCenter.RemoveListener("OnUpdateCombat", gameStageFunc.OnUpdateCombat);
        eventCenter.RemoveListener("OnUpdateLoss", gameStageFunc.OnUpdateLoss);
        eventCenter.RemoveListener("OnLeavePreparation", gameStageFunc.OnLeavePreparation);
        eventCenter.RemoveListener("OnLeaveCombat", gameStageFunc.OnLeaveCombat);
        eventCenter.RemoveListener("OnLeaveLoss", gameStageFunc.OnLeaveLoss);
    }


    public void StageChange(GameStage nextStage)
    {
        eventCenter.Broadcast("OnLeave" + currentGameStage.ToString());
        lastStage = currentGameStage;
        currentGameStage = nextStage;
        eventCenter.Broadcast("OnEnter" + currentGameStage.ToString());
    }

    #region StageFuncs
    public void OnEnterPreparation()
    {
        UIController.Instance.SetTimerTextActive(true);
        currentGold += CalculateIncome();
        UIController.Instance.UpdateUI();
        ChampionShop.Instance.RefreshShop(true);

        if (currentHP <= 0)
        {
            currentGameStage = GameStage.Loss;
            StageChange(GameStage.Loss);
        }

    }
    public void OnUpdatePreparation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GamePlayController.Instance.ownChampionManager.StartDrag();
        }
        if (Input.GetMouseButtonUp(0))
        {
            GamePlayController.Instance.ownChampionManager.StopDrag();
        }


        timer += Time.deltaTime;
        timerDisplay = (int)(PreparationStageDuration - timer);
        UIController.Instance.UpdateTimerText();
        if (timer > PreparationStageDuration)
        {
            timer = 0;
            StageChange(GameStage.Combat);
        }
    }
    public void OnLeavePreparation()
    {

    }

    public void OnEnterCombat()
    {
        //Map.Instance.HideIndicators();
        UIController.Instance.SetTimerTextActive(false);
        if (ownChampionManager.IsAllChampionDead())
            EndRound();

    }
    public void OnUpdateCombat()
    {
        timer += Time.deltaTime;
        timerDisplay = (int)timer;
        if (timer > CombatStageDuration)
        {
            timer = 0;
            StageChange(GameStage.Preparation);
        }
    }
    public void OnLeaveCombat()
    {

    }

    public void OnEnterLoss()
    {
        UIController.Instance.ShowLossScreen();

    }
    public void OnUpdateLoss()
    {

    }
    public void OnLeaveLoss()
    {

    }
    #endregion












}
