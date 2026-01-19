using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

/// <summary>
/// 技能状态机
/// 管理技能的执行阶段和状态转换
/// 实现 ISkillExecutionContext 供 SkillInstance 使用
/// </summary>
public class SkillStateMachine : ISkillExecutionContext
{
    private readonly SkillRuntime _runtime;
    private readonly SkillTargetFinder _targetFinder;
    private readonly SkillEffectFactory _effectFactory;
    private readonly SkillHitHandler _hitHandler;
    private readonly SkillVFXLoader _vfxLoader;

    #region ISkillExecutionContext 实现
    public SkillHelper.SkillLogicData LogicData => _runtime.LogicData;
    public SkillHelper.SkillAttackType AttackType => _runtime.AttackType;
    public SkillTargetType SkillTargetType => _runtime.SkillTargetType;
    public ChampionTeam Team => _runtime.Owner.team;
    
    public float GetMoveSpeed() => _runtime.GetMoveSpeed();
    
    public Transform ReGetTarget()
    {
        _runtime.SkillState.HaveTargetInRange();
        var list = _runtime.SkillState.GetTargetList();
        var result = list.Count > 0 ? list[0].transform : null;
        if (result == null)
        {
            _runtime.CurrentPhase = SkillPhase.Finished;
        }
        return result;
    }
    
    public void SetCanFinish(bool canFinish) => _runtime.SetCanFinish(canFinish);
    
    public void SkillHitEffect(Collider collider, ChampionController target, bool onlyEffect = false)
    {
        HandleSkillHit(collider, target, onlyEffect);
    }
    #endregion

    public SkillStateMachine(SkillRuntime runtime)
    {
        _runtime = runtime;
        _targetFinder = new SkillTargetFinder();
        _effectFactory = new SkillEffectFactory();
        _hitHandler = new SkillHitHandler();
        _vfxLoader = SkillVFXLoader.Instance;
    }

    /// <summary>
    /// 初始化状态机
    /// </summary>
    public void Initialize(ISkillState skillState)
    {
        _runtime.Initialize(skillState);
        
        // 预加载VFX资源
        _vfxLoader.GetVFXBundle(_runtime.SkillData.ID);
    }

    /// <summary>
    /// 执行状态机tick
    /// </summary>
    /// <returns>当前执行结果</returns>
    public SkillExeResult Tick()
    {
        switch (_runtime.CurrentPhase)
        {
            case SkillPhase.Idle:
                return HandleIdle();
                
            case SkillPhase.WaitingCD:
                return HandleWaitingCD();
                
            case SkillPhase.FindingTarget:
                return HandleFindingTarget();
                
            case SkillPhase.Casting:
                return HandleCasting();
                
            case SkillPhase.Executing:
                return HandleExecuting();
                
            case SkillPhase.Finished:
            case SkillPhase.Failed:
                return _runtime.CurrentPhase.ToExeResult();
        }

        return SkillExeResult.None;
    }

    #region 状态处理
    private SkillExeResult HandleIdle()
    {
        if (!_runtime.SkillState.IsStartCd())
        {
            _runtime.CurrentPhase = SkillPhase.FindingTarget;
        }
        return SkillExeResult.None;
    }

    private SkillExeResult HandleWaitingCD()
    {
        if (!_runtime.SkillState.IsStartCd())
        {
            _runtime.CurrentPhase = SkillPhase.Finished;
        }
        return SkillExeResult.ChargeEnergy;
    }

    private SkillExeResult HandleFindingTarget()
    {
        // 转向目标
        if (_runtime.Owner.TurnToTarget(_runtime.Constructor))
        {
            return SkillExeResult.Prepare;
        }

        // 检查技能是否可用
        if (!_runtime.SkillState.IsPrepared())
        {
            _runtime.CurrentPhase = SkillPhase.Failed;
            return SkillExeResult.Fail;
        }

        // 开始施法
        _runtime.CurrentPhase = SkillPhase.Casting;
        _runtime.Owner.skillController.AddUsedSkill(_runtime.SkillState);
        Cast();
        
        return SkillExeResult.Casting;
    }

