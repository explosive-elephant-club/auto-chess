using System;
using System.Collections.Generic;
using ExcelConfig;
using UnityEngine;

/// <summary>
/// 技能执行器
/// 把技能的数据和行为解耦
/// 后续需要做的事情：
/// 主执行器作为一个概念，每个技能在释放阶段(寻敌，释放动作)都需要占用，占用时不能直接执行下一个技能
/// 脱手技能实际为持续释放时不占用主执行器，主执行器继续释放下一个技能
/// </summary>
public class SkillExeProcess
{
    private SingleSkillExeProcess _globalProcess;
    private System.Action _sellSkill;
    private System.Action _sellSkillReset;
    private readonly Stack<SingleSkillExeProcess> _skillPool = new();

    public void RegisterSellSkill(System.Action reset, System.Action sellSkill)
    {
        _sellSkill += sellSkill;
        _sellSkillReset += reset;
    }
    
    public void UnRegisterSellSkill(System.Action reset, System.Action sellSkill)
    {
        _sellSkill -= sellSkill;
        _sellSkillReset -= reset;
    }

    public void SetCurSkill(ISkillState skill)
    {
        if(skill == null) return;
        var process = _skillPool.Count > 0 ? _skillPool.Pop() : new SingleSkillExeProcess(this);
        _globalProcess = process;
        _globalProcess.InitSkill(skill);
    }

    public void ReturnToPool(SingleSkillExeProcess process)
    {
        _skillPool.Push(process);
    }

    // 脱手技能施法动作结束, 进入自动释放阶段, 开始进入下一个技能
    public void SetToSellCast()
    {
        _globalProcess = null;
    }
    
    public void ExecuteSkill()
    {
        if(_globalProcess != null)
            _globalProcess.TickSkill();
    }
    public void ExecuteSellSkill()
    {
        _sellSkill?.Invoke();
    }

    public ProcessExeState GetCurState()
    {
        return _globalProcess?.GetCurState() ?? ProcessExeState.None;
    }

    public void Reset()
    {
        _sellSkillReset?.Invoke();
        if(_globalProcess != null)
            _globalProcess.Reset();
        _globalProcess = null;
    }
}

public class SingleSkillExeProcess
{
    private readonly SkillExeProcess _skillExeProcess;
    private ISkillState _skill;
    private readonly SkillExeContext _skillExeContext;
    private ProcessExeState _state;
    private bool _isInSellCast;
    public SingleSkillExeProcess(SkillExeProcess skillExeProcess)
    {
        _skillExeProcess = skillExeProcess;
        _skillExeContext = new SkillExeContext();
    }

    public void InitSkill(ISkillState skill)
    {
        _skill = skill;
        _skill.ResetSkillContext();
        _skillExeContext.Init(_skill);
        // _skillExeProcess.RegisterSellSkill(Reset, InvokeSkill);
        _isInSellCast = false;
        _state = _skill.HaveTargetInRange() ? ProcessExeState.Casting : ProcessExeState.FindTarget;
    }

    private void InvokeSkill()
    {
        TickSkill();
    }

    public ProcessExeState TickSkill()
    {
        switch (_state)
        {
            case ProcessExeState.FindTarget:
                if (_skill.HaveTargetInRange())
                {
                    _state = ProcessExeState.Casting;
                }
                break;
            case ProcessExeState.Casting:
                var castState = _skillExeContext?.ExeState();
                if (castState == SkillExeResult.Ing)
                {
                    _state = ProcessExeState.Ing;
                    if (_skill.CheckIsSellSkill())
                    {
                        _skillExeProcess.SetToSellCast();
                        _skillExeProcess.RegisterSellSkill(Reset, InvokeSkill);
                        _isInSellCast = true;
                    }
                }
                else if (castState == SkillExeResult.Fail)
                {
                    OnSkillEnd();
                }
                break;
            case ProcessExeState.Ing:
                var state = _skillExeContext?.ExeState();
                _state = state is SkillExeResult.Done or SkillExeResult.Fail ? ProcessExeState.Done : ProcessExeState.Ing;
                if (state == SkillExeResult.Done)
                {
                    OnSkillEnd();
                }
                break;
        }
        return _state;
    }

