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
    public GameObject[] championInventoryArray;
    [HideInInspector]
    public GameObject[,] gridChampionsArray;


    [HideInInspector]
    public int currentChampionLimit = 3;
    [HideInInspector]
    public int currentChampionCount = 0;

    public Dictionary<ChampionType, int> championTypeCount;
    public List<BaseBuffData> bonusBuffList;


    private GameObject draggedChampion = null;
    private TriggerInfo dragStartTrigger = null;

    private void Start()
    {
        championInventoryArray = new GameObject[Map.inventorySize];
        gridChampionsArray = new GameObject[Map.hexMapSizeX, Map.hexMapSizeZ / 2];
    }

    private void StoreChampionInArray(int gridType, int gridX, int gridZ, GameObject champion)
    {
        //assign current trigger to champion
        ChampionController championController = champion.GetComponent<ChampionController>();
        championController.SetGridPosition(gridType, gridX, gridZ);

        if (gridType == Map.GRIDTYPE_OWN_INVENTORY || gridType == Map.GRIDTYPE_OPONENT_INVENTORY)
        {
            championInventoryArray[gridX] = champion;
        }
        else if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            if (team == ChampionTeam.Player)
                gridChampionsArray[gridX, gridZ] = champion;
            else if (team == ChampionTeam.Oponent)
                gridChampionsArray[gridX, gridZ - 4] = champion;
        }
    }

    private void RemoveChampionFromArray(int type, int gridX, int gridZ)
    {
        if (type == Map.GRIDTYPE_OWN_INVENTORY || type == Map.GRIDTYPE_OPONENT_INVENTORY)
        {
            championInventoryArray[gridX] = null;
        }
        else if (type == Map.GRIDTYPE_HEXA_MAP)
        {
            if (team == ChampionTeam.Player)
                gridChampionsArray[gridX, gridZ] = null;
            else if (team == ChampionTeam.Oponent)
                gridChampionsArray[gridX, gridZ - 4] = null;
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
                if (gridChampionsArray[x, z] == null)
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
        foreach (GameObject championOBJ in gridChampionsArray)
        {
            if (championOBJ != null)
            {
                count++;
            }
        }
        return count;
    }
    private GameObject GetChampionFromTriggerInfo(TriggerInfo triggerinfo)
    {
        GameObject championGO = null;

        if (triggerinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY || triggerinfo.gridType == Map.GRIDTYPE_OPONENT_INVENTORY)
        {
            championGO = championInventoryArray[triggerinfo.gridX];
        }
        else if (triggerinfo.gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            if (team == ChampionTeam.Player && triggerinfo.gridZ < 4)
                championGO = gridChampionsArray[triggerinfo.gridX, triggerinfo.gridZ];
            else if (team == ChampionTeam.Oponent && triggerinfo.gridZ >= 4)
                championGO = gridChampionsArray[triggerinfo.gridX, triggerinfo.gridZ - 4];
        }

        return championGO;
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

        //setup chapioncontroller
        championController.Init(champion, team);

        //set grid position
        if (team == ChampionTeam.Player)
            championController.SetGridPosition(Map.GRIDTYPE_OWN_INVENTORY, emptyIndex, -1);
        else
            championController.SetGridPosition(Map.GRIDTYPE_OPONENT_INVENTORY, emptyIndex, -1);
        //set position and rotation
        championController.SetWorldPosition();
        championController.SetWorldRotation();


        //store champion in inventory array
        StoreChampionInArray(Map.GRIDTYPE_OWN_INVENTORY, Map.Instance.ownTriggerArray[emptyIndex].gridX, -1, championPrefab);




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

        //setup chapioncontroller
        championController.Init(champion, team);

        //set grid position
        championController.SetGridPosition(Map.GRIDTYPE_HEXA_MAP, indexX, indexZ);

        //set position and rotation
        championController.SetWorldPosition();
        championController.SetWorldRotation();

        StoreChampionInArray(Map.GRIDTYPE_HEXA_MAP, indexX, indexZ, championPrefab);

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
        foreach (GameObject championOBJ in championInventoryArray)
        {
            if (championOBJ != null)
            {
                ChampionController championController = championOBJ.GetComponent<ChampionController>();

                if (championController.champion == champion)
                {
                    if (championController.lvl == 1)
                        championList_lvl_1.Add(championController);
                    else if (championController.lvl == 2)
                        championList_lvl_2.Add(championController);
                }
            }
        }
        foreach (GameObject championOBJ in gridChampionsArray)
        {
            if (championOBJ != null)
            {
                ChampionController championController = championOBJ.GetComponent<ChampionController>();

                if (championController.champion == champion)
                {
                    if (championController.lvl == 1)
                        championList_lvl_1.Add(championController);
                    else if (championController.lvl == 2)
                        championList_lvl_2.Add(championController);
                }
            }
        }

        //if we have 3 we upgrade a champion and delete rest
        if (championList_lvl_1.Count > 2)
        {
            //upgrade
            championList_lvl_1[2].UpgradeLevel();

            //remove from array
            RemoveChampionFromArray(championList_lvl_1[0].gridType, championList_lvl_1[0].gridPositionX, championList_lvl_1[0].gridPositionZ);
            RemoveChampionFromArray(championList_lvl_1[1].gridType, championList_lvl_1[1].gridPositionX, championList_lvl_1[1].gridPositionZ);

            //destroy gameobjects
            Destroy(championList_lvl_1[0].gameObject);
            Destroy(championList_lvl_1[1].gameObject);

            //we upgrade to lvl 3
            if (championList_lvl_2.Count > 1)
            {
                //upgrade
                championList_lvl_1[2].UpgradeLevel();

                //remove from array
                RemoveChampionFromArray(championList_lvl_2[0].gridType, championList_lvl_2[0].gridPositionX, championList_lvl_2[0].gridPositionZ);
                RemoveChampionFromArray(championList_lvl_2[1].gridType, championList_lvl_2[1].gridPositionX, championList_lvl_2[1].gridPositionZ);

                //destroy gameobjects
                Destroy(championList_lvl_2[0].gameObject);
                Destroy(championList_lvl_2[1].gameObject);
            }
        }



        currentChampionCount = GetChampionCountOnHexGrid();

        //update ui
        UIController.Instance.UpdateUI();

    }

    public GameObject FindTarget(Vector3 centerPos, float bestDistance)
    {
        GameObject closestTarget = null;
        foreach (GameObject championOBJ in gridChampionsArray)
        {
            if (championOBJ != null)
            {
                ChampionController championController = championOBJ.GetComponent<ChampionController>();

                if (championController.isDead == false)
                {
                    float distance = Vector3.Distance(centerPos, championOBJ.transform.position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        closestTarget = championOBJ;
                    }
                }
            }
        }
        return closestTarget;
    }

    public void StartDrag()
    {
        //get trigger info
        TriggerInfo triggerinfo = InputController.Instance.triggerInfo;
        //if mouse cursor on trigger
        if (triggerinfo != null)
        {
            dragStartTrigger = triggerinfo;

            GameObject championGO = GetChampionFromTriggerInfo(triggerinfo);

            if (championGO != null)
            {
                //show indicators
                Map.Instance.ShowIndicators(team);

                draggedChampion = championGO;
                championGO.GetComponent<ChampionController>().IsDragged = true;
            }

        }
    }

    public void StopDrag()
    {
        //hide indicators
        Map.Instance.HideIndicators();

        int championsOnField = GetChampionCountOnHexGrid();


        if (draggedChampion != null)
        {
            //set dragged
            draggedChampion.GetComponent<ChampionController>().IsDragged = false;

            //get trigger info
            TriggerInfo triggerEndinfo = InputController.Instance.triggerInfo;

            //if mouse cursor on trigger
            if (triggerEndinfo != null)
            {
                //get current champion over mouse cursor
                GameObject currentTriggerEndChampion = GetChampionFromTriggerInfo(triggerEndinfo);

                //目标点有单位
                if (currentTriggerEndChampion != null)
                {
                    //交换位置
                    if ((triggerEndinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY && team == ChampionTeam.Player)
                        || (triggerEndinfo.gridType == Map.GRIDTYPE_OPONENT_INVENTORY && team == ChampionTeam.Oponent))
                    {
                        StoreChampionInArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ, currentTriggerEndChampion);
                        StoreChampionInArray(triggerEndinfo.gridType, triggerEndinfo.gridX, triggerEndinfo.gridZ, draggedChampion);
                    }
                }
                else//目标点无单位
                {
                    //目标点是战场
                    if (triggerEndinfo.gridType == Map.GRIDTYPE_HEXA_MAP)
                    {
                        if (championsOnField < currentChampionLimit || dragStartTrigger.gridType == Map.GRIDTYPE_HEXA_MAP)
                        {
                            RemoveChampionFromArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ);
                            StoreChampionInArray(triggerEndinfo.gridType, triggerEndinfo.gridX, triggerEndinfo.gridZ, draggedChampion);

                            if (dragStartTrigger.gridType != Map.GRIDTYPE_HEXA_MAP)
                                championsOnField++;
                        }
                    } //目标点是仓库
                    else if (triggerEndinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY)
                    {
                        RemoveChampionFromArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ);
                        StoreChampionInArray(triggerEndinfo.gridType, triggerEndinfo.gridX, triggerEndinfo.gridZ, draggedChampion);

                        if (dragStartTrigger.gridType == Map.GRIDTYPE_HEXA_MAP)
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

        foreach (GameObject championOBJ in gridChampionsArray)
        {
            //there is a champion
            if (championOBJ != null)
            {
                //get champion
                Champion c = championOBJ.GetComponent<ChampionController>().champion;

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
            foreach (GameObject championOBJ in gridChampionsArray)
            {
                if (championOBJ != null)
                {
                    ChampionController championController = championOBJ.GetComponent<ChampionController>();
                    championController.buffController.AddBuff(b, gameObject);

                }
            }
        }
    }

    private void ResetChampions()
    {
        foreach (GameObject championOBJ in gridChampionsArray)
        {
            //there is a champion
            if (championOBJ != null)
            {
                ChampionController championController = championOBJ.GetComponent<ChampionController>();
                championController.Reset();
            }
        }
    }

    public bool IsAllChampionDead()
    {
        int championCount = 0;
        int championDead = 0;
        //start own champion combat

        foreach (GameObject championOBJ in gridChampionsArray)
        {
            //there is a champion
            if (championOBJ != null)
            {
                ChampionController championController = championOBJ.GetComponent<ChampionController>();
                championCount++;

                if (championController.isDead)
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
                ChampionController championController = championInventoryArray[i].GetComponent<ChampionController>();

                Destroy(championController.gameObject);
                championInventoryArray[i] = null;
            }

        }
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();
                    Destroy(championController.gameObject);
                    gridChampionsArray[x, z] = null;
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
        ResetChampions();

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
        foreach (GameObject c in championInventoryArray)
        {
            if (c != null)
            {
                ChampionController championController = c.GetComponent<ChampionController>();
                championController.OnCombatStart();
            }
        }
        foreach (GameObject c in gridChampionsArray)
        {
            if (c != null)
            {
                ChampionController championController = c.GetComponent<ChampionController>();
                championController.OnCombatStart();
            }
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