    private SkillExeResult HandleCasting()
    {
        // 更新所有技能实例
        UpdateEffectInstances();

        // 执行施法更新
        bool effectCountIsOver = OnCastingUpdate();

        if (effectCountIsOver)
        {
            _runtime.CurrentPhase = SkillPhase.Executing;
            
            // 非持续施法技能开始CD
            if (!_runtime.HasStartedCD && !_runtime.NeedsContinuousCasting && CheckNeedCharge())
            {
                _runtime.HasStartedCD = true;
                _runtime.SkillState.StartCastCdCutDown();
            }
        }

        UpdateDuration();
        return SkillExeResult.Casting;
    }

    private SkillExeResult HandleExecuting()
    {
        // 更新所有技能实例
        UpdateEffectInstances();

        UpdateDuration();

        if (_runtime.ShouldFinish())
        {
            OnFinish();
            return SkillExeResult.Done;
        }

        return SkillExeResult.Ing;
    }
    #endregion

    #region 施法相关
    private void Cast()
    {
        _runtime.Owner.buffController.eventCenter.Broadcast(BuffActiveMode.BeforeCast.ToString());
        
        // 扣除法力值
        _runtime.Owner.attributesController.curMana -= 30; // 后续应改为 _runtime.SkillData.manaCost
        
        // 更新剩余使用次数
        if (_runtime.Context.CountRemain != -1)
            _runtime.Context.CountRemain -= 1;

        // 播放施法动画
        PlayCastAnim();
        
        _runtime.Owner.buffController.eventCenter.Broadcast(BuffActiveMode.AfterCast.ToString());
    }

    private bool OnCastingUpdate()
    {
        bool effectCountIsOver = _runtime.IsEffectCountComplete();
        
        if (!effectCountIsOver)
        {
            if (_runtime.CurrentIntervalTime <= 0)
            {
                ExecuteEffect();
                _runtime.CurrentEffectCount++;
                _runtime.CurrentIntervalTime = _runtime.Context.IntervalTime;
                effectCountIsOver = _runtime.IsEffectCountComplete();
            }
            else
            {
                _runtime.CurrentIntervalTime -= Time.deltaTime;
            }
        }

        return effectCountIsOver;
    }

    private void ExecuteEffect()
    {
        if (_runtime.SkillData.isDirectEffect)
        {
            ExecuteDirectEffect();
        }
        else
        {
            CreateProjectileEffect();
        }
    }

    private void ExecuteDirectEffect()
    {
        var targetList = _runtime.SkillState.GetTargetList();
        if (targetList == null || targetList.Count == 0)
            return;

        var position = _runtime.LogicData.IsCreateInSelf 
            ? _runtime.Owner.transform.position 
            : _runtime.GetCurrentCastPoint().position;
        var rotation = _runtime.LogicData.IsCreateInSelf 
            ? _runtime.Owner.transform.rotation 
            : _runtime.GetCurrentCastPoint().rotation;

        // 生成发射特效
        _effectFactory.CreateEmitEffect(_runtime.SkillData.ID, position, rotation, SkillConstants.DEFAULT_EMIT_EFFECT_DURATION);
        _runtime.MoveToNextCastPoint();

        // 对所有目标造成效果
        var directContext = new DirectEffectContext
        {
            Caster = _runtime.Owner,
            Targets = targetList,
            SkillData = _runtime.SkillData,
            HitEffectDuration = SkillConstants.DEFAULT_HIT_EFFECT_DURATION
        };
        _hitHandler.ApplyDirectEffect(directContext);
    }

