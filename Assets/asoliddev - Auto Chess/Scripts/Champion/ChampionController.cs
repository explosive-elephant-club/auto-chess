using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using System;
using System.Reflection;
using UnityEngine.InputSystem;
using ExcelConfig;
using System.Linq;

public enum FindTargetMode { AnyInRange, Nearest, Farthest }

/// <summary>
/// Controls a single champion movement and combat
/// </summary>
public class ChampionController : MonoBehaviour, IGameStage
{
    /// <summary>
    /// 部件数组
    /// </summary>
    public List<ConstructorBase> constructors = new List<ConstructorBase>();
    /// <summary>
    /// 所处地块
    /// </summary>
    public MapContainer container;
    /// <summary>
    /// 战斗开始前的位置
    /// </summary>
    public Vector3 originPos;

    public ChampionTeam team = ChampionTeam.Player;
    public ChampionUnitType unitType = ChampionUnitType.Main;

    /// <summary>
    /// 单位的选中框和体积控制管理
    /// </summary>
    public ChampionVolumeController championVolumeController;
    /// <summary>
    /// 单位的属性管理
    /// </summary>
    public ChampionAttributesController attributesController;
    /// <summary>
    /// 单位的移动管理
    /// </summary>
    public ChampionMovementController championMovementController;
    /// <summary>
    /// 单位战斗计算管理
    /// </summary>
    public ChampionCombatController championCombatController;
    /// <summary>
    /// buff管理
    /// </summary>
    public BuffController buffController;
    /// <summary>
    /// 技能管理
    /// </summary>
    public SkillController skillController;

    /// <summary>
    /// 羁绊增益字典
    /// </summary>
    public Dictionary<ConstructorBonusType, int> bonus;
    /// <summary>
    /// 羁绊增益buff
    /// </summary>
    public List<int> bonusBuffList;


    private bool _isDragged = false;

    [HideInInspector]
    public bool isDead = false;
    /// <summary>
    /// 技能目标
    /// </summary>
    public ChampionController target;

    private List<Effect> effects;

    public ChampionManager championManeger;
    /// <summary>
    /// AI状态机
    /// </summary>
    public Fsm AIActionFsm;

    public float totalDamage = 0;

