using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using General;
using ExcelConfig;

/// <summary>
/// 所属的队伍 玩家/对手
/// </summary>
public enum ChampionTeam { Player, Oponent }
/// <summary>
/// 单位类型 部署的单位/被技能召唤出的单位
/// </summary>
public enum ChampionUnitType { Main, Support }
/// <summary>
/// 空间类型 地面/空中
/// </summary>
public enum ChampionSpaceType { Ground, Air }

/// <summary>
/// 管理战斗和仓库中的单位，包括单位的添加、移除、移动、战斗目标选择等。
/// 还涉及一些与游戏状态相关的功能，如检查单位是否死亡、处理拖拽操作、更新UI
/// </summary>
public class ChampionManager : MonoBehaviour, IGameStage
{
    /// <summary>
    /// 当前 ChampionManager 对象所属的队伍
    /// </summary>
    public ChampionTeam team;

    [HideInInspector]
    public MapContainer battleContainer;
    [HideInInspector]
    public MapContainer inventoryContainer;

    /// <summary>
    /// 仓库区域中的单位数组
    /// </summary>
    public List<ChampionController> championInventoryArray;
    /// <summary>
    /// 战场区域中的单位数组
    /// </summary>
    public List<ChampionController> championsBattleArray;


    /// <summary>
    /// 当前可以部署到战斗区域单位数量的最大值
    /// </summary>
    [HideInInspector]
    public int currentChampionLimit = 4;
    /// <summary>
    /// 当前已经部署到战斗区域的单位数量
    /// </summary>
    [HideInInspector]
    public int currentChampionCount = 0;

    /// <summary>
    /// 用于记录拖拽开始时的地块
    /// </summary>
    private MapContainer dragStartContainer = null;

    private void Awake()
    {

    }


    private void Start()
    {
        //初始化了仓库和战斗区域的地块
        championInventoryArray = new List<ChampionController>();
        championsBattleArray = new List<ChampionController>();
        //依据是 team 的不同，玩家或对手的单位数组初始化
        if (team == ChampionTeam.Player)
        {
            battleContainer = Map.Instance.ownBattleContainer;
            inventoryContainer = Map.Instance.ownInventoryContainer;
        }
        else
        {
            battleContainer = Map.Instance.oponentBattleContainer;
            inventoryContainer = Map.Instance.oponentInventoryContainer;
        }
        battleContainer.team = team;
        inventoryContainer.team = team;
    }

    /// <summary>
    /// 根据目标位置,将单位存储到仓库或战斗单位数组中，并为单位指定地块
    /// </summary>
    /// <param name="pos">目标位置</param>
    /// <param name="champion">单位</param>
    private void StoreChampionInArray(Vector3 pos, ChampionController champion)
    {

        if (inventoryContainer.IsContainsPos(pos))
        {
            championInventoryArray.Add(champion);
            champion.container = inventoryContainer;

        }
        else if (battleContainer.IsContainsPos(pos))
        {
            championsBattleArray.Add(champion);
            champion.container = battleContainer;

        }
        champion.originPos = pos;
    }

    /// <summary>
    /// 从仓库或战斗数组中移除单位
    /// </summary>
    /// <param name="champion">单位</param>
    public void RemoveChampionFromArray(ChampionController champion)
    {
        MapContainer container = champion.container;
        if (container.containerType == ContainerType.Inventory)
        {
            championInventoryArray.Remove(champion);
        }
        else if (container.containerType == ContainerType.Battle)
        {
            championsBattleArray.Remove(champion);
        }
    }

    /// <summary>
    /// 将单位添加到仓库数组
    /// </summary>
    /// <param name="constructorData">单位数据</param>
    /// <param name="pos">目标位置</param>
    public void AddChampionToInventory(ConstructorBaseData constructorData, Vector3 pos)
    {
        //初始化构造数据
        GameObject championPrefab = Instantiate(Resources.Load<GameObject>("Prefab/Champion/ChampionEmpty"));
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        GameObject constructorPrefab = Instantiate(Resources.Load<GameObject>("Prefab/Constructor/" + constructorData.prefab), championPrefab.transform);
        ConstructorBase constructorBase = constructorPrefab.GetComponent<ConstructorBase>();

        StoreChampionInArray(pos, championController);

        //初始化单位
        championController.constructors.Add(constructorBase);
        championController.Init(team, this, constructorData);

        //设置位置和旋转
        championController.SetWorldPosition();
        championController.SetWorldRotation();

        //并计算单位的加成
        championController.CalculateBonuses();

        //更新UI
        UIController.Instance.UpdateUI();
    }

