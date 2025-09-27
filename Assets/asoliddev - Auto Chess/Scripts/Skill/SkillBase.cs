using System;
using System.Collections.Generic;
using ExcelConfig;
using UnityEngine;

public class SkillBase : ISkillState
{
    private readonly SkillData _skillCfg;
    private readonly SkillContext _skillContext;
    private ConstructorBase _constructor;
    private SkillExeResult _skillExeResult;
    #region 参数
    /// <summary>
    /// 技能的拥有者
    /// </summary>
    private readonly ChampionController _owner;
    /// <summary>
    /// 技能的拥有者的管理器
    /// </summary>
    private ChampionManager _manager;
    /// <summary>
    /// 目标选择器
    /// </summary>
    private readonly SkillTargetsSelector _targetsSelector;
    /// <summary>
    /// 被选中的目标
    /// </summary>
    private SelectorResult _selectorResult;
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
    private List<SkillEffect> _effectInstances = new List<SkillEffect>();
    private bool _isStartCd;
    #endregion
    
    public SkillBase(SkillData skillData, ChampionController championController, ConstructorBase constructor)
    {
        _skillCfg = skillData;
        _skillContext = new SkillContext();
        //目标选择器加载
        _selectorResult = new SelectorResult();
        _targetsSelector = new SkillTargetsSelector();  
        _constructor = constructor;

        _owner = championController;
        _manager = championController.championManeger;
        //特效加载
        var path = SkillHelper.GetVFXPath(skillData.ID);
        if (skillData.emitFXPrefab)
            _emitPrefab = Resources.Load<GameObject>(path + "Emit");
        if (skillData.effectPrefab)
        {
            _effectPrefab = Resources.Load<GameObject>(path + "Effect");
        }
        if (skillData.hitFXPrefab)
            _hitFXPrefab = Resources.Load<GameObject>(path + "Hit");
    }

    public virtual SkillExeResult CheckState()
    {
        return _skillExeResult;
    }
    public SkillData GetSkillCfg()
    {
        return _skillCfg;
    }

    private bool CheckTurnToTarget()
    {
        return _owner.TurnToTarget(_constructor);
    }

    /// <summary>
    /// 这个技能释放后的充能时间,充能结束之后才会结束技能的释放,但如果是在技能链的最后一个技能(最后非null节点),则不需要这个充能时间
    /// </summary>
    private void StartCastCdCutDown()
    {
        if(_isStartCd) return;
        _isStartCd = true;
        _skillContext.CdCutDown = _owner.attributesController.castDelay.GetTrueValue(GetSkillCastDelay());
        _skillContext.CurAllCutDown = _skillContext.CdCutDown;
    }

