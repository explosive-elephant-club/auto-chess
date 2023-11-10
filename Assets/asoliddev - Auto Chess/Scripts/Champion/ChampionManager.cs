using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using General;
using ExcelConfig;

public enum ChampionTeam { Player, Oponent }

public class ChampionManager : MonoBehaviour
{
    public ChampionTeam team;

    public List<ChampionController> championInventoryArray;
    public List<ChampionController> championsHexaMapArray;


    [HideInInspector]
    public int currentChampionLimit = 3;
    [HideInInspector]
    public int currentChampionCount = 0;

    public ChampionController pickedChampion = null;
    private GridInfo dragStartGridInfo = null;

    public Dictionary<string, CallBack> gameStageActions = new Dictionary<string, CallBack>();

    private void Awake()
    {
        InitStageDic();
    }


    private void Start()
    {
        championInventoryArray = new List<ChampionController>();
        championsHexaMapArray = new List<ChampionController>();
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

    private void StoreChampionInArray(GridInfo gridInfo, ChampionController champion)
    {
        champion.EnterGrid(gridInfo);
        if (gridInfo.gridType == GridType.Inventory)
        {
            championInventoryArray.Add(champion);
        }
        else if (gridInfo.gridType == GridType.HexaMap)
        {
            championsHexaMapArray.Add(champion);
        }
    }

    private void RemoveChampionFromArray(ChampionController champion)
    {
        GridInfo gridInfo = champion.occupyGridInfo;
        if (gridInfo.gridType == GridType.Inventory)
        {
            championInventoryArray.Remove(champion);
        }
        else if (gridInfo.gridType == GridType.HexaMap)
        {
            championsHexaMapArray.Remove(champion);
        }
        champion.LeaveGrid();
    }

    public bool AddChampionToInventory()
    {
        GridInfo emptyGrid = Map.Instance.GetEmptySlot(team, GridType.Inventory);
        if (emptyGrid == null)
            return false;

        //instantiate champion prefab
        GameObject championPrefab = Instantiate(Resources.Load<GameObject>("Prefab/Champion/Champion"));

        //get championController
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        //store champion in inventory array
        StoreChampionInArray(emptyGrid, championController);

        //setup chapioncontroller
        championController.Init(team, this);

        //set position and rotation
        championController.SetWorldPosition();
        championController.SetWorldRotation();

        //set gold on ui
        UIController.Instance.UpdateUI();

        //return true if succesful buy
        return true;
    }

    public bool AddChampionToBattle(string name)
    {
        GridInfo emptyGrid = Map.Instance.GetEmptySlot(team, GridType.HexaMap);
        if (emptyGrid == null)
            return false;

        GameObject championPrefab = Instantiate(Resources.Load<GameObject>("Prefab/Champion/" + name));

        //get championController
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        StoreChampionInArray(emptyGrid, championController);

        championPrefab.name = championPrefab.name + championsHexaMapArray.Count;
        //setup chapioncontroller
        championController.Init(team, this);

        //set position and rotation
        championController.SetWorldPosition();
        championController.SetWorldRotation();

        championController.CalculateBonuses();

        //set gold on ui
        UIController.Instance.UpdateUI();
        return true;
    }

    public ChampionController FindAnyTargetInRange(ChampionController _championController, int bestDistance)
    {
        foreach (ChampionController championCtrl in championsHexaMapArray)
        {
            if (championCtrl != null)
            {
                if (championCtrl.isDead == false)
                {
                    int distance = _championController.occupyGridInfo.GetDistance(championCtrl.occupyGridInfo);
                    if (distance <= bestDistance)
                    {

                        return championCtrl;
                    }
                }
            }
        }
        return null;
    }

    public ChampionController FindNearestTarget(ChampionController _championController, int bestDistance)
    {
        ChampionController closestTarget = null;
        int tempDis = bestDistance;
        foreach (ChampionController championCtrl in championsHexaMapArray)
        {
            if (championCtrl != null)
            {
                ChampionController targetChampion = championCtrl;
                if (targetChampion.isDead == false)
                {
                    int distance = _championController.occupyGridInfo.GetDistance(targetChampion.occupyGridInfo);
                    if (distance <= bestDistance && distance < tempDis)
                    {
                        tempDis = distance;
                        closestTarget = targetChampion;
                    }
                }
            }
        }
        return closestTarget;
    }

    public ChampionController FindFarthestTarget(ChampionController _championController, int bestDistance)
    {
        ChampionController farthestTarget = null;
        int tempDis = 0;
        foreach (ChampionController championCtrl in championsHexaMapArray)
        {
            if (championCtrl != null)
            {
                ChampionController targetChampion = championCtrl;

                if (targetChampion.isDead == false)
                {
                    int distance = _championController.occupyGridInfo.GetDistance(targetChampion.occupyGridInfo);
                    if (distance <= bestDistance && distance > tempDis)
                    {
                        tempDis = distance;
                        farthestTarget = targetChampion;
                    }
                }
            }
        }
        return farthestTarget;
    }

    public void PickChampion()
    {
        if (InputController.Instance.ui == null)
        {
            //get trigger info
            GridInfo gridInfo = InputController.Instance.gridInfo;
            //if mouse cursor on trigger
            if (gridInfo != null)
            {
                ChampionController championCtrl = gridInfo.occupyChampion;
                if (championCtrl != null)
                {
                    pickedChampion = championCtrl;
                    UIController.Instance.championInfoController.UpdateUI();
                    UIController.Instance.constructorAssembleController.UpdateUI();
                    return;
                }
            }
            pickedChampion = null;
            UIController.Instance.championInfoController.UpdateUI();
            UIController.Instance.constructorAssembleController.UpdateUI();
        }
    }

    public void StartDrag()
    {
        if (pickedChampion != null && InputController.Instance.ui == null)
        {
            dragStartGridInfo = InputController.Instance.gridInfo;
            pickedChampion.IsDragged = true;
        }
    }

    public void StopDrag()
    {
        //hide indicators
        //Map.Instance.HideIndicators();

        if (pickedChampion != null && InputController.Instance.ui == null)
        {
            //set dragged
            pickedChampion.IsDragged = false;

            //get trigger info
            GridInfo gridInfo = InputController.Instance.gridInfo;

            //if mouse cursor on trigger
            if (gridInfo != null)
            {
                if (!CheckGridInfoInRange(gridInfo, pickedChampion.team))
                    return;
                //get current champion over mouse cursor
                ChampionController championCtrl = gridInfo.occupyChampion;

                //目标点有单位
                if (championCtrl != null)
                {
                    //交换位置
                    championCtrl.LeaveGrid();
                    pickedChampion.LeaveGrid();
                    championCtrl.EnterGrid(dragStartGridInfo);
                    pickedChampion.EnterGrid(gridInfo);

                }
                else//目标点无单位
                {
                    //目标点是战场
                    if (gridInfo.gridType == GridType.HexaMap)
                    {
                        if (championsHexaMapArray.Count < currentChampionLimit || dragStartGridInfo.gridType == GridType.HexaMap)
                        {
                            RemoveChampionFromArray(pickedChampion);
                            StoreChampionInArray(gridInfo, pickedChampion);
                        }
                    } //目标点是仓库
                    else if (gridInfo.gridType == GridType.Inventory)
                    {
                        RemoveChampionFromArray(pickedChampion);
                        StoreChampionInArray(gridInfo, pickedChampion);
                    }
                }
            }

            currentChampionCount = championsHexaMapArray.Count;

            //update ui
            UIController.Instance.UpdateUI();
            //draggedChampion = null;
        }
    }

    bool CheckGridInfoInRange(GridInfo grid, ChampionTeam championTeam)
    {
        if (championTeam == ChampionTeam.Player)
        {
            if (grid.gridType == GridType.Inventory)
            {
                return Array.IndexOf(Map.Instance.ownInventoryGridArray, grid) != -1;
            }
            else
            {
                return grid.index.y < Map.hexMapSizeZ / 2;
            }
        }
        else
        {
            if (grid.gridType == GridType.Inventory)
            {
                return Array.IndexOf(Map.Instance.oponentInventoryGridArray, grid) != -1;
            }
            else
            {
                return grid.index.y >= Map.hexMapSizeZ / 2;
            }
        }
    }



    public bool IsAllChampionDead()
    {
        int championDead = 0;
        //start own champion combat

        foreach (ChampionController championCtrl in championsHexaMapArray)
        {
            //there is a champion
            if (championCtrl != null)
            {
                if (championCtrl.isDead)
                    championDead++;

            }
        }

        if (championDead == championsHexaMapArray.Count)
            return true;

        return false;
    }

    public void Reset()
    {
        foreach (var champion in championInventoryArray)
        {
            champion.OnRemove();
            Destroy(champion.gameObject);
        }
        championInventoryArray.Clear();
        foreach (var champion in championsHexaMapArray)
        {
            champion.OnRemove();
            Destroy(champion.gameObject);
        }
        championsHexaMapArray.Clear();
        currentChampionLimit = 3;
        currentChampionCount = 0;
    }

    public void OnChampionDeath(ChampionController championController)
    {
        if (championController == pickedChampion)
        {
            UIController.Instance.championInfoController.UpdateUI();
            UIController.Instance.constructorAssembleController.UpdateUI();
        }
        if (IsAllChampionDead())
        {
            GamePlayController.Instance.EndRound();
        }

    }

    public virtual void OnEnterPreparation()
    {
    }

    public virtual void OnUpdatePreparation()
    {

    }
    public virtual void OnLeavePreparation()
    {

    }

    public virtual void OnEnterCombat()
    {
        if (pickedChampion != null)
        {
            pickedChampion.GetComponent<ChampionController>().IsDragged = false;
            //draggedChampion = null;
        }
        if (IsAllChampionDead())
        {
            GamePlayController.Instance.EndRound();
        }
    }
    public virtual void OnUpdateCombat()
    {

    }
    public virtual void OnLeaveCombat()
    {

    }

    public virtual void OnEnterLoss()
    {

    }
    public virtual void OnUpdateLoss()
    {

    }
    public virtual void OnLeaveLoss()
    {

    }
}