using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using System;
using System.Reflection;
using General;
using ExcelConfig;

public enum FindTargetMode { AnyInRange, Closet, Farthest }

/// <summary>
/// Controls a single champion movement and combat
/// </summary>
public class ChampionController : MonoBehaviour
{
    public GameObject levelupEffectPrefab;
    public GameObject projectileStart;

    public GridInfo occupyGridInfo;
    public GridInfo bookGridInfo;
    public GridInfo originGridInfo;

    public ChampionTeam team = ChampionTeam.Player;


    [HideInInspector]
    public ChampionBaseData champion;

    public ChampionAttributesController attributesController;

    [HideInInspector]
    ///The upgrade level of the champion
    public int lvl = 1;

    public ChampionAnimation championAnimation;

    public BuffController buffController;

    private NavMeshAgent navMeshAgent;

    private bool _isDragged = false;

    [HideInInspector]
    public bool isDead = false;

    public ChampionController target;

    private List<Effect> effects;

    public ChampionManager championManeger;

    public Fsm AIActionFsm;

    public List<GridInfo> path;

    public string state;

    public EventCenter eventCenter;

    public Dictionary<string, CallBack> gameStageActions = new Dictionary<string, CallBack>();

    public float attackIntervelTimer = 0;

    public float ATK = 0;