    public ProcessExeState GetCurState()
    {
        return _state;
    }

    private void OnSkillEnd()
    {
        if (_isInSellCast)
        {
            _skillExeProcess.UnRegisterSellSkill(Reset, InvokeSkill);
        }
        _skillExeProcess.ReturnToPool(this);
        _skillExeContext.Release();
    }

    public void Reset()
    {
        OnSkillEnd();
    }
}

/// <summary>
/// 技能执行过程中的上下文, 用来控制技能实例的生命周期
/// </summary>
public class SkillExeContext
{
    /// <summary>
    /// 技能当前执行时间
    /// </summary>
    private float _curDurationTime;
    /// <summary>
    /// 当前已生效(释放)次数
    /// </summary>
    private uint _curEffectCount = 0;
    /// <summary>
    /// 技能当前生效间隔
    /// </summary>
    private float _curIntervalTime;
    /// <summary>
    /// 发射特效预制体
    /// </summary>
    private GameObject _emitPrefab;
    /// <summary>
    /// 投射物预制体
    /// </summary>
    private GameObject _effectPrefab;
    /// <summary>
    /// 击中特效预制体
    /// </summary>
    private GameObject _hitFXPrefab;
    /// <summary>
    /// 目前技能特效生成点
    /// </summary>
    private int _curCastPointIndex;
    /// <summary>
    ///  伤害次数 => 默认一次后技能结束, 只会在脱手技能中才会生效, 用于做穿透, 比如子弹, 穿透一次就是 伤害两次
    /// </summary>
    private int _totalHitCount;
    private int _hitCount;
    
    
    private readonly List<SkillInstance> _effectInstances = new ();
    
    private ISkillState _skillState;
    private SkillExeResult __skillExeResult;

    private SkillExeResult _skillExeResult
    {
        get => __skillExeResult;
        set
        {
            Debug.Log($"stateChange {__skillExeResult} => {value}");
            __skillExeResult = value;
        }
    }
    private SkillContext _skillContext;
    private SkillData _skillCfg;
    private ChampionController _owner;
    private ConstructorBase _constructor;
    private SkillHelper.SkillLogicData _logicData;
    public SkillHelper.SkillLogicData LogicData => _logicData;
    public SkillTargetType SkillTargetType => _skillContext.SkillTargetType;
    public ChampionTeam Team => _owner.team;
    private SkillHelper.SkillAttackType _attackType;
    public SkillHelper.SkillAttackType AttackType => _attackType;
    // 技能移动速度
    private float _moveSpeed;

    /// <summary>
    /// 技能的这次释放是否已经CD过了
    /// </summary>
    private bool _isCd;
    private bool _canFinish;
    /// <summary>
    /// 设置技能是否可以结束, 只在伤害后消失(结束) 技能中生效
    /// </summary>
    /// <param name="canFinish"></param>
    private int _counter; // 多导弹的计数器, 临时先这么处理, 后续想想有没有更好的方法, --考虑跟技能实例一一绑定,防止同一个技能多次触发导致counter数据不对
    
    public void Init(ISkillState skillState)
    {
        _skillState = skillState;
        _skillExeResult = SkillExeResult.None;
        _skillContext = skillState.GetContext();
        _skillCfg = skillState.GetSkillCfg();
        _owner = skillState.GetOwner();
        _constructor = skillState.GetConstructor();
        _attackType = (SkillHelper.SkillAttackType)_skillCfg.AttackType;
        _logicData = SkillHelper.AllLogicDataDic[_attackType];
        _curDurationTime = 0;
        _curIntervalTime = 0;
        _curEffectCount = 0;
        _totalHitCount = 1;
        _hitCount = 0;
        _isCd = false;
        _canFinish = false;
        _counter = 0;
        
        //特效加载
        var path = SkillHelper.GetVFXPath(_skillCfg.ID);
        if (_skillCfg.emitFXPrefab)
            _emitPrefab = Resources.Load<GameObject>(path + "Emit");
        if (_skillCfg.effectPrefab)
        {
            _effectPrefab = Resources.Load<GameObject>(path + "Effect");
        }
        if (_skillCfg.hitFXPrefab)
            _hitFXPrefab = Resources.Load<GameObject>(path + "Hit");
    }