    /// <summary>
    /// 根据敌人配置数据，生成单位，查找战斗区域的空位置并进行初始化。
    /// </summary>
    /// <param name="enemyConfig"></param>
    /// <returns></returns>
    public bool AddChampionToBattle(EnemyConfig enemyConfig)
    {
        //查找战斗区域的空位置
        Vector3 emptyPos = Vector3.zero;
        if (!battleContainer.GetEmptyPos(enemyConfig.pos, out emptyPos, 3))
            return false;

        StoreChampionInArray(emptyPos, enemyConfig.championController);
        //初始化单位
        enemyConfig.championController.Init(team, this);
        //添加技能
        foreach (var id in enemyConfig.skillIDs)
        {
            enemyConfig.championController.skillController.AddActivedSkill(id);
        }

        //设置位置和旋转
        enemyConfig.championController.SetWorldPosition();
        enemyConfig.championController.SetWorldRotation();

        //并计算单位的加成
        enemyConfig.championController.CalculateBonuses();
        //更新容量
        currentChampionCount = GetCurrentChampionCount();
        //更新UI
        UIController.Instance.UpdateUI();

        return true;
    }

    /// <summary>
    /// 将指定名称的单位添加到战斗单位数组，查找战斗区域的空位置并进行初始化。
    /// </summary>
    /// <param name="name">单位名称</param>
    /// <returns></returns>
    public bool AddChampionToBattle(string name)
    {
        //查找战斗区域的空位置
        Vector3 emptyPos = Vector3.zero;
        if (!battleContainer.GetEmptyPos(3, out emptyPos))
            return false;

        //初始化构造数据
        GameObject championPrefab = Instantiate(Resources.Load<GameObject>("Prefab/Champion/" + name));
        ChampionController championController = championPrefab.GetComponent<ChampionController>();
        StoreChampionInArray(emptyPos, championController);

        championPrefab.name = championPrefab.name + championsBattleArray.Count;
        //初始化单位
        championController.Init(team, this);

        //设置位置和旋转
        championController.SetWorldPosition();
        championController.SetWorldRotation();

        //并计算单位的加成
        championController.CalculateBonuses();
        //更新容量
        currentChampionCount = GetCurrentChampionCount();
        //更新UI
        UIController.Instance.UpdateUI();

        return true;
    }

    /// <summary>
    /// 将指定名称的召唤单位添加到战斗单位数组，放置在战斗区域的指定位置并进行初始化。
    /// </summary>
    /// <param name="pos">指定位置</param>
    /// <param name="name">单位名称</param>
    /// <param name="skillIDs">技能组</param>
    /// <returns></returns>
    public bool AddSupportChampionToBattle(Vector3 pos, string name, int[] skillIDs)
    {
        //初始化构造数据
        GameObject championPrefab = Instantiate(Resources.Load<GameObject>("Prefab/Champion/Support/" + name));
        ChampionController championController = championPrefab.GetComponent<ChampionController>();
        StoreChampionInArray(pos, championController);
        championPrefab.name = championPrefab.name + championsBattleArray.Count;

        //初始化单位
        championController.Init(team, this, null, ChampionUnitType.Support);
        //添加技能
        foreach (var id in skillIDs)
        {
            championController.skillController.AddActivedSkill(id);
        }

        //设置位置和旋转
        championController.SetWorldPosition();
        championController.SetWorldRotation();

        //并计算单位的加成
        championController.CalculateBonuses();

        //启动寻路
        championController.championMovementController.MoveMode();
        //更新UI
        UIController.Instance.UpdateUI();

        return true;
    }

