using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using General;

public enum ChampionTeam { Player, Oponent }

public class ChampionManager : MonoBehaviour, GameStageInterface
{
    public ChampionTeam team;

    [HideInInspector]
    public ChampionController[] championInventoryArray;
    [HideInInspector]
    public ChampionController[,] championsHexaMapArray;


    [HideInInspector]
    public int currentChampionLimit = 3;
    [HideInInspector]
    public int currentChampionCount = 0;

    public Dictionary<ChampionType, int> championTypeCount;
    public List<BaseBuffData> bonusBuffList;


    private ChampionController draggedChampion = null;
    private GridInfo dragStartGridInfo = null;

    private void Start()
    {
        championInventoryArray = new ChampionController[Map.inventorySize];
        championsHexaMapArray = new ChampionController[Map.hexMapSizeX, Map.hexMapSizeZ / 2];
    }

    private void StoreChampionInArray(GridInfo gridInfo, ChampionController champion)
    {
        //assign current trigger to champion
        ChampionController championController = champion.GetComponent<ChampionController>();

        championController.SetOccupyGridInfo(gridInfo);
        if (gridInfo.gridType == GridType.Inventory)
        {
            championInventoryArray[(int)gridInfo.index.x] = champion;
        }
        else if (gridInfo.gridType == GridType.HexaMap)
        {
            if (team == ChampionTeam.Player)
                championsHexaMapArray[(int)gridInfo.index.x, (int)gridInfo.index.y] = champion;
            else if (team == ChampionTeam.Oponent)
                championsHexaMapArray[(int)gridInfo.index.x, (int)gridInfo.index.y - 4] = champion;
        }
    }

    private void RemoveChampionFromArray(GridInfo gridInfo)
    {

        if (gridInfo.gridType == GridType.Inventory)
        {
            championInventoryArray[(int)gridInfo.index.x].SetOccupyGridInfo();
            championInventoryArray[(int)gridInfo.index.x] = null;
        }
        else if (gridInfo.gridType == GridType.HexaMap)
        {


            if (team == ChampionTeam.Player)
            {
                championsHexaMapArray[(int)gridInfo.index.x, (int)gridInfo.index.y].SetOccupyGridInfo();
                championsHexaMapArray[(int)gridInfo.index.x, (int)gridInfo.index.y] = null;
            }
            else if (team == ChampionTeam.Oponent)
            {
                championsHexaMapArray[(int)gridInfo.index.x, (int)gridInfo.index.y - 4].SetOccupyGridInfo();
                championsHexaMapArray[(int)gridInfo.index.x, (int)gridInfo.index.y - 4] = null;
            }
        }
    }

