using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using System;
using System.Reflection;
using UnityEngine.InputSystem;
using ExcelConfig;

public enum FindTargetMode { AnyInRange, Nearest, Farthest }

/// <summary>
/// Controls a single champion movement and combat
/// </summary>
public class ChampionController : MonoBehaviour
{
    public List<ConstructorBase> constructors = new List<ConstructorBase>();

    public GridInfo occupyGridInfo;
    public GridInfo bookGridInfo;
    public GridInfo originGridInfo;

    public ChampionTeam team = ChampionTeam.Player;
    public ChampionUnitType unitType = ChampionUnitType.Main;

    public ChampionAttributesController attributesController;
    public BuffController buffController;
    public SkillController skillController;
    public NavMeshAgent navMeshAgent;


    public Dictionary<ConstructorBonusType, int> constructorTypeCount;
    public List<int> bonusBuffList;


    private bool _isDragged = false;

    [HideInInspector]
    public bool isDead = false;

    public ChampionController target;

    private List<Effect> effects;

    public ChampionManager championManeger;

    public Fsm AIActionFsm;

    public List<GridInfo> path;

    public string state;

    public Dictionary<string, CallBack> gameStageActions = new Dictionary<string, CallBack>();

    public float totalDamage = 0;
    public Vector3 speed;

