using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using System;
using System.Reflection;
using General;

/// <summary>
/// Controls a single champion movement and combat
/// </summary>
public class ChampionController : MonoBehaviour
{
    public GameObject levelupEffectPrefab;
    public GameObject projectileStart;

    [HideInInspector]
    public GridInfo occupyGridInfo;

    [HideInInspector]
    ///Team of this champion, can be player = 0, or enemy = 1
    public ChampionTeam team = ChampionTeam.Player;


    [HideInInspector]
    public Champion champion;

    [HideInInspector]
    ///Maximum health of the champion
    public float maxHealth = 0;

    [HideInInspector]
    ///current health of the champion 
    public float currentHealth = 0;

    [HideInInspector]
    ///Current damage of the champion deals with a attack
    public float currentDamage = 0;

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

    public ChampionManager championmaneger;

    public Fsm AIActionFsm;

    public List<GridInfo> path;

    public int pathStep = 0;

    /// Start is called before the first frame update
    void Awake()
    {
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        championAnimation = this.GetComponent<ChampionAnimation>();
        buffController = this.GetComponent<BuffController>();
    }

    /// <summary>
    /// When champion created Champion and teamID passed
    /// </summary>
    /// <param name="_champion"></param>
    /// <param name="_teamID"></param>
    public void Init(Champion _champion, ChampionTeam _team, ChampionManager _championmaneger)
    {
        champion = _champion;
        team = _team;

        //store scripts

        championmaneger = _championmaneger;

        //disable agent
        navMeshAgent.enabled = false;

        //set stats
        maxHealth = champion.health;
        currentHealth = champion.health;
        currentDamage = champion.damage;

        WorldCanvasController.Instance.AddHealthBar(this.gameObject);

        effects = new List<Effect>();
        GamePlayController.Instance.StageStateAddListener(this);

        AIActionFsm = new Fsm();
        InitFsm();
    }

    public void OnDestroy()
    {
        SetOccupyGridInfo();
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

    /// Update is called once per frame
    void Update()
    {
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
        maxHealth = champion.health * lvl;
        currentHealth = champion.health * lvl;
        isDead = false;
        target = null;

        //reset position
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
        maxHealth = champion.health;
        currentHealth = champion.health;


        if (lvl == 2)
        {
            newSize = 1.5f;
            maxHealth = champion.health * 2;
            currentHealth = champion.health * 2;
            currentDamage = champion.damage * 2;

        }

        if (lvl == 3)
        {
            newSize = 2f;
            maxHealth = champion.health * 3;
            currentHealth = champion.health * 3;
            currentDamage = champion.damage * 3;
        }



        //set size
        this.transform.localScale = new Vector3(newSize, newSize, newSize);

        //instantiate level up effect
        GameObject levelupEffect = Instantiate(levelupEffectPrefab);

        //set position
        levelupEffect.transform.position = this.transform.position;

        //destroy effect after finished
        Destroy(levelupEffect, 1.0f);
    }

    public ChampionController FindTarget(float bestDistance)
    {
        ChampionController closestEnemy = null;

        //find enemy
        if (team == ChampionTeam.Player)
        {
            closestEnemy = GamePlayController.Instance.oponentChampionManager.FindTarget(this, bestDistance);
        }
        else if (team == ChampionTeam.Oponent)
        {
            closestEnemy = GamePlayController.Instance.ownChampionManager.FindTarget(this, bestDistance);
        }
        if (closestEnemy == null)
        {
            return null;
        }
        else
        {
            return closestEnemy;
        }
    }

    public void FindPath()
    {
        if (target != null)
        {

            path = Map.Instance.FindPath(occupyGridInfo, target.occupyGridInfo);
            pathStep = 0;
        }
    }

    public void SetOccupyGridInfo(GridInfo gridInfo = null)
    {
        if (occupyGridInfo != null)
            occupyGridInfo.walkable = true;
        if (gridInfo != null)
        {
            occupyGridInfo = gridInfo;
            occupyGridInfo.walkable = false;
        }
        else
        {
            occupyGridInfo = null;
        }

    }

    public void MoveToNext()
    {
        if (pathStep + 1 < path.Count - 1)
        {
            pathStep += 1;
            SetOccupyGridInfo(path[pathStep]);
            SetWorldPosition();
        }
        else
        {
            path = null;
        }
    }

    float t = 0;
    public void Move()
    {
        if (t > 1)
        {
            if (!path[pathStep + 1].walkable ||
            path[path.Count - 1] != target.occupyGridInfo)
            {
                FindPath();
            }
            MoveToNext();
            t = 0;
        }
        else
        {
            t += Time.deltaTime;
        }
    }

    public void MoveToTarget()
    {
        if (Vector3.Distance(transform.position, path[pathStep].transform.position) <= 0.05)
        {
            if (pathStep + 1 < path.Count - 1)
            {
                if (path[pathStep + 1].walkable && path[path.Count - 1] == target.occupyGridInfo)
                {
                    pathStep++;
                    SetOccupyGridInfo(path[pathStep]);
                }
                else
                {
                    FindPath();
                }
            }
            else
            {
                StopMove();
                path = null;
                SetWorldPosition();
            }
        }
        else
        {
            navMeshAgent.destination = path[pathStep].transform.position;
            navMeshAgent.isStopped = false;
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
            ChampionController targetChamoion = target.GetComponent<ChampionController>();
            targetChamoion.OnGotHit(currentDamage);

            //create projectile if have one
            if (champion.attackProjectile != null && projectileStart != null)
            {
                GameObject projectile = Instantiate(champion.attackProjectile);
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
    public bool OnGotHit(float damage)
    {
        buffController.eventCenter.Broadcast(BuffActiveMode.BeforeHit.ToString());
        currentHealth -= damage;


        //death
        if (currentHealth <= 0)
        {
            this.gameObject.SetActive(false);
            isDead = true;
            championmaneger.OnChampionDeath();
        }
        //add floating text
        WorldCanvasController.Instance.AddDamageText(this.transform.position + new Vector3(0, 2.5f, 0), damage);

        buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());
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
        Reset();
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

        //in combat grid
        if (occupyGridInfo.gridType == GridType.HexaMap)
        {
            navMeshAgent.enabled = true;
            championAnimation.InitBehavour();
            championAnimation.animator.SetBool("isInCambat", true);
        }

        //添加羁绊Buff
        List<BaseBuffData> activeBonuses = championmaneger.bonusBuffList;

        foreach (BaseBuffData b in activeBonuses)
        {
            buffController.AddBuff(b, gameObject);
        }
    }
    public void OnUpdateCombat()
    {
        AIActionFsm.curState.OnUpdate();
    }
    public void OnLeaveCombat()
    {
        championAnimation.animator.SetBool("isInCambat", false);
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