    /// Start is called before the first frame update
    void Awake()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        championAnimation = this.GetComponent<ChampionAnimation>();
        buffController = this.GetComponent<BuffController>();
        eventCenter = new EventCenter();
        InitStageDic();
    }

    /// <summary>
    /// When champion created Champion and teamID passed
    /// </summary>
    /// <param name="_champion"></param>
    /// <param name="_teamID"></param>
    public void Init(ChampionBaseData _champion, ChampionTeam _team, ChampionManager _championManeger)
    {
        champion = _champion;
        team = _team;

        //store scripts

        championManeger = _championManeger;

        //disable agent
        navMeshAgent.enabled = false;

        //set stats
        attributesController = new ChampionAttributesController(_champion);

        WorldCanvasController.Instance.AddHealthBar(this.gameObject);

        effects = new List<Effect>();

        GamePlayController.Instance.StageStateAddListener(gameStageActions);
        AIActionFsm = new Fsm();
        InitFsm();
    }

    void InitFsm()
    {
        State[] states = transform.Find("States").GetComponents<State>();
        foreach (State s in states)
        {
            s.Init();
            AIActionFsm.states.Add(s.name, s);
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
        state = AIActionFsm.curState.name;
        if (_isDragged != championAnimation.animator.GetBool("isDragged"))
            championAnimation.animator.SetBool("isDragged", _isDragged);
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

        //remove add buffs
        buffController.RemoveAllBuff();

        //remove all effects
        foreach (Effect e in effects)
        {
            e.Remove();
        }

        effects = new List<Effect>();
    }

    /// <summary>
    /// Move to corrent world position
    /// </summary>
    public void SetWorldPosition()
    {
        Vector3 worldPosition = occupyGridInfo.gameObject.transform.position;
        worldPosition.z = this.transform.position.z;
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
            rotation = new Vector3(0, 200, 0);
        }
        else if (team == ChampionTeam.Oponent)
        {
            rotation = new Vector3(0, 20, 0);
        }
        this.transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Upgrade champion lvl
    /// </summary>
    public void UpgradeLevel()
    {
        //incrase lvl
        lvl++;

        float newSize = 1;
        if (lvl == 2)
            newSize = 1.5f;
        if (lvl == 3)
            newSize = 2f;
        //set size
        this.transform.localScale = new Vector3(newSize, newSize, newSize);

        attributesController.UpdateLevelAttributes(champion, lvl);

        //instantiate level up effect
        GameObject levelupEffect = Instantiate(levelupEffectPrefab);

        //set position
        levelupEffect.transform.position = this.transform.position;

        //destroy effect after finished
        Destroy(levelupEffect, 1.0f);
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
            case FindTargetMode.Closet:
                return manager.FindClosestTarget(this, bestDistance);
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
        eventCenter.Broadcast("OnEnterGrid", grid);
    }

    public void LeaveGrid()
    {
        if (occupyGridInfo != null)
        {
            eventCenter.Broadcast("OnLeaveGrid", occupyGridInfo);
            occupyGridInfo.occupyChampion = null;
            occupyGridInfo = null;
        }

    }

    public void BookGrid(GridInfo grid)
    {
        ClearBook();
        bookGridInfo = grid;
        grid.bookChampion = this;
        eventCenter.Broadcast("OnBookGrid", grid);
    }

    public void ClearBook()
    {
        if (bookGridInfo != null)
        {
            bookGridInfo.bookChampion = null;
            bookGridInfo = null;
        }
    }

    public bool IsTargetInAttackRange()
    {
        return occupyGridInfo.GetDistance(target.occupyGridInfo) <=
            (int)attributesController.attackRange.GetTrueLinearValue();
    }

    public bool IsLegalAttackIntervel()
    {
        return attackIntervelTimer >= attributesController.GetAttackIntervel();
    }

    void DebugPrint(string str)
    {
        if (team == ChampionTeam.Player)
            Debug.Log(gameObject.name + ":  " + str);
    }

    public void MoveToTarget()
    {
        if (bookGridInfo.CheckInGrid(this))
        {
            //Debug.Log("InGrid:" + bookGridInfo.name);
            EnterGrid(bookGridInfo);

            if (path == null)
                return;
            if (bookGridInfo == target.occupyGridInfo)
            {
                StopMove();
                SetWorldPosition();
                eventCenter.Broadcast("OnGetTarget", bookGridInfo);
            }
        }
        else
        {
            if (navMeshAgent.enabled)
            {
                if (navMeshAgent.speed != attributesController.moveSpeed.GetTrueLinearValue())
                {
                    navMeshAgent.speed = attributesController.moveSpeed.GetTrueLinearValue();
                }
                navMeshAgent.destination = bookGridInfo.transform.position;
                navMeshAgent.isStopped = false;
            }
        }
    }

    public void StopMove()
    {
        if (navMeshAgent.enabled)
            navMeshAgent.isStopped = true;
    }


    /// <summary>
    /// Called when attack animation finished
    /// </summary>
    public void OnAttackAnimationFinished()
    {
        if (target != null)
        {
            buffController.eventCenter.Broadcast(BuffActiveMode.BeforeAttack.ToString());
            target.OnGotHit(attributesController.GetAttackDamage(), (DamageType)Enum.Parse(typeof(DamageType), champion.attackType));

            //create projectile if have one
            if (!string.IsNullOrEmpty(champion.attackProjectile))
            {
                GameObject projectile = Instantiate(Resources.Load<GameObject>(champion.attackProjectile));
                projectile.transform.position = projectileStart.transform.position;

                projectile.GetComponent<Projectile>().Init(target.gameObject);
            }

            buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());
        }
    }

    /// <summary>
    /// Called when this champion takes damage
    /// </summary>
    /// <param name="damage"></param>
    public bool OnGotHit(float damage, DamageType dmgType)
    {
        if (attributesController.DodgeCheck())
        {
            Debug.Log("闪避");
        }
        else
        {
            buffController.eventCenter.Broadcast(BuffActiveMode.BeforeHit.ToString());
            float trueDMG = attributesController.ApplyDamage(damage, dmgType);
            //add floating text
            WorldCanvasController.Instance.AddDamageText(this.transform.position + new Vector3(0, 2.5f, 0), trueDMG);
            //death
            if (attributesController.curHealth <= 0)
            {
                this.gameObject.SetActive(false);
                isDead = true;
                LeaveGrid();
                ClearBook();
                championManeger.OnChampionDeath();
            }
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterHit.ToString());
        }
        return isDead;
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

    #region StageFuncs
    public void OnEnterPreparation()
    {
        return;
    }
    public void OnUpdatePreparation()
    {
        if (_isDragged)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float enter = 100.0f;
            if (Map.Instance.m_Plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 p = new Vector3(hitPoint.x, 1.0f, hitPoint.z);
                this.transform.position = Vector3.Lerp(this.transform.position, p, 0.1f);
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

    }

    public void OnEnterCombat()
    {
        IsDragged = false;
        this.transform.position = occupyGridInfo.gameObject.transform.position;
        attackIntervelTimer = attributesController.GetAttackIntervel();
        //in combat grid
        if (occupyGridInfo.gridType == GridType.HexaMap)
        {
            navMeshAgent.enabled = true;
            championAnimation.InitBehavour();
            championAnimation.animator.SetBool("isInCambat", true);
            originGridInfo = occupyGridInfo;
        }

        //添加羁绊Buff
        List<int> activeBonuses = championManeger.bonusBuffList;

        foreach (int b in activeBonuses)
        {
            buffController.AddBuff(b, gameObject);
        }
    }
    public void OnUpdateCombat()
    {
        ATK = attributesController.attackDamage.GetTrueLinearValue();
        if (!isDead && occupyGridInfo != null)
        {
            attackIntervelTimer += Time.deltaTime;
            attributesController.Regenerate();
            if (occupyGridInfo.gridType == GridType.HexaMap)
                AIActionFsm.curState.OnUpdate();
        }

    }
    public void OnLeaveCombat()
    {
        navMeshAgent.enabled = false;
        championAnimation.animator.SetBool("isInCambat", false);
        Reset();
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