    /// Start is called before the first frame update
    void Awake()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        buffController = this.GetComponent<BuffController>();
        skillController = this.GetComponent<SkillController>();
        InitStageDic();
    }

    /// <summary>
    /// When champion created Champion and teamID passed
    /// </summary>
    /// <param name="_champion"></param>
    /// <param name="_teamID"></param>
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
        //store scripts

        championManeger = _championManeger;

        //disable agent
        navMeshAgent.enabled = false;

        //set stats
        attributesController = new ChampionAttributesController();
        attributesController.fireResistance.callAction = () => { buffController.AddBuff(301); };
        attributesController.iceResistance.callAction = () => { buffController.AddBuff(302); };
        attributesController.lightningResistance.callAction = () => { buffController.AddBuff(303); };
        attributesController.acidResistance.callAction = () => { buffController.AddBuff(304); };

        WorldCanvasController.Instance.AddHealthBar(this.gameObject);

        effects = new List<Effect>();

        AIActionFsm = new Fsm();
        InitFsm();
        if (constructorData == null)
            GetChassisConstructor().Init(this, true);
        else
            GetChassisConstructor().Init(constructorData, this, true);

        attributesController.RecalculateAfterMaxChange();
        GamePlayController.Instance.StageStateAddListener(gameStageActions);
    }

    void InitFsm()
    {
        State[] states = transform.Find("States").GetComponents<State>();
        foreach (State s in states)
        {
            s.Init();
            AIActionFsm.states.Add(s._name, s);
        }
        AIActionFsm.Init("Idle");
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

    public void OnRemove()
    {
        GamePlayController.Instance.StageStateRemoveListener(gameStageActions);
    }

    /// Update is called once per frame
    void Update()
    {
        state = AIActionFsm.curState._name;
        speed = navMeshAgent.velocity;
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
    /// Resets champion after combat is over
    /// </summary>
    public void Reset()
    {
        //set active
        this.gameObject.SetActive(true);

        //reset stats
        attributesController.Reset();
        isDead = false;
        target = null;

        path = null;

        //reset position
        if (originGridInfo != null)
        {
            LeaveGrid();
            EnterGrid(originGridInfo);
        }
        SetWorldPosition();
        SetWorldRotation();

        skillController.Reset();

        //remove add buffs
        buffController.RemoveAllBuff();

        //remove all effects
        foreach (Effect e in effects)
        {
            e.Remove();
        }
        effects = new List<Effect>();
    }

    public ConstructorBase GetChassisConstructor()
    {
        foreach (var c in constructors)
        {
            Debug.Log(c.constructorDataID + ":" + c.type);
        }
        return constructors.Find(c => c.type == ConstructorType.Chassis || c.type == ConstructorType.Isolate);
    }

    /// <summary>
    /// Move to corrent world position
    /// </summary>
    public void SetWorldPosition()
    {
        Vector3 worldPosition = occupyGridInfo.gameObject.transform.position;
        worldPosition.y = this.transform.position.y;
        this.transform.position = worldPosition;
    }

    /// <summary>
    /// Set correct rotation
    /// </summary>
    public void SetWorldRotation()
    {
        Vector3 rotation = Vector3.zero;

        if (team == ChampionTeam.Player)
        {
            rotation = new Vector3(0, 205, 0);
        }
        else if (team == ChampionTeam.Oponent)
        {
            rotation = new Vector3(0, 25, 0);
        }
        this.transform.rotation = Quaternion.Euler(rotation);
    }

    public bool TurnToTarget()
    {
        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0;
        Quaternion q = Quaternion.LookRotation(dir);
        if (skillController.GetNextSkillConstructor().GetRotateTrans() != null)
        {
            Transform trans = skillController.GetNextSkillConstructor().GetRotateTrans();
            trans.rotation = Quaternion.Slerp(trans.rotation, q, 8 * Time.deltaTime);
            return Vector3.Angle(dir, trans.forward) > 2f;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, q, 8 * Time.deltaTime);
            return Vector3.Angle(dir, transform.forward) > 2f;
        }
    }

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

    public bool FindPath()
    {
        if (target != null)
        {
            path = Map.Instance.FindPath(occupyGridInfo, target.occupyGridInfo, this);
            if (path == null)
                return false;
            //Debug.Log("FindPath:" + occupyGridInfo.name + "=>" + target.occupyGridInfo.name);
            BookGrid(path[0]);
            return true;
        }
        return false;
    }


    public void EnterGrid(GridInfo grid)
    {
        //Debug.Log("EnterGrid:" + grid.name);
        ClearBook();
        LeaveGrid();
        occupyGridInfo = grid;
        grid.occupyChampion = this;
    }

    public void LeaveGrid()
    {
        if (occupyGridInfo != null)
        {
            occupyGridInfo.occupyChampion = null;
            occupyGridInfo = null;
        }

    }

    public void BookGrid(GridInfo grid)
    {
        ClearBook();
        bookGridInfo = grid;
        grid.bookChampion = this;
    }

    public void ClearBook()
    {
        if (bookGridInfo != null)
        {
            bookGridInfo.bookChampion = null;
            bookGridInfo = null;
        }
    }

    public int GetInAttackRange()
    {
        return (int)attributesController.addRange.GetTrueValue() + skillController.GetNextSkillRange();
    }

    public bool IsTargetInAttackRange()
    {
        if (target == null || target.isDead)
            return false;
        return occupyGridInfo.GetDistance(target.occupyGridInfo) <= GetInAttackRange();
    }

    public void DebugPrint(string str)
    {
        if (team == ChampionTeam.Player)
            Debug.Log(gameObject.name + ":  " + str);
    }

    public void NavMeshAgentMove()
    {
        if (navMeshAgent.enabled)
        {
            if (navMeshAgent.speed != attributesController.moveSpeed.GetTrueValue())
            {
                navMeshAgent.speed = attributesController.moveSpeed.GetTrueValue();
            }
            navMeshAgent.destination = bookGridInfo.transform.position;
            navMeshAgent.isStopped = false;
        }
    }

    public void StopMove()
    {
        if (navMeshAgent.enabled)
            navMeshAgent.isStopped = true;
    }

    public int GetDistance(ChampionController _target)
    {
        return occupyGridInfo.GetDistance(_target.occupyGridInfo);
    }

    public void TakeDamage(ChampionController _target, SkillData.damageDataClass[] damages, float fix = 1)
    {
        if (target == null)
            return;
        buffController.eventCenter.Broadcast(BuffActiveMode.BeforeAttack.ToString());

        if (_target.skillController.curVoidShieldEffect != null)
        {
            _target.skillController.curVoidShieldEffect.OnGotHit(this, damages);
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());
            return;
        }

        float crit = 1;
        if (attributesController.CritCheck())
        {
            Debug.Log("暴击");
            crit = attributesController.critMultiple.GetTrueValue();
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterCrit.ToString());
        }
        List<SkillData.damageDataClass> addDamages = new List<SkillData.damageDataClass>();
        for (int i = 0; i < damages.Length; i++)
        {
            float trueDamage = attributesController.GetTrueDamage(damages[i].dmg,
                (DamageType)Enum.Parse(typeof(DamageType), damages[i].type), damages[i].correction);
            trueDamage *= fix;
            trueDamage *= crit;

            totalDamage += trueDamage;
            addDamages.Add(new SkillData.damageDataClass() { dmg = (int)trueDamage, type = damages[i].type });
        }
        _target.OnGotHit(addDamages);
        buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());
    }

    public void TakeDamage(ChampionController _target, float dmg, DamageType damageType, float correction = 0, float fix = 1)
    {
        if (target == null)
            return;
        buffController.eventCenter.Broadcast(BuffActiveMode.BeforeAttack.ToString());
        float crit = 1;
        if (attributesController.CritCheck())
        {
            Debug.Log("暴击");
            crit = attributesController.critMultiple.GetTrueValue();
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterCrit.ToString());
        }
        float trueDamage = attributesController.GetTrueDamage(dmg, damageType, correction);
        trueDamage *= fix;
        trueDamage *= crit;
        OnGotHit(trueDamage, damageType);

        totalDamage += trueDamage;

        buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());
    }

    public bool OnGotHit(List<SkillData.damageDataClass> addDamages)
    {
        if (attributesController.HitCheck())
        {
            buffController.eventCenter.Broadcast(BuffActiveMode.BeforeHit.ToString());

            foreach (var d in addDamages)
            {
                float trueDMG = attributesController.ApplyDamage(d.dmg, (DamageType)Enum.Parse(typeof(DamageType), d.type));

                //add floating text
                WorldCanvasController.Instance.AddDamageText(this.transform.position + new Vector3(0, 2.5f, 0), trueDMG);
                //death
                if (attributesController.curHealth <= 0)
                {
                    Dead();
                }
            }

            buffController.eventCenter.Broadcast(BuffActiveMode.AfterHit.ToString());
        }
        else
        {
            Debug.Log("闪避");
            WorldCanvasController.Instance.AddDamageText(this.transform.position + new Vector3(0, 2.5f, 0), 0);
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterDodge.ToString());
        }
        return isDead;
    }

    public bool OnGotHit(float dmg, DamageType damageType)
    {
        if (attributesController.HitCheck())
        {
            buffController.eventCenter.Broadcast(BuffActiveMode.BeforeHit.ToString());

            float trueDMG = attributesController.ApplyDamage(dmg, damageType);
            //add floating text
            WorldCanvasController.Instance.AddDamageText(this.transform.position + new Vector3(0, 2.5f, 0), trueDMG);
            switch (damageType)
            {
                case DamageType.Fire:
                    attributesController.fireResistance.OnGetHit(trueDMG);
                    break;
                case DamageType.Ice:
                    attributesController.iceResistance.OnGetHit(trueDMG);
                    break;
                case DamageType.Lightning:
                    attributesController.lightningResistance.OnGetHit(trueDMG);
                    break;
                case DamageType.Acid:
                    attributesController.acidResistance.OnGetHit(trueDMG);
                    break;
                default:
                    break;
            }
            //death
            if (attributesController.curHealth <= 0)
            {
                Dead();
            }
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterHit.ToString());
        }
        else
        {
            Debug.Log("闪避");
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterDodge.ToString());
        }
        return isDead;
    }

    public void Dead()
    {
        AIActionFsm.SwitchState("Idle");
        this.gameObject.SetActive(false);
        isDead = true;
        LeaveGrid();
        ClearBook();
        championManeger.OnChampionDeath(this);
    }

    /// <summary>
    /// Add effect to this champion
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
    /// Remove effect when expired
    /// </summary>
    public void RemoveEffect(Effect effect)
    {
        effects.Remove(effect);
        effect.Remove();
    }

    public bool CheckState(string stateName)
    {
        return buffController.buffStateContainer.GetState(stateName);
    }

    public void CalculateBonuses()
    {
        //init dictionary
        constructorTypeCount = new Dictionary<ConstructorBonusType, int>();

        List<ConstructorBonusType> types = new List<ConstructorBonusType>();
        foreach (ConstructorBase constructor in constructors)
        {
            types = GamePlayController.Instance.GetAllChampionTypes(constructor.constructorData);
            foreach (ConstructorBonusType t in types)
            {
                if (t != null)
                {
                    if (constructorTypeCount.ContainsKey(t))
                    {
                        int cCount = 0;
                        constructorTypeCount.TryGetValue(t, out cCount);
                        cCount++;
                        constructorTypeCount[t] = cCount;

                    }
                    else
                    {
                        constructorTypeCount.Add(t, 1);
                    }
                }
            }
        }

        bonusBuffList.Clear();
        foreach (KeyValuePair<ConstructorBonusType, int> m in constructorTypeCount)
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
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            float enter = 100.0f;
            if (Map.Instance.m_Plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 p = new Vector3(hitPoint.x, 1.0f, hitPoint.z);
                this.transform.position = Vector3.Lerp(this.transform.position, p, 0.2f);
            }
        }
        else
        {
            float distance = Vector3.Distance(occupyGridInfo.gameObject.transform.position, this.transform.position);
            if (distance > 0.25f)
            {
                this.transform.position = Vector3.Lerp(this.transform.position, occupyGridInfo.gameObject.transform.position, 0.1f);
            }
            else
            {
                this.transform.position = occupyGridInfo.gameObject.transform.position;
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
        this.transform.position = occupyGridInfo.gameObject.transform.position;

        //in combat grid
        if (occupyGridInfo.gridType == GridType.HexaMap)
        {
            navMeshAgent.enabled = true;
            originGridInfo = occupyGridInfo;
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
        if (!isDead && occupyGridInfo != null)
        {
            attributesController.Regenerate();
            navMeshAgent.speed = attributesController.moveSpeed.GetTrueValue();

            if (occupyGridInfo.gridType == GridType.HexaMap)
                AIActionFsm.curState.OnUpdate();
            buffController.OnUpdateCombat();
            skillController.OnUpdateCombat();

            //DebugPrint("fireResistance layer:" + attributesController.fireResistance.curLayer);
            //DebugPrint("fireResistance Value:" + attributesController.fireResistance.curValue);
        }

    }
    public void OnLeaveCombat()
    {
        navMeshAgent.enabled = false;
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