    private bool CheckNeedCharge()
    {
        return !_owner.skillController.CheckIsLastSkill();
    }
        
    public virtual SkillExeResult ExeState()
    {
        switch (_skillExeResult)
        {
            case SkillExeResult.None:
                if (!_skillState.IsStartCd())
                {
                    _skillExeResult = SkillExeResult.Prepare;
                }
                break;
            case SkillExeResult.Prepare:
                if (_owner.TurnToTarget(_constructor))
                {
                    break;
                }
                if (!_skillState.IsPrepared())
                {
                    _skillExeResult = SkillExeResult.Fail;
                    break;
                }
                else
                {
                    _skillExeResult = SkillExeResult.Casting;
                    _owner.skillController.AddUsedSkill(_skillState);
                    Cast();
                }
                break;
            case SkillExeResult.Casting:
                foreach (var instance in _effectInstances)
                {
                    instance.UpDateSkill();
                }
                OnCastingUpdate(out var effectCountIsOver);
                if (effectCountIsOver)
                {
                    _skillExeResult = SkillExeResult.Ing;
                    if (!_isCd && !SkillHelper.CheckIsNeedContinuousCasting(_attackType) && CheckNeedCharge())
                    {
                        _isCd = true;
                        _skillState.StartCastCdCutDown();
                    }
                }
                break;
            case SkillExeResult.Ing:
                foreach (var instance in _effectInstances)
                {
                    instance.UpDateSkill();
                }
                UpdateDuration();
                break;
            case SkillExeResult.ChargeEnergy:
                if(!_skillState.IsStartCd())
                    _skillExeResult = SkillExeResult.Done;
                break;
            case SkillExeResult.Done:
                break;
            case SkillExeResult.Fail:
                break;
        }
        return _skillExeResult;
    }
    
    public void Release()
    {
        DestroyEffect();
    }
    
    #region 技能释放相关
    public Transform ReGetTarget()
    {
        _skillState.HaveTargetInRange();
        var list = _skillState.GetTargetList();
        var result = list.Count > 0 ? list[0].transform : null;
        if (result == null)
        {
            OnFinish();
        }
        return result;
    }

    public float GetMoveSpeed()
    {
        return _skillCfg.MoveSpeed;
    }
    protected virtual void Cast()
    {
        _owner.buffController.eventCenter.Broadcast(BuffActiveMode.BeforeCast.ToString());
        //扣除施放技能所需的法力值，更新剩余使用次数
        _owner.attributesController.curMana -= 30;//_skillCfg.manaCost;
        if (_skillContext.CountRemain != -1)
            _skillContext.CountRemain -= 1;
        //播放施放动画
        PlayCastAnim();
        _owner.buffController.eventCenter.Broadcast(BuffActiveMode.AfterCast.ToString());
    }
    