    /// <summary>
    /// 获取当前战斗区域单位数量
    /// </summary>
    /// <returns></returns>
    public int GetCurrentChampionCount()
    {
        int i = 0;
        foreach (var c in championsBattleArray)
        {
            if (c.unitType == ChampionUnitType.Main)
            {
                i++;
            }
        }
        return i;
    }


    /// <summary>
    /// 获取目标单位周围的任意目标
    /// </summary>
    /// <param name="_championController">目标单位</param>
    /// <param name="bestDistance">最大距离</param>
    /// <returns></returns>
    public ChampionController FindAnyTargetInRange(ChampionController _championController, int bestDistance)
    {
        foreach (ChampionController championCtrl in championsBattleArray)
        {
            if (championCtrl != null)
            {

                if (championCtrl.isDead == false)
                {

                    float distance = Vector3.Distance(_championController.transform.position, championCtrl.transform.position);
                    if (distance <= bestDistance)
                    {
                        return championCtrl;

                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 获取目标单位周围的最近目标
    /// </summary>
    /// <param name="_championController">目标单位</param>
    /// <param name="bestDistance">最大距离</param>
    /// <returns></returns>
    public ChampionController FindNearestTarget(ChampionController _championController, int bestDistance)
    {
        ChampionController closestTarget = null;
        float tempDis = 30;
        foreach (ChampionController championCtrl in championsBattleArray)
        {

            if (championCtrl != null)
            {
                ChampionController targetChampion = championCtrl;

                if (targetChampion.isDead == false)
                {
                    float distance = Vector3.Distance(_championController.transform.position, targetChampion.transform.position);
                    if (distance <= bestDistance && distance < tempDis && distance > 0)
                    {
                        tempDis = distance;
                        closestTarget = targetChampion;
                    }
                }
            }
        }
        return closestTarget;
    }

    /// <summary>
    /// 获取目标单位周围的最远目标
    /// </summary>
    /// <param name="_championController">目标单位</param>
    /// <param name="bestDistance">最大距离</param>
    /// <returns></returns>
    public ChampionController FindFarthestTarget(ChampionController _championController, int bestDistance)
    {
        ChampionController farthestTarget = null;
        float tempDis = 0;
        foreach (ChampionController championCtrl in championsBattleArray)
        {
            if (championCtrl != null)
            {
                ChampionController targetChampion = championCtrl;

                if (targetChampion.isDead == false)
                {

                    float distance = Vector3.Distance(_championController.transform.position, targetChampion.transform.position);
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

    /// <summary>
    /// 处理拖拽操作的开始
    /// </summary>
    public void StartDrag()
    {
        //确保玩家选择了一个单位 并且检查是否有 UI 操作正在进行，确保拖拽不受 UI 操作的影响
        if (GamePlayController.Instance.pickedChampion != null && InputController.Instance.ui == null)
        {
            //记录当前拖拽操作开始时单位所在的地块
            dragStartContainer = InputController.Instance.mapContainer;
            //设置当前选中的单位为正在被拖拽
            GamePlayController.Instance.pickedChampion.IsDragged = true;
        }
    }

    /// <summary>
    /// 处理拖拽操作的结束
    /// </summary>
    public void StopDrag()
    {
        //hide indicators
        //Map.Instance.HideIndicators();
        //确保玩家选择了一个单位 并且没有正在进行UI拖拽操作
        if (GamePlayController.Instance.pickedChampion != null && !UIController.Instance.isSlotUIDragged)
        {


            //获取当前鼠标指示的单位
            ChampionController championCtrl = InputController.Instance.champion;
            //获取当前鼠标所在的地块
            MapContainer mapContainer = InputController.Instance.mapContainer;


            if (mapContainer != null)
            {
                //判断地块是否为当前玩家的
                if (mapContainer.team == GamePlayController.Instance.pickedChampion.team)
                {
                    //是否在mapContainer范围里
                    if (mapContainer.CheckChampionInBounds(GamePlayController.Instance.pickedChampion))
                    {
                        Debug.Log("在mapContainer范围里");
                        if (championCtrl != null || InputController.Instance.CheckChampionInRange(GamePlayController.Instance.pickedChampion)) //目标点有单位
                        {
                            Debug.Log("目标点有单位");
                            //交换位置
                        }
                        else//目标点无单位
                        {
                            Debug.Log("目标点无单位");
                            //起始点是仓库 目标点是战场
                            if (mapContainer.containerType == ContainerType.Battle && dragStartContainer.containerType == ContainerType.Inventory)
                            {
                                if (GetCurrentChampionCount() < currentChampionLimit)
                                {
                                    RemoveChampionFromArray(GamePlayController.Instance.pickedChampion);

                                    StoreChampionInArray(InputController.Instance.mousePosition, GamePlayController.Instance.pickedChampion);
                                }
                            } //目标点是仓库
                            else //if (mapContainer.containerType == ContainerType.Inventory && dragStartContainer.containerType == ContainerType.Battle)
                            {
                                RemoveChampionFromArray(GamePlayController.Instance.pickedChampion);
                                StoreChampionInArray(InputController.Instance.mousePosition, GamePlayController.Instance.pickedChampion);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("不在mapContainer范围里");
                    }
                }
                else
                {
                    Debug.Log("在对方mapContainer范围里");

                }


            }


            //set dragged
            GamePlayController.Instance.pickedChampion.IsDragged = false;
            currentChampionCount = GetCurrentChampionCount();

            //update ui
            UIController.Instance.UpdateUI();
            //draggedChampion = null;

        }
    }

    /// <summary>
    /// 判断战斗区域的单位是否全部阵亡
    /// </summary>
    /// <returns></returns>
    public bool IsAllChampionDead()
    {
        int championDead = 0;
        //start own champion combat

        foreach (ChampionController championCtrl in championsBattleArray)
        {
            //there is a champion
            if (championCtrl != null)
            {
                if (championCtrl.isDead)
                    championDead++;

            }
        }

        if (championDead == GetCurrentChampionCount())
            return true;

        return false;
    }

    /// <summary>
    /// 单位数组和游戏状态
    /// </summary>
    public virtual void Reset()
    {
        foreach (var champion in championInventoryArray)
        {
            DestroyChampion(champion);
        }
        championInventoryArray.Clear();
        foreach (var champion in championsBattleArray)
        {
            DestroyChampion(champion);
        }
        championsBattleArray.Clear();
        currentChampionLimit = 4;
        currentChampionCount = 0;
    }

    /// <summary>
    /// 处理单位死亡的事件
    /// </summary>
    /// <param name="championController">单位对象</param>
    public virtual void OnChampionDeath(ChampionController championController)
    {
        //检查死亡单位是否是当前选中的单位
        if (championController == GamePlayController.Instance.pickedChampion)
        {
            GamePlayController.Instance.SetPickedChampion(null);
            UIController.Instance.championInfoController.UpdateUI();
            UIController.Instance.constructorAssembleController.UpdateUI();
        }
        //如果所有单位都死亡，则结束回合
        if (IsAllChampionDead())
        {
            GamePlayController.Instance.EndRound();
        }

    }


    /// <summary>
    /// 销毁指定的单位对象
    /// </summary>
    /// <param name="championController">单位对象</param>
    public void DestroyChampion(ChampionController championController)
    {
        if (championController == GamePlayController.Instance.pickedChampion)
        {
            GamePlayController.Instance.SetPickedChampion(null);
            UIController.Instance.championInfoController.UpdateUI();
            UIController.Instance.constructorAssembleController.UpdateUI();
        }
        if (GamePlayController.Instance._GOToUICameraController.cameraTarget == championController.transform)
        {
            GamePlayController.Instance._GOToUICameraController.ResetCam();
        }
        championController.OnRemove();
        Destroy(championController.gameObject);
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
        if (GamePlayController.Instance.pickedChampion != null)
        {
            //取消选中单位的拖拽状态
            GamePlayController.Instance.pickedChampion.GetComponent<ChampionController>().IsDragged = false;
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