    private void GetEmptySlot(out int emptyIndexX, out int emptyIndexZ)
    {
        emptyIndexX = -1;
        emptyIndexZ = -1;

        //get first empty inventory slot
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                if (championsHexaMapArray[x, z] == null)
                {
                    emptyIndexX = x;
                    if (team == ChampionTeam.Player)
                        emptyIndexZ = z;
                    else if (team == ChampionTeam.Oponent)
                        emptyIndexZ = z + 4;
                    break;
                }
            }
        }
    }

    private int GetChampionCountOnHexGrid()
    {
        int count = 0;
        foreach (ChampionController championOBJ in championsHexaMapArray)
        {
            if (championOBJ != null)
            {
                count++;
            }
        }
        return count;
    }

    private ChampionController GetChampionFromGridInfo(GridInfo gridInfo)
    {
        foreach (var champion in championsHexaMapArray)
        {
            if (champion != null)
                if (champion.occupyGridInfo == gridInfo)
                    return champion;
        }
        foreach (var champion in championInventoryArray)
        {
            if (champion != null)
                if (champion.occupyGridInfo == gridInfo)
                    return champion;
        }
        return null;
    }

    public bool AddChampionToInventory(Champion champion)
    {
        //get first empty inventory slot
        int emptyIndex = -1;
        for (int i = 0; i < championInventoryArray.Length; i++)
        {
            if (championInventoryArray[i] == null)
            {
                emptyIndex = i;
                break;
            }
        }

        //return if no slot to add champion
        if (emptyIndex == -1)
            return false;

        //instantiate champion prefab
        GameObject championPrefab = Instantiate(champion.prefab);

        //get championController
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        //store champion in inventory array
        StoreChampionInArray(Map.Instance.ownInventoryGridArray[emptyIndex], championController);

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

    public bool AddChampionToBattle(Champion champion)
    {
        int indexX;
        int indexZ;
        GetEmptySlot(out indexX, out indexZ);

        //dont add champion if there is no empty slot
        if (indexX == -1 || indexZ == -1)
            return false;

        GameObject championPrefab = Instantiate(champion.prefab);

        //get championController
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        StoreChampionInArray(Map.Instance.mapGridArray[indexX, indexZ], championController);

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

    private void TryUpgradeChampion(Champion champion)
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
            RemoveChampionFromArray(championList_lvl_1[0].occupyGridInfo);
            RemoveChampionFromArray(championList_lvl_1[1].occupyGridInfo);

            //destroy gameobjects
            Destroy(championList_lvl_1[0].gameObject);
            Destroy(championList_lvl_1[1].gameObject);

            //we upgrade to lvl 3
            if (championList_lvl_2.Count > 1)
            {
                //upgrade
                championList_lvl_1[2].UpgradeLevel();

                //remove from array
                RemoveChampionFromArray(championList_lvl_2[0].occupyGridInfo);
                RemoveChampionFromArray(championList_lvl_2[1].occupyGridInfo);

                //destroy gameobjects
                Destroy(championList_lvl_2[0].gameObject);
                Destroy(championList_lvl_2[1].gameObject);
            }
        }



        currentChampionCount = GetChampionCountOnHexGrid();

        //update ui
        UIController.Instance.UpdateUI();

    }

    public ChampionController FindTarget(ChampionController _championController, int bestDistance)
    {
        ChampionController closestTarget = null;
        foreach (ChampionController championCtrl in championsHexaMapArray)
        {
            if (championCtrl != null)
            {
                closestTarget = championCtrl.GetComponent<ChampionController>();

                if (closestTarget.isDead == false)
                {
                    int distance = _championController.occupyGridInfo.GetDistance(closestTarget.occupyGridInfo);
                    if (distance <= bestDistance)
                    {
                        return closestTarget;
                    }
                }
            }
        }
        return null;
    }

    public void StartDrag()
    {
        //get trigger info
        GridInfo gridInfo = InputController.Instance.gridInfo;
        //if mouse cursor on trigger
        if (gridInfo != null)
        {
            dragStartGridInfo = gridInfo;
            ChampionController championCtrl = GetChampionFromGridInfo(gridInfo);
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

        int championsOnField = GetChampionCountOnHexGrid();


        if (draggedChampion != null)
        {
            //set dragged
            draggedChampion.IsDragged = false;

            //get trigger info
            GridInfo gridInfo = InputController.Instance.gridInfo;

            //if mouse cursor on trigger
            if (gridInfo != null)
            {
                //get current champion over mouse cursor
                ChampionController championCtrl = GetChampionFromGridInfo(gridInfo);

                //目标点有单位
                if (championCtrl != null)
                {
                    //交换位置
                    if (gridInfo.gridType == GridType.Inventory)
                    {
                        championCtrl.occupyGridInfo = null;
                        draggedChampion.occupyGridInfo = null;
                        StoreChampionInArray(dragStartGridInfo, championCtrl);
                        StoreChampionInArray(gridInfo, draggedChampion);
                    }
                }
                else//目标点无单位
                {
                    //目标点是战场
                    if (gridInfo.gridType == GridType.HexaMap)
                    {
                        if (championsOnField < currentChampionLimit || dragStartGridInfo.gridType == GridType.HexaMap)
                        {
                            RemoveChampionFromArray(dragStartGridInfo);
                            StoreChampionInArray(gridInfo, draggedChampion);

                            if (dragStartGridInfo.gridType != GridType.HexaMap)
                                championsOnField++;
                        }
                    } //目标点是仓库
                    else if (gridInfo.gridType == GridType.Inventory)
                    {
                        RemoveChampionFromArray(dragStartGridInfo);
                        StoreChampionInArray(gridInfo, draggedChampion);

                        if (dragStartGridInfo.gridType == GridType.HexaMap)
                            championsOnField--;
                    }
                }
            }


            CalculateBonuses();
            currentChampionCount = GetChampionCountOnHexGrid();

            //update ui
            UIController.Instance.UpdateUI();
            draggedChampion = null;
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
                Champion c = championCtrl.champion;

                if (championTypeCount.ContainsKey(c.type1))
                {
                    int cCount = 0;
                    championTypeCount.TryGetValue(c.type1, out cCount);

                    cCount++;

                    championTypeCount[c.type1] = cCount;
                }
                else
                {
                    championTypeCount.Add(c.type1, 1);
                }

                if (championTypeCount.ContainsKey(c.type2))
                {
                    int cCount = 0;
                    championTypeCount.TryGetValue(c.type2, out cCount);

                    cCount++;

                    championTypeCount[c.type2] = cCount;
                }
                else
                {
                    championTypeCount.Add(c.type2, 1);
                }

            }

        }

        bonusBuffList = new List<BaseBuffData>();
        foreach (KeyValuePair<ChampionType, int> m in championTypeCount)
        {
            ChampionBonus championBonus = m.Key.championBonus;
            BaseBuffData buff = championBonus.GetBuffBonus(m.Value);
            //have enough champions to get bonus
            if (buff != null)
            {
                bonusBuffList.Add(buff);
            }
        }

    }

    private void ActiveBonuses()
    {
        foreach (BaseBuffData b in bonusBuffList)
        {
            foreach (ChampionController championCtrl in championsHexaMapArray)
            {
                if (championCtrl != null)
                {
                    championCtrl.buffController.AddBuff(b, championCtrl.gameObject);
                }
            }
        }
    }

    public bool IsAllChampionDead()
    {
        int championCount = 0;
        int championDead = 0;
        //start own champion combat

        foreach (ChampionController championCtrl in championsHexaMapArray)
        {
            //there is a champion
            if (championCtrl != null)
            {
                championCount++;
                if (championCtrl.isDead)
                    championDead++;

            }
        }

        if (championDead == championCount)
            return true;

        return false;
    }

    public void Reset()
    {
        for (int i = 0; i < championInventoryArray.Length; i++)
        {
            if (championInventoryArray[i] != null)
            {
                Destroy(championInventoryArray[i].gameObject);
                championInventoryArray[i] = null;
            }

        }
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (championsHexaMapArray[x, z] != null)
                {
                    Destroy(championsHexaMapArray[x, z].gameObject);
                    championsHexaMapArray[x, z] = null;
                }
            }
        }
        currentChampionLimit = 3;
        currentChampionCount = GetChampionCountOnHexGrid();
    }

    /// <summary>
    /// Called when a champion killd
    /// </summary>
    public void OnChampionDeath()
    {
        if (IsAllChampionDead())
            GamePlayController.Instance.EndRound();
    }

    public virtual void OnEnterPreparation()
    {
        for (int i = 0; i < GameData.Instance.championsArray.Length; i++)
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