    /// <summary>
    /// 获取当前技能施放点
    /// </summary>
    /// <returns></returns>
    private Transform GetCastPoint()
    {
        return _constructor.skillCastPoints[_curCastPointIndex];
    }
    /// <summary>
    /// 技能生效
    /// </summary>
    protected virtual void Effect()
    {
        //直接生效
        if (_skillCfg.isDirectEffect)
        {
            DirectEffect();
        }
        else  //生成技能特效弹道
        {
            InstanceEffect();
        }
    }
    /// <summary>
    /// 直接生效 对目标直接造成伤害并施加buff
    /// </summary>
    protected virtual void DirectEffect()
    {
        var targetList = _skillState.GetTargetList();
        if (targetList == null || targetList.Count == 0)
            return;
        var position = _logicData.IsCreateInSelf ? _owner.transform.position : GetCastPoint().position;
        var rotation = _logicData.IsCreateInSelf ? _owner.transform.rotation : GetCastPoint().rotation;
        InstantiateEmitInstance(position, rotation, 1.5f);
        _curCastPointIndex = (_curCastPointIndex + 1) % _constructor.skillCastPoints.Length;
        foreach (ChampionController C in targetList)
        {
            if (!C.isDead)
            {
                InstantiateHitInstance(C.transform.position, Quaternion.FromToRotation(Vector3.up, Vector3.zero), 1.5f);
                AddBuffToTarget(C);
                AddDMGToTarget(C);
            }
        }
    }

    public void SkillHitEffect(Collider c, ChampionController target, bool onlyEffect = false)
    {
        InstantiateHitInstance(c.bounds.ClosestPoint(target.transform.position), Quaternion.FromToRotation(Vector3.up, Vector3.zero), 1.5f);
        if(onlyEffect) return;
        AddBuffToTarget(target);
        AddDMGToTarget(target);
        
        if(++_hitCount >= _totalHitCount)
            SetCanFinish(true);
    }
    /// <summary>
    /// 创建技能投射物特效实例
    /// </summary>
    protected virtual void InstanceEffect()
    {
        var obj = GameObject.Instantiate(_effectPrefab);
        obj.transform.position = _logicData.IsCreateInSelf ? _owner.transform.position : GetCastPoint().position;
        obj.transform.rotation = _logicData.IsCreateInSelf ? _owner.transform.rotation : GetCastPoint().rotation;
        _curCastPointIndex = (_curCastPointIndex + 1) % _constructor.skillCastPoints.Length;
        var com = obj.GetComponent<SkillInstance>();
        if(com == null)
            com = obj.AddComponent<SkillInstance>();
        com.Init(this, _owner.transform, _skillState.GetTargetList()[0].transform);
        _effectInstances.Add(com);
    }

    /// <summary>
    /// 对目标施加buff
    /// </summary>
    /// <param name="target">目标</param>
    protected virtual void AddBuffToTarget(ChampionController target)
    {
        foreach (int buff_ID in _skillCfg.addBuffs)
        {
            if (buff_ID != 0)
                target.buffController.AddBuff(buff_ID, _owner);
        }
    }
    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    /// <param name="target">目标</param>
    protected virtual void AddDMGToTarget(ChampionController target)
    {
        if (!string.IsNullOrEmpty(_skillCfg.damageData[0].type))
        {
            _owner.championCombatController.TakeDamage(target, _skillCfg.damageData);
        }
    }

    /// <summary>
    /// 技能持续施法时Update
    /// </summary>
    protected virtual void OnCastingUpdate(out bool effectCountIsOver)
    {
        effectCountIsOver = _curEffectCount >= _skillCfg.effectCounts;
        if (!effectCountIsOver)
        {
            if (_curIntervalTime <= 0)
            {
                Effect();
                _curEffectCount++;
                _curIntervalTime = _skillContext.IntervalTime;
                effectCountIsOver = _curEffectCount >= _skillCfg.effectCounts;
            }
            else
            {
                _curIntervalTime -= Time.deltaTime;
            }
        }

        UpdateDuration();
    }

    private void UpdateDuration()
    {
        _curDurationTime += Time.deltaTime;
        if (IsFinish())
        {
            OnFinish();
        }
    }

