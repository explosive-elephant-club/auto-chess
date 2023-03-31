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
    public int gridType = 0;
    [HideInInspector]
    public int gridPositionX = 0;
    [HideInInspector]
    public int gridPositionZ = 0;

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

    private Vector3 gridTargetPosition;

    private bool _isDragged = false;

    [HideInInspector]
    public bool isAttacking = false;

    [HideInInspector]
    public bool isDead = false;

    private bool isInCombat = false;
    private float combatTimer = 0;

    public GameObject target;
    private List<Effect> effects;

    public ChampionManager championmaneger;

    public Fsm AIActionFsm;

    /// Start is called before the first frame update
    void Start()
    {

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
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        championAnimation = this.GetComponent<ChampionAnimation>();
        buffController = this.GetComponent<BuffController>();
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

    void InitFsm()
    {
        State[] states = transform.Find("States").GetComponents<State>();
        foreach (State s in states)
        {
            AIActionFsm.states.Add(s.name, s);
        }
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
        isInCombat = false;
        target = null;
        isAttacking = false;

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
    /// Assign new grid position
    /// </summary>
    /// <param name="_gridType"></param>
    /// <param name="_gridPositionX"></param>
    /// <param name="_gridPositionZ"></param>
    public void SetGridPosition(int _gridType, int _gridPositionX, int _gridPositionZ)
    {
        gridType = _gridType;
        gridPositionX = _gridPositionX;
        gridPositionZ = _gridPositionZ;


        //set new target when chaning grid position
        gridTargetPosition = GetWorldPosition();
    }

    /// <summary>
    /// Convert grid position to world position
    /// </summary>
    /// <returns></returns>
    public Vector3 GetWorldPosition()
    {
        //get world position
        Vector3 worldPosition = Vector3.zero;

        if (gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            worldPosition = Map.Instance.ownInventoryGridPositions[gridPositionX];
        }
        else if (gridType == Map.GRIDTYPE_OPONENT_INVENTORY)
        {
            worldPosition = Map.Instance.oponentInventoryGridPositions[gridPositionX];
        }
        else if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            worldPosition = Map.Instance.mapGridPositions[gridPositionX, gridPositionZ];
        }

        return worldPosition;
    }

    /// <summary>
    /// Move to corrent world position
    /// </summary>
    public void SetWorldPosition()
    {
        navMeshAgent.enabled = false;

        //get world position
        Vector3 worldPosition = GetWorldPosition();

        this.transform.position = worldPosition;

        gridTargetPosition = worldPosition;
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

    /// <summary>
    /// Find the a champion the the closest world position
    /// </summary>
    /// <returns></returns>
    public GameObject FindTarget()
    {
        GameObject closestEnemy = null;
        float bestDistance = 1000;

        //find enemy
        if (team == ChampionTeam.Player)
        {
            closestEnemy = GamePlayController.Instance.oponentChampionManager.FindTarget(transform.position, bestDistance);
        }
        else if (team == ChampionTeam.Oponent)
        {
            closestEnemy = GamePlayController.Instance.ownChampionManager.FindTarget(transform.position, bestDistance);
        }


        return closestEnemy;
    }

    /// <summary>
    /// Looks for new target to attack if there is any
    /// </summary>
    public void TryAttackNewTarget()
    {
        //find closest enemy
        target = FindTarget();

        //if target found
        if (target != null)
        {
            //set pathfinder target
            //navMeshAgent.destination = target.transform.position;
            //navMeshAgent.isStopped = false;
            MoveToTarget(target.transform);
        }
    }

    public void MoveToTarget(Transform _target)
    {
        if (buffController.buffStateContainer.GetState("immovable"))
        {
            StopMove();
        }
        else
        {
            navMeshAgent.destination = _target.transform.position;
            navMeshAgent.isStopped = false;
        }
    }

    public void StopMove()
    {
        navMeshAgent.isStopped = true;
    }


    /// <summary>
    /// Start attack against enemy champion
    /// </summary>
    private void DoAttack()
    {
        if (!buffController.buffStateContainer.GetState("disarm"))
        {
            isAttacking = true;

            //stop navigation
            navMeshAgent.isStopped = true;

            championAnimation.DoAttack(true);
        }
        else
        {
            StopMove();
        }
    }

    /// <summary>
    /// Called when attack animation finished
    /// </summary>
    public void OnAttackAnimationFinished()
    {
        isAttacking = false;

        if (target != null)
        {
            buffController.eventCenter.Broadcast(BuffActiveMode.BeforeAttack.ToString());
            ChampionController targetChamoion = target.GetComponent<ChampionController>();
            bool isTargetDead = targetChamoion.OnGotHit(currentDamage);


            //target died from attack
            if (isTargetDead)
                TryAttackNewTarget();


            //create projectile if have one
            if (champion.attackProjectile != null && projectileStart != null)
            {
                GameObject projectile = Instantiate(champion.attackProjectile);
                projectile.transform.position = projectileStart.transform.position;

                projectile.GetComponent<Projectile>().Init(target);
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

    #region StageFuncs
    public void OnEnterPreparation()
    {

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
            float distance = Vector3.Distance(gridTargetPosition, this.transform.position);
            if (distance > 0.25f)
            {
                this.transform.position = Vector3.Lerp(this.transform.position, gridTargetPosition, 0.1f);
            }
            else
            {
                this.transform.position = gridTargetPosition;
            }
        }
    }
    public void OnLeavePreparation()
    {

    }

    public void OnEnterCombat()
    {
        IsDragged = false;
        this.transform.position = gridTargetPosition;

        //in combat grid
        if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            isInCombat = true;

            navMeshAgent.enabled = true;

            TryAttackNewTarget();

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
        if (target == null)
        {
            combatTimer += Time.deltaTime;
            if (combatTimer > 0.5f)
            {
                combatTimer = 0;
                TryAttackNewTarget();
            }
        }
        else
        {
            //rotate towards target
            this.transform.LookAt(target.transform, Vector3.up);
            if (target.GetComponent<ChampionController>().isDead == true) //target champion is alive
            {
                target = null;
                StopMove();
            }
            else
            {
                if (isAttacking == false)
                {
                    float distance = Vector3.Distance(this.transform.position, target.transform.position);
                    if (distance < champion.attackRange)
                    {
                        DoAttack();
                    }
                    else
                    {
                        MoveToTarget(target.transform);
                    }
                }
            }
        }

    }
    public void OnLeaveCombat()
    {

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
