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

    public Dictionary<ChampionType, int> championTypeCount;
    public List<int> bonusBuffList;


    private ChampionController draggedChampion = null;
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

    public bool AddChampionToInventory(ChampionBaseData champion)
    {
        GridInfo emptyGrid = Map.Instance.GetEmptySlot(team, GridType.Inventory);
        if (emptyGrid == null)
            return false;

        //instantiate champion prefab
        GameObject championPrefab = Instantiate(Resources.Load<GameObject>(champion.prefab));

        //get championController
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        //store champion in inventory array
        StoreChampionInArray(emptyGrid, championController);

        //setup chapioncontroller
        championController.Init(champion, team, this);

        //set position and rotation
        championController.SetWorldPosition();
        championController.SetWorldRotation();





        //only upgrade when in preparation stage
        if (GamePlayController.Instance.currentGameStage == GameStage.Preparation)
            TryUpgradeChampion(champion); //upgrade champion

        //set gold on ui
        UIController.Instance.UpdateUI();

        //return true if succesful buy
        return true;
    }

    public bool AddChampionToBattle(ChampionBaseData champion)
    {
        GridInfo emptyGrid = Map.Instance.GetEmptySlot(team, GridType.HexaMap);
        if (emptyGrid == null)
            return false;

        GameObject championPrefab = Instantiate(Resources.Load<GameObject>(champion.prefab));

        //get championController
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        StoreChampionInArray(emptyGrid, championController);

        //setup chapioncontroller
        championController.Init(champion, team, this);

        //set position and rotation
        championController.SetWorldPosition();
        championController.SetWorldRotation();

        //only upgrade when in preparation stage
        if (GamePlayController.Instance.currentGameStage == GameStage.Preparation)
            TryUpgradeChampion(champion); //upgrade champion

        //set gold on ui
        UIController.Instance.UpdateUI();
        CalculateBonuses();
        return true;
    }

    private void TryUpgradeChampion(ChampionBaseData champion)
    {
        //check for champion upgrade
        List<ChampionController> championList_lvl_1 = new List<ChampionController>();
        List<ChampionController> championList_lvl_2 = new List<ChampionController>();
        foreach (ChampionController championCtrl in championInventoryArray)
        {
            if (championCtrl != null)
            {
                if (championCtrl.champion == champion)
                {
                    if (championCtrl.lvl == 1)
                        championList_lvl_1.Add(championCtrl);
                    else if (championCtrl.lvl == 2)
                        championList_lvl_2.Add(championCtrl);
                }
            }
        }
        foreach (ChampionController championCtrl in championsHexaMapArray)
        {
            if (championCtrl != null)
            {
                if (championCtrl.champion == champion)
                {
                    if (championCtrl.lvl == 1)
                        championList_lvl_1.Add(championCtrl);
                    else if (championCtrl.lvl == 2)
                        championList_lvl_2.Add(championCtrl);
                }
            }
        }

        //if we have 3 we upgrade a champion and delete rest
        if (championList_lvl_1.Count > 2)
        {
            //upgrade
            championList_lvl_1[2].UpgradeLevel();

            //remove from array
            RemoveChampionFromArray(championList_lvl_1[0]);
            RemoveChampionFromArray(championList_lvl_1[1]);

            //destroy gameobjects
            championList_lvl_1[0].OnRemove();
            championList_lvl_1[1].OnRemove();
            Destroy(championList_lvl_1[0].gameObject);
            Destroy(championList_lvl_1[1].gameObject);

            //we upgrade to lvl 3
            if (championList_lvl_2.Count > 1)
            {
                //upgrade
                championList_lvl_1[2].UpgradeLevel();

                //remove from array
                RemoveChampionFromArray(championList_lvl_2[0]);
                RemoveChampionFromArray(championList_lvl_2[1]);

                //destroy gameobjects
                championList_lvl_1[0].OnRemove();
                championList_lvl_1[1].OnRemove();
                Destroy(championList_lvl_2[0].gameObject);
                Destroy(championList_lvl_2[1].gameObject);
            }
        }



        currentChampionCount = championsHexaMapArray.Count;

        //update ui
        UIController.Instance.UpdateUI();

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

    public void StartDrag()
    {
        //get trigger info
        GridInfo gridInfo = InputController.Instance.gridInfo;
        //if mouse cursor on trigger
        if (gridInfo != null)
        {
            dragStartGridInfo = gridInfo;
            ChampionController championCtrl = gridInfo.occupyChampion;
            if (championCtrl != null)
            {
                //show indicators
                Map.Instance.ShowIndicators(team);
                draggedChampion = championCtrl;
                championCtrl.IsDragged = true;
            }

        }
    }

    public void StopDrag()
    {
        //hide indicators
        //Map.Instance.HideIndicators();

        if (draggedChampion != null)
        {
            //set dragged
            draggedChampion.IsDragged = false;

            //get trigger info
            GridInfo gridInfo = InputController.Instance.gridInfo;

            //if mouse cursor on trigger
            if (gridInfo != null)
            {
                if (!CheckGridInfoInRange(gridInfo, draggedChampion.team))
                    return;
                //get current champion over mouse cursor
                ChampionController championCtrl = gridInfo.occupyChampion;

                //目标点有单位
                if (championCtrl != null)
                {
                    //交换位置
                    championCtrl.LeaveGrid();
                    draggedChampion.LeaveGrid();
                    championCtrl.EnterGrid(dragStartGridInfo);
                    draggedChampion.EnterGrid(gridInfo);

                }
                else//目标点无单位
                {
                    //目标点是战场
                    if (gridInfo.gridType == GridType.HexaMap)
                    {
                        if (championsHexaMapArray.Count < currentChampionLimit || dragStartGridInfo.gridType == GridType.HexaMap)
                        {
                            RemoveChampionFromArray(draggedChampion);
                            StoreChampionInArray(gridInfo, draggedChampion);
                        }
                    } //目标点是仓库
                    else if (gridInfo.gridType == GridType.Inventory)
                    {
                        RemoveChampionFromArray(draggedChampion);
                        StoreChampionInArray(gridInfo, draggedChampion);
                    }
                }
            }


            CalculateBonuses();
            currentChampionCount = championsHexaMapArray.Count;

            //update ui
            UIController.Instance.UpdateUI();
            draggedChampion = null;
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

    private void CalculateBonuses()
    {
        //init dictionary
        championTypeCount = new Dictionary<ChampionType, int>();


        foreach (ChampionController championCtrl in championsHexaMapArray)
        {

            //there is a champion
            if (championCtrl != null)
            {
                //get champion
                ChampionBaseData c = championCtrl.champion;

                List<ChampionType> types = GamePlayController.Instance.GetAllChampionTypes(c);
                foreach (ChampionType t in types)
                {
                    if (championTypeCount.ContainsKey(t))
                    {
                        int cCount = 0;
                        championTypeCount.TryGetValue(t, out cCount);

                        cCount++;

                        championTypeCount[t] = cCount;

                    }
                    else
                    {
                        championTypeCount.Add(t, 1);
                    }
                }
            }

        }

        bonusBuffList.Clear();
        foreach (KeyValuePair<ChampionType, int> m in championTypeCount)
        {
            int buffID = 0;
            foreach (ChampionType.BonusClass b in m.Key.Bonus)
            {
                if (m.Value >= b.count)
                {
                    buffID = b.buff_ID;
                }
                else
                {
                    break;
                }
            }

            //have enough champions to get bonus
            if (buffID != 0)
            {
                bonusBuffList.Add(buffID);
            }
        }

    }

    private void ActiveBonuses()
    {
        /*foreach (BaseBuffData b in bonusBuffList)
        {
            foreach (ChampionController championCtrl in championsHexaMapArray)
            {
                if (championCtrl != null)
                {
                    championCtrl.buffController.AddBuff(b, championCtrl.gameObject);
                }
            }
        }*/
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

    public void OnChampionDeath()
    {
        if (IsAllChampionDead())
        {
            GamePlayController.Instance.EndRound();
        }

    }

    public virtual void OnEnterPreparation()
    {
        for (int i = 0; i < GameData.Instance.championsArray.Count; i++)
        {
            TryUpgradeChampion(GameData.Instance.championsArray[i]);
        }

    }
    public virtual void OnUpdatePreparation()
    {

    }
    public virtual void OnLeavePreparation()
    {

    }

    public virtual void OnEnterCombat()
    {
        if (draggedChampion != null)
        {
            draggedChampion.GetComponent<ChampionController>().IsDragged = false;
            draggedChampion = null;
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