    public void SetCanFinish(bool canFinish)
    {
        if (canFinish)
        {
            _counter--;
        }
        else
        {
            _counter++;
        }
        _canFinish = _counter <= 0;
    }
    /// <summary>
    /// 脱手技能 需要等技能全部执行完了才算结束
    /// 非脱手技能 根据技能总持续时间或目标状态（例如目标死亡）判断是否结束技能施放
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsFinish()
    {
        // var list = _skillState.GetTargetList();
        if (SkillHelper.CheckIsDamageDestroySkill(_attackType))
        {
            return _canFinish;
        }
        else
        {
            // 这一帧执行的时候已经重新获取过目标列表了,这里不需要重新再get一遍
            var target = _skillState.GetTargetList(); 
            return target is { Count: > 0 } && _curDurationTime >= _skillCfg.duration + _skillCfg.delay;
        }
    }
    
    /// <summary>
    /// 销毁所有已生成的技能特效实例
    /// </summary>
    protected virtual void DestroyEffect()
    {
        if (_effectInstances.Count > 0)
        {
            foreach (var e in _effectInstances)
            {
                if (e != null)
                    e.DestroySelf();
            }
            _effectInstances.Clear();
        }
    }
    /// <summary>
    /// 技能结束
    /// </summary>
    protected virtual void OnFinish()
    {
        PlayEndAnim();
        DestroyEffect();
        // 非持续释放技能在技能结束时 检测是否需要充能
        if (SkillHelper.CheckIsNeedContinuousCasting(_attackType) && CheckNeedCharge())
        {
            _skillState.StartCastCdCutDown();
            _skillExeResult = SkillExeResult.ChargeEnergy;
        }
        else
        {
            _skillExeResult = SkillExeResult.Done;
        }
    }

    /// <summary>
    /// 播放释放动画
    /// </summary>
    protected virtual void PlayCastAnim()
    {

        if (!string.IsNullOrEmpty(_skillCfg.skillAnimTrigger[0].constructorType))
        {
            //触发动画
            foreach (var animTrigger in _skillCfg.skillAnimTrigger)
            {
                foreach (var c in _constructor.GetAllParentConstructors(true))
                {
                    if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                    {
                        c.enablePlayNewSkillAnim = false;
                        c.animator.SetTrigger(animTrigger.trigger);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 播放持续释放结束动画
    /// </summary>
    protected virtual void PlayEndAnim()
    {
        //绑定动画结束时间
        if (!string.IsNullOrEmpty(_skillCfg.skillAnimTrigger[0].constructorType))
        {
            //触发动画
            foreach (var animTrigger in _skillCfg.skillAnimTrigger)
            {
                foreach (var c in _constructor.GetAllParentConstructors(true))
                {
                    if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                    {
                        if (HasParameter(c.animator, "EndCasting"))
                            c.animator.SetTrigger("EndCasting");
                    }
                }
            }
        }
    }
    /// <summary>
    /// 实例化发射特效
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="rot">旋转</param>
    /// <param name="duration">持续时间</param>
    /// <returns></returns>
    protected virtual GameObject InstantiateEmitInstance(Vector3 pos, Quaternion rot, float duration)
    {
        if (_emitPrefab != null)
        {
            GameObject emitParticleInstance = GameObject.Instantiate(_emitPrefab, pos, rot) as GameObject;
            GameObject.Destroy(emitParticleInstance, duration);
            return emitParticleInstance;
        }
        return null;
    }
    /// <summary>
    /// 实例化命中特效
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="rot">旋转</param>
    /// <param name="duration">持续时间</param>
    /// <returns></returns>
    protected virtual GameObject InstantiateHitInstance(Vector3 pos, Quaternion rot, float duration)
    {
        if (_hitFXPrefab != null)
        {
            GameObject hitParticleInstance = GameObject.Instantiate(_hitFXPrefab, pos, rot) as GameObject;
            GameObject.Destroy(hitParticleInstance, duration);
            return hitParticleInstance;
        }
        return null;
    }
    /// <summary>
    /// 判断动画状态机中是否存在某个指定参数名
    /// </summary>
    /// <param name="animator">动画状态机</param>
    /// <param name="parameterName">参数名</param>
    /// <returns></returns>
    protected virtual bool HasParameter(Animator animator, string parameterName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == parameterName)
                return true;
        }
        return false;
    }
    #endregion
}