    private void CreateProjectileEffect()
    {
        var target = _runtime.PrimaryTarget;
        if (target == null)
            return;
            
        var createContext = new EffectCreateContext
        {
            SkillData = _runtime.SkillData,
            Owner = _runtime.Owner,
            Target = target,
            CastPoint = _runtime.GetCurrentCastPoint(),
            UseOwnerPosition = _runtime.LogicData.IsCreateInSelf,
            ExecutionContext = this
        };

        var skillInstance = _effectFactory.CreateProjectile(createContext);
        if (skillInstance != null)
        {
            // 使用新的 ISkillExecutionContext 接口初始化
            skillInstance.Init(this, _runtime.Owner.transform, target.transform);
            _runtime.EffectInstances.Add(skillInstance);
        }

        _runtime.MoveToNextCastPoint();
    }
    #endregion

    #region 辅助方法
    private void UpdateEffectInstances()
    {
        foreach (var instance in _runtime.EffectInstances)
        {
            if (instance != null)
            {
                instance.UpDateSkill();
            }
        }
    }

    private void UpdateDuration()
    {
        _runtime.UpdateDuration(Time.deltaTime);
    }

    private bool CheckNeedCharge()
    {
        return !_runtime.Owner.skillController.CheckIsLastSkill();
    }

    private void OnFinish()
    {
        PlayEndAnim();
        DestroyEffects();

        // 持续释放技能在结束时检查是否需要充能
        if (_runtime.NeedsContinuousCasting && CheckNeedCharge())
        {
            _runtime.SkillState.StartCastCdCutDown();
            _runtime.CurrentPhase = SkillPhase.WaitingCD;
        }
        else
        {
            _runtime.CurrentPhase = SkillPhase.Finished;
        }
    }

    private void DestroyEffects()
    {
        foreach (var instance in _runtime.EffectInstances)
        {
            if (instance != null)
            {
                instance.DestroySelf();
            }
        }
        _runtime.EffectInstances.Clear();
    }
    #endregion

    #region 动画相关
    private void PlayCastAnim()
    {
        var skillData = _runtime.SkillData;
        if (string.IsNullOrEmpty(skillData.skillAnimTrigger[0].constructorType))
            return;

        foreach (var animTrigger in skillData.skillAnimTrigger)
        {
            foreach (var c in _runtime.Constructor.GetAllParentConstructors(true))
            {
                if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                {
                    c.enablePlayNewSkillAnim = false;
                    c.animator.SetTrigger(animTrigger.trigger);
                }
            }
        }
    }

    private void PlayEndAnim()
    {
        var skillData = _runtime.SkillData;
        if (string.IsNullOrEmpty(skillData.skillAnimTrigger[0].constructorType))
            return;

        foreach (var animTrigger in skillData.skillAnimTrigger)
        {
            foreach (var c in _runtime.Constructor.GetAllParentConstructors(true))
            {
                if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                {
                    if (HasParameter(c.animator, "EndCasting"))
                        c.animator.SetTrigger("EndCasting");
                }
            }
        }
    }

    private bool HasParameter(Animator animator, string parameterName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == parameterName)
                return true;
        }
        return false;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 处理技能命中
    /// </summary>
    public void HandleSkillHit(Collider collider, ChampionController target, bool onlyEffect = false)
    {
        var hitContext = new HitContext
        {
            Caster = _runtime.Owner,
            Target = target,
            SkillData = _runtime.SkillData,
            HitPosition = collider.bounds.ClosestPoint(target.transform.position),
            HitCollider = collider,
            OnlyEffect = onlyEffect
        };

        _hitHandler.HandleHit(hitContext);

        if (!onlyEffect && ++_runtime.CurrentHitCount >= _runtime.TotalHitCount)
        {
            _runtime.SetCanFinish(true);
        }
    }

    /// <summary>
    /// 获取目标查找器
    /// </summary>
    public SkillTargetFinder GetTargetFinder() => _targetFinder;

    /// <summary>
    /// 获取命中处理器
    /// </summary>
    public SkillHitHandler GetHitHandler() => _hitHandler;

    /// <summary>
    /// 获取效果工厂
    /// </summary>
    public SkillEffectFactory GetEffectFactory() => _effectFactory;

    /// <summary>
    /// 获取运行时数据
    /// </summary>
    public SkillRuntime GetRuntime() => _runtime;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Release()
    {
        DestroyEffects();
    }
    #endregion
}
