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
    private Stack<SingleSkillExeProcess> _skillPool = new();
    public bool GlobalProcessIsUsing;
    public SkillExeProcess()
    {
        InitProcess();
    }
    /// <summary>
    /// 初始化执行器
    /// </summary>
    private void InitProcess()
    {
        _globalProcess = new(this);
    }

    public void RegisterSellSkill(System.Action sellSkill)
    {
        _sellSkill += sellSkill;
    }
    
    public void UnRegisterSellSkill(System.Action sellSkill)
    {
        _sellSkill -= sellSkill;
    }

    public void SetCurSkill(ISkillState skill)
    {
        if(skill == null) return;
        if (skill.CheckIsSellSkill())
        {
            _globalProcess.InitSkill(skill);
        }
        else
        {
            if(_skillPool.Count > 0)
            {
                var process = _skillPool.Pop();
                process.InitSkill(skill);
            }
            else
            {
                new SingleSkillExeProcess(this, true).InitSkill(skill);
            }
        }
    }

    public void ReturnToPool(SingleSkillExeProcess process)
    {
        _skillPool.Push(process);
    }
    
    public void ExecuteSkill()
    {
        _globalProcess.TickSkill();
    }
    public void ExecuteSellSkill()
    {
        _sellSkill?.Invoke();
    }

    public ProcessExeState GetCurState()
    {
        return _globalProcess.GetCurState();
    }
}

public class SingleSkillExeProcess
{
    private readonly SkillExeProcess _skillExeProcess;
    private bool _isSellSkill;
    private ISkillState _skill;
    private readonly SkillExeContext _skillExeContext;
    
    private ProcessExeState _state;
    public SingleSkillExeProcess(SkillExeProcess skillExeProcess, bool isSellSkill = false)
    {
        _skillExeProcess = skillExeProcess;
        _isSellSkill = isSellSkill;
        _skillExeContext = new SkillExeContext();
    }

    public void InitSkill(ISkillState skill, bool isGlobalProcess = false)
    {
        
        
        _skill = skill;
        _skill.ResetSkillContext();
        _skillExeContext.Init(_skill);
        if(_isSellSkill)
            _skillExeProcess.RegisterSellSkill(InvokeSkill);
        _state = _skill.HaveTargetInRange() ? ProcessExeState.Ing : ProcessExeState.FindTarget;

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
                    _state = ProcessExeState.Ing;
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
        if (_isSellSkill)
        {
            _skillExeProcess.UnRegisterSellSkill(InvokeSkill);
            _skillExeProcess.ReturnToPool(this);
            _skillExeContext.Release();
        }
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
    private readonly List<SkillInstance> _effectInstances = new ();
    
    private ISkillState _skillState;
    private SkillExeResult _skillExeResult;
    private SkillContext _skillContext;
    private SkillData _skillCfg;
    private ChampionController _owner;
    private ConstructorBase _constructor;
    private SkillHelper.SkillLogicData _logicData;
    public SkillHelper.SkillLogicData LogicData => _logicData;
    // 技能移动速度
    private float _moveSpeed;
    public void Init(ISkillState skillState)
    {
        _skillState = skillState;
        _skillExeResult = SkillExeResult.None;
        _skillContext = skillState.GetContext();
        _skillCfg = skillState.GetSkillCfg();
        _owner = skillState.GetOwner();
        _constructor = skillState.GetConstructor();
        _logicData = SkillHelper.AllLogicDataDic[(SkillHelper.SkillAttackType)_skillCfg.AttackType];
        
        _curDurationTime = 0;
        _curIntervalTime = 0;
        _curEffectCount = 0;
        
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
                //非脱手技能才需要判断朝向
                if (!_skillContext.IsSell && !_owner.TurnToTarget(_constructor))
                {
                    _skillExeResult = SkillExeResult.Ing;
                    break;
                }
                if (!_skillState.IsPrepared())
                {
                    _skillExeResult = SkillExeResult.Fail;
                    break;
                }
                else
                {
                    _owner.skillController.AddUsedSkill(_skillState);
                    Cast();
                    _skillExeResult = SkillExeResult.Ing;
                }
                break;
            case SkillExeResult.Ing:
                OnCastingUpdate();
                foreach (var instance in _effectInstances)
                {
                    instance.UpDateSkill();
                }
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
    }
    #region 技能释放相关
    protected virtual void Cast()
    {
        _owner.buffController.eventCenter.Broadcast(BuffActiveMode.BeforeCast.ToString());
        //扣除施放技能所需的法力值，更新剩余使用次数
        _owner.attributesController.curMana -= _skillCfg.manaCost;
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
        InstantiateEmitInstance(GetCastPoint().position, GetCastPoint().rotation, 1.5f);
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
    /// <summary>
    /// 创建技能投射物特效实例
    /// </summary>
    protected virtual void InstanceEffect()
    {
        var obj = GameObject.Instantiate(_effectPrefab);
        // if (false)_skillContext.isFollowMe)
            // obj.transform.parent = GetCastPoint();
        obj.transform.position = GetCastPoint().position;
        obj.transform.rotation = GetCastPoint().rotation;
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
    protected virtual void OnCastingUpdate()
    {
        if (_curIntervalTime <= 0 && _curEffectCount < _skillCfg.effectCounts)
        {
            _curIntervalTime = _skillContext.IntervalTime;
            _curEffectCount++;
            Effect();
        }
        else
        {
            _curIntervalTime -= Time.deltaTime;
        }
        
        _curDurationTime += Time.deltaTime;
        if (IsFinish())
        {
            OnFinish();
        }
    }
    /// <summary>
    /// 根据技能总持续时间或目标状态（例如目标死亡）判断是否结束技能施放
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsFinish()
    {
        var list = _skillState.GetTargetList();
        return (_curDurationTime >= _skillCfg.duration + _skillCfg.delay) ||
               (list[0] != null && list[0].isDead);
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
        // 释放结束后检测是否需要充能
        if (CheckNeedCharge())
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