    /// Start is called before the first frame update
    void Awake()
    {
        championVolumeController = this.GetComponent<ChampionVolumeController>();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="_team">阵营</param>
    /// <param name="_championManeger">上级管理器</param>
    /// <param name="constructorData">底盘组件数据</param>
    /// <param name="_unitType">单位主次类型</param>
    public void Init(ChampionTeam _team, ChampionManager _championManeger, ConstructorBaseData constructorData = null, ChampionUnitType _unitType = ChampionUnitType.Main)
    {
        team = _team;
        if (team == ChampionTeam.Player)
        {
            gameObject.tag = "Own";
        }
        else
        {
            gameObject.tag = "Enemy";
        }

        championManeger = _championManeger;


        //初始化子管理器
        attributesController = new ChampionAttributesController(this);
        championMovementController = new ChampionMovementController(this);
        championCombatController = new ChampionCombatController(this);
        buffController = new BuffController(this);
        skillController = new SkillController(this);
        InitFsm();

        //分配部件
        if (constructorData == null)
            GetChassisConstructor().Init(this, true);
        else
            GetChassisConstructor().Init(constructorData, this, true);

        //修正最大值变化后的属性
        attributesController.RecalculateAfterMaxChange();
        //向GamePlayController注册阶段事件
        GamePlayController.Instance.AddStageListener(this);
        WorldCanvasController.Instance.AddHealthBar(this.gameObject);
        effects = new List<Effect>();
    }

    /// <summary>
    /// 初始化有限状态机
    /// </summary>
    void InitFsm()
    {
        AIActionFsm = new Fsm();
        //获取所有挂载的状态
        State[] states = transform.Find("States").GetComponents<State>();
        foreach (State s in states)
        {
            s.Init();
            AIActionFsm.states.Add(s._name, s);
        }
        //默认为Idle
        AIActionFsm.Init("Idle");
    }

    /// <summary>
    /// 被移除出发的事件
    /// </summary>
    public void OnRemove()
    {
        GamePlayController.Instance.RemoveStageListener(this);
    }

    /// Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Set dragged when moving champion with mouse
    /// </summary>
    public bool IsDragged
    {
        get { return _isDragged; }
        set { _isDragged = value; }
    }


    /// <summary>
    /// 复位单位状态
    /// </summary>
    public void Reset()
    {
        //set active
        this.gameObject.SetActive(true);

        //reset stats
        attributesController.Reset();
        isDead = false;
        target = null;

        championMovementController.path = null;

        //reset position
        if (originPos != null)
        {
            transform.position = originPos;
        }
        SetWorldPosition();
        SetWorldRotation();

        skillController.Reset();

        //移除所有buff
        buffController.RemoveAllBuff();

        //移除所有特效
        foreach (Effect e in effects)
        {
            e.Remove();
        }
        effects = new List<Effect>();
    }

    /// <summary>
    /// 获取单位的底盘部件
    /// </summary>
    public ConstructorBase GetChassisConstructor()
    {
        return constructors.Find(c => c.type == ConstructorType.Chassis || c.type == ConstructorType.Isolate);
    }

    /// <summary>
    /// 设置单位到战斗开始前的位置
    /// </summary>
    public void SetWorldPosition()
    {
        this.transform.position = originPos;
    }

    /// <summary>
    /// 根据阵营设置不同的朝向
    /// </summary>
    public void SetWorldRotation()
    {
        Vector3 rotation = Vector3.zero;

        if (team == ChampionTeam.Player)
        {
            rotation = new Vector3(0, 0, 0);
        }
        else if (team == ChampionTeam.Oponent)
        {
            rotation = new Vector3(0, 180, 0);
        }
        this.transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// 逐渐旋转到面向目标的方向
    /// </summary>
    /// <returns>是否正对目标</returns>
    public bool TurnToTarget()
    {
        //需要朝向目标的方向
        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0;
        Quaternion q = Quaternion.LookRotation(dir);

        ConstructorBase skillConstructor = skillController.GetNextSkillConstructor();
        float rotationSpeed = 8f * Time.deltaTime;

        //判断拥有下一个释放的技能部件是否有底座可以旋转
        if (skillConstructor.GetRotateTrans() != null)
        {
            //应用旋转到该组件
            Transform trans = skillConstructor.GetRotateTrans();
            trans.rotation = Quaternion.Slerp(trans.rotation, q, rotationSpeed);
            return Vector3.Angle(dir, trans.forward) > 2f;
        }
        else
        {
            //应用旋转到单位本体
            transform.rotation = Quaternion.Slerp(transform.rotation, q, rotationSpeed);
            return Vector3.Angle(dir, transform.forward) > 2f;
        }
    }

    /// <summary>
    /// 寻找下一个释放技能的目标
    /// </summary>
    /// <returns></returns>
    public ChampionController FindNextAvailableSkillTarget()
    {
        Skill skill = skillController.GetNextAvailableSkill();
        if (skill != null)
        {
            ChampionController c = skill.FindAvailableTarget();
            if (c != null)
            {
                return c;
            }
        }
        return null;
    }

    /// <summary>
    /// 寻找目标
    /// </summary>
    /// <param name="bestDistance">最大距离</param>
    /// <param name="mode">寻找模式</param>
    /// <returns></returns>
    public ChampionController FindTarget(int bestDistance, FindTargetMode mode)
    {
        ChampionManager manager;
        if (team == ChampionTeam.Player)
        {
            manager = GamePlayController.Instance.oponentChampionManager;
        }
        else
        {
            manager = GamePlayController.Instance.ownChampionManager;
        }

        switch (mode)
        {
            case FindTargetMode.AnyInRange:
                return manager.FindAnyTargetInRange(this, bestDistance);
            case FindTargetMode.Nearest:
                return manager.FindNearestTarget(this, bestDistance);
            case FindTargetMode.Farthest:
                return manager.FindFarthestTarget(this, bestDistance);
        }
        return null;
    }

    /// <summary>
    /// 获取下一个释放技能的施法距离
    /// </summary>
    /// <returns></returns>
    public int GetNextAvailableSkillDistance()
    {
        return (int)attributesController.addRange.GetTrueValue() + skillController.GetNextAvailableSkill().skillData.distance;
    }

    /// <summary>
    /// 是否有目标在打击范围里
    /// </summary>
    /// <returns></returns>
    public bool IsTargetInAttackRange()
    {
        if (target == null || target.isDead || skillController.GetNextAvailableSkill() == null)
            return false;
        return Vector3.Distance(transform.position, target.transform.position) <= GetNextAvailableSkillDistance();
    }

    public void DebugPrint(string str)
    {
        if (team == ChampionTeam.Player)
            Debug.Log(gameObject.name + ":  " + str);
    }

    public float GetDistance(ChampionController _target)
    {
        return Vector3.Distance(transform.position, _target.transform.position);
    }

    /// <summary>
    /// 获取周围的友军单位
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<ChampionController> GetTeammateNeighbors(float range = 3f)
    {
        List<ChampionController> list = new List<ChampionController>();
        foreach (var c in championManeger.championsBattleArray)
        {
            if (GetDistance(c) < range && c.team == team)
            {
                list.Add(c);
            }
        }
        return list;
    }

    /// <summary>
    /// 获取周围的敌军单位
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<ChampionController> GetEnemyNeighbors(float range = 3f)
    {
        List<ChampionController> list = new List<ChampionController>();
        foreach (var c in championManeger.championsBattleArray)
        {
            if (GetDistance(c) < range && c.team != team)
            {
                list.Add(c);
            }
        }
        return list;
    }

    public void Dead()
    {
        AIActionFsm.SwitchState("Idle");
        this.gameObject.SetActive(false);
        isDead = true;
        championManeger.OnChampionDeath(this);
    }

    /// <summary>
    /// 添加特效
    /// </summary>
    public void AddEffect(GameObject effectPrefab, float duration)
    {
        if (effectPrefab == null)
            return;

        //look for effect
        bool foundEffect = false;
        foreach (Effect e in effects)
        {
            if (effectPrefab == e.effectPrefab)
            {
                e.duration = duration;
                foundEffect = true;
            }
        }

        //not found effect
        if (foundEffect == false)
        {
            Effect effect = this.gameObject.AddComponent<Effect>();
            effect.Init(effectPrefab, this.gameObject, duration);
            effects.Add(effect);
        }

    }

    /// <summary>
    /// 移除特效
    /// </summary>
    public void RemoveEffect(Effect effect)
    {
        effects.Remove(effect);
        effect.Remove();
    }

    /// <summary>
    /// 检查异常状态
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    public bool CheckState(string stateName)
    {
        return buffController.buffStateContainer.GetState(stateName);
    }

    /// <summary>
    /// 计算羁绊增益
    /// </summary>
    public void CalculateBonuses()
    {
        //init dictionary
        bonus = new Dictionary<ConstructorBonusType, int>();

        List<ConstructorBonusType> types = new List<ConstructorBonusType>();
        foreach (ConstructorBase constructor in constructors)
        {
            types = GamePlayController.Instance.GetAllChampionTypes(constructor.constructorData);
            foreach (ConstructorBonusType t in types)
            {
                if (t != null)
                {
                    if (bonus.ContainsKey(t))
                    {
                        int cCount = 0;
                        bonus.TryGetValue(t, out cCount);
                        cCount++;
                        bonus[t] = cCount;

                    }
                    else
                    {
                        bonus.Add(t, 1);
                    }
                }
            }
        }

        bonusBuffList.Clear();
        foreach (KeyValuePair<ConstructorBonusType, int> m in bonus)
        {
            int buffID = 0;
            foreach (ConstructorBonusType.BonusClass b in m.Key.Bonus)
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

    /// <summary>
    /// 计算单位的价值
    /// </summary>
    /// <returns></returns>
    public int CalculateTotalCost()
    {
        int cost = 0;
        foreach (var c in constructors)
        {
            cost += c.cost;
        }
        return cost;
    }

    public void DestroySelf()
    {
        championManeger.RemoveChampionFromArray(this);
        championManeger.DestroyChampion(this);
    }

    #region StageFuncs
    public void OnEnterPreparation()
    {

    }
    public void OnUpdatePreparation()
    {
        if (_isDragged)
        {
            // 关闭当前单位的碰撞体，以避免拖拽时与其他单位或地图产生物理碰撞
            championVolumeController.col.enabled = false;

            //使单位平滑移动 到鼠标位置
            Vector3 mousePosition = InputController.Instance.mousePosition;
            Vector3 p = new Vector3(mousePosition.x, mousePosition.y, mousePosition.z);
            this.transform.position = Vector3.Lerp(this.transform.position, p, 0.2f);
        }
        else
        {
            //打开碰撞体，避免与其他单位发生重叠问题
            championVolumeController.col.enabled = true;
            float distance = Vector3.Distance(originPos, this.transform.position);
            if (distance > 0.25f)
            {
                this.transform.position = Vector3.Lerp(this.transform.position, originPos, 0.1f);
            }
            else
            {
                this.transform.position = originPos;
            }
        }
    }
    public void OnLeavePreparation()
    {
        totalDamage = 0;
    }

    public void OnEnterCombat()
    {
        IsDragged = false;
        this.transform.position = originPos;

        //检查单位是否在战斗区域
        if (container.containerType == ContainerType.Battle)
        {
            //打开移动
            championMovementController.MoveMode();
            //更新原点
            originPos = new Vector3(transform.position.x, container.col.bounds.min.y, transform.position.z);
        }

        //添加羁绊Buff
        foreach (int b in bonusBuffList)
        {
            buffController.AddBuff(b, this);
        }
        attributesController.RecalculateAfterMaxChange();
        buffController.eventCenter.Broadcast(BuffActiveMode.BeforeBattle.ToString());
        skillController.OnEnterCombat();
    }
    public void OnUpdateCombat()
    {
        if (!isDead)
        {
            //血量和能量恢复
            attributesController.Regenerate();
            championMovementController.UpdateSpeed();

            //子管理器驱动
            if (container.containerType == ContainerType.Battle)
                AIActionFsm.curState.OnUpdate();
            buffController.OnUpdateCombat();
            skillController.OnUpdateCombat();

            //DebugPrint("fireResistance layer:" + attributesController.fireResistance.curLayer);
            //DebugPrint("fireResistance Value:" + attributesController.fireResistance.curValue);
        }

    }
    public void OnLeaveCombat()
    {
        //关闭移动
        championMovementController.StaticMode();
        buffController.eventCenter.Broadcast(BuffActiveMode.AfterBattle.ToString());
        //Reset();
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