    private bool CheckNeedCharge()
    {
        return !_owner.skillController.CheckIsLastSkill();
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
        if (_selectorResult.targets.Count == 0)
            return;
        if (_selectorResult.targets != null)
        {
            InstantiateEmitInstance(GetCastPoint().position, GetCastPoint().rotation, 1.5f);
            _curCastPointIndex = (_curCastPointIndex + 1) % _constructor.skillCastPoints.Length;
            foreach (ChampionController C in _selectorResult.targets)
            {
                if (!C.isDead)
                {
                    InstantiateHitInstance(C.transform.position, Quaternion.FromToRotation(Vector3.up, Vector3.zero), 1.5f);
                    AddBuffToTarget(C);
                    AddDMGToTarget(C);
                }
            }
        }
    }
    /// <summary>
    /// 创建技能投射物特效实例
    /// </summary>
    protected virtual void InstanceEffect()
    {
        var obj = GameObject.Instantiate(_effectPrefab);
        if (false)//_skillContext.isFollowMe)
            obj.transform.parent = GetCastPoint();
        obj.transform.position = GetCastPoint().position;
        obj.transform.rotation = GetCastPoint().rotation;
        _curCastPointIndex = (_curCastPointIndex + 1) % _constructor.skillCastPoints.Length;
        SkillEffect skillEffect = obj.GetComponent<SkillEffect>();
        // skillEffect.Init(this, _selectorResult.targets[0].transform);
        _effectInstances.Add(skillEffect);
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
        if (_skillContext.CurIntervalTime <= 0 && _skillContext.CurEffectCount < _skillCfg.effectCounts)
        {
            _skillContext.CurIntervalTime = _skillContext.IntervalTime;
            _skillContext.CurEffectCount++;
            Effect();
        }
        else
        {
            _skillContext.CurIntervalTime -= Time.deltaTime;
        }
        
        _skillContext.CurDurationTime += Time.deltaTime;
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
        return (_skillContext.CurDurationTime >= _skillCfg.duration + _skillCfg.delay) || (_selectorResult.targets[0] != null && _selectorResult.targets[0].isDead);
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
            StartCastCdCutDown();
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
    /// <summary>
    /// 通过传入装饰器名称，使用反射创建对应的SkillDecorator实例
    /// </summary>
    /// <param name="decoratorName">装饰器名称</param>
    public void GetDecorator(string decoratorName)
    {
        // Type type = Type.GetType(decoratorName);
        // SkillDecorator skillDecorator = (SkillDecorator)Activator.CreateInstance(type);
        // skillDecorators.Add(skillDecorator);
    }
    #endregion

    #region 接口实现
    public virtual void ResetSkillContext(bool isResetCount = false)
    {
        var skillData = _skillCfg;
        _skillContext.SkillTargetType = (SkillTargetType)Enum.Parse(typeof(SkillTargetType), skillData.skillTargetType);
        _skillContext.SkillRangeSelectorType = (SkillRangeSelectorType)Enum.Parse(typeof(SkillRangeSelectorType), skillData.skillRangeSelectorType);
        _skillContext.SkillTargetSelectorType = (SkillTargetSelectorType)Enum.Parse(typeof(SkillTargetSelectorType), skillData.skillTargetSelectorType);
       
        _skillContext.IntervalTime = skillData.duration / skillData.effectCounts;
        _skillContext.CurIntervalTime = 0;

        _skillContext.Duration = skillData.duration;
        _skillContext.CurDurationTime = 0;
        
        _skillContext.CurEffectCount = 0;
        _skillContext.Range = skillData.range;
        _skillContext.IsSell = !skillData.isBlockOther;
        _skillExeResult = SkillExeResult.None;
        
        if(isResetCount)
            _skillContext.CountRemain = skillData.usableCount;
    }
    
    public virtual SkillExeResult ExeState()
    {
        switch (_skillExeResult)
        {
            case SkillExeResult.None:
                if (!_isStartCd)
                {
                    _skillExeResult = SkillExeResult.Prepare;
                }
                break;
            case SkillExeResult.Prepare:
                //非脱手技能才需要判断朝向
                if (!_skillContext.IsSell && !CheckTurnToTarget())
                {
                    _skillExeResult = SkillExeResult.Ing;
                    break;
                }
                if (!IsPrepared())
                {
                    _skillExeResult = SkillExeResult.Fail;
                    break;
                }
                else
                {
                    _owner.skillController.AddUsedSkill(this);
                    Cast();
                    _skillExeResult = SkillExeResult.Ing;
                }
                break;
            case SkillExeResult.Ing:
                OnCastingUpdate();
                break;
            case SkillExeResult.ChargeEnergy:
                if(!_isStartCd)
                    _skillExeResult = SkillExeResult.Done;
                break;
            case SkillExeResult.Done:
                break;
            case SkillExeResult.Fail:
                break;
        }
        return _skillExeResult;
    }
    
    public float GetSkillCastDelay()
    {
        // 后续考虑这两个cd统一一下？
        return  _owner.attributesController.chargingDelay.GetTrueValue(_skillCfg.castDelay);
    }
    public float GetSkillChargingDelay()
    {
        return _skillCfg.chargingDelay;
    }
    
    /// <summary>
    /// 是否脱手
    /// </summary>
    /// <returns></returns>
    public bool CheckIsSellSkill()
    {
        return _skillContext.IsSell;
    }
    
    /// <summary>
    /// 判断技能是否可用
    /// </summary>
    /// <returns></returns>
    public bool IsPrepared()
    {
        //剩余使用次数（countRemain）大于零（或者等于 -1 表示无限制）且拥有者当前法力值足够支付技能消耗
        if (_skillContext.CountRemain is > 0 or -1)
        {
            if (_owner.attributesController.curMana >= _skillCfg.manaCost)
            {
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// 判断技能是否就绪
    /// </summary>
    /// <returns></returns>
    public bool HaveTargetInRange()
    {
        //判断是否成功找到目标
        return IsFindTarget();
    }
    /// <summary>
    /// 用于检查是否能找到目标，同时将查找到的目标列表保存到selectorResult中
    /// </summary>
    /// <returns></returns>
    private bool IsFindTarget()
    {
        if (_skillContext.SkillTargetType != SkillTargetType.Self)
        {
            ChampionManager manager = _targetsSelector.FindTargetsManagerByType(_skillContext.SkillTargetType, _owner.team);
            ChampionController c = _targetsSelector.FindTargetBySelectorType(_skillContext.SkillTargetSelectorType, manager, _owner, _skillCfg.distance);

            if (c == null)
            {
                return false;
            }

            _selectorResult = _targetsSelector.FindTargetByRange(c, _skillContext.SkillRangeSelectorType, _skillCfg.range, _owner.team);
            return true;
        }
        else
        {
            _selectorResult = new SelectorResult(new List<ChampionController>() { _owner }, Vector3.zero);
            return true;
        }
    }

    public SkillContext GetContext()
    {
        return _skillContext;
    }

    public void TickCd()
    {
        if (_isStartCd && _skillContext.CdCutDown > 0)
        {
            _skillContext.CdCutDown -= Time.deltaTime;
        }
        else
        {
            _isStartCd = false;
            _skillContext.CdCutDown = 0;
        }
    }
    
    public void StartSkillChainCdCutDown(float cd)
    {
        if(_isStartCd) return;
        _isStartCd = true;
        _skillContext.CdCutDown = cd;
        _skillContext.CurAllCutDown = _skillContext.CdCutDown;
    }

    public ConstructorBase GetConstructor()
    {
        return _constructor;
    }
    
    /// <summary>
    /// 根据技能目标类型，利用目标选择器查找目标
    /// </summary>
    /// <returns></returns>
    public virtual ChampionController FindAvailableTarget()
    {
        if (_skillContext.SkillTargetType != SkillTargetType.Self)
        {
            ChampionManager manager = _targetsSelector.FindTargetsManagerByType(_skillContext.SkillTargetType, _owner.team);
            ChampionController c = _targetsSelector.FindTargetBySelectorType(_skillContext.SkillTargetSelectorType, manager, _owner, 60);
            if (c == null)
            {
                return null;
            }
            SelectorResult _selectorResult = _targetsSelector.FindTargetByRange(c, _skillContext.SkillRangeSelectorType, _skillCfg.range, _owner.team);
            return _selectorResult.targets[0];
        }
        else
        {
            return _owner;
        }
    }
    #endregion
}