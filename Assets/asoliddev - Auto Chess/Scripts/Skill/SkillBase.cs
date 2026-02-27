using System;
using System.Collections.Generic;
using ExcelConfig;
using UnityEngine;

public class SkillBase : ISkillState
{
    private readonly SkillData _skillCfg;
    private readonly SkillContext _skillContext;
    private ConstructorBase _constructor;
    #region 参数
    /// <summary>
    /// 技能的拥有者
    /// </summary>
    private readonly ChampionController _owner;
    /// <summary>
    /// 统一目标查找器（替代原有的 SkillTargetsSelector）
    /// </summary>
    private readonly SkillTargetFinder _targetFinder;
    /// <summary>
    /// 运行时数据（用于目标查找）
    /// </summary>
    private readonly SkillRuntime _runtime;
    /// <summary>
    /// 被选中的目标结果
    /// </summary>
    private TargetResult _targetResult;
    private bool _isStartCd;
    #endregion
    
    public SkillBase(SkillData skillData, ChampionController championController, ConstructorBase constructor)
    {
        _skillCfg = skillData;
        _skillContext = new SkillContext();
        _targetFinder = new SkillTargetFinder();
        _runtime = new SkillRuntime();
        _targetResult = new TargetResult();
        _constructor = constructor;
        _owner = championController;
    }

    public SkillData GetSkillCfg()
    {
        return _skillCfg;
    }

    #region 接口实现
    public virtual void ResetSkillContext(bool isResetCount = false)
    {
        var skillData = _skillCfg;
        _skillContext.SkillTargetType = (SkillTargetType)Enum.Parse(typeof(SkillTargetType), skillData.skillTargetType);
        _skillContext.SkillRangeSelectorType = (SkillRangeSelectorType)Enum.Parse(typeof(SkillRangeSelectorType), skillData.skillRangeSelectorType);
        _skillContext.SkillTargetSelectorType = (SkillTargetSelectorType)Enum.Parse(typeof(SkillTargetSelectorType), skillData.skillTargetSelectorType);
       
        _skillContext.IntervalTime = skillData.duration / skillData.effectCounts;
        _skillContext.Range = skillData.range;
        _skillContext.IsSell = !skillData.isBlockOther;
        
        if(isResetCount)
            _skillContext.CountRemain = skillData.usableCount;
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
        // 使用统一的目标查找器
        return FindAndCacheTargets();
    }
    
    /// <summary>
    /// 用于检查是否能找到目标，同时将查找到的目标列表缓存
    /// 使用新的 SkillTargetFinder 统一目标查找逻辑
    /// </summary>
    /// <returns></returns>
    private bool FindAndCacheTargets()
    {
        // 确保运行时数据已初始化（用于目标查找）
        EnsureRuntimeInitialized();
        
        // 使用统一的目标查找器
        _targetResult = _targetFinder.FindTargets(_runtime, false);
        return _targetResult.HasValidTarget;
    }
    
    /// <summary>
    /// 确保运行时数据已初始化
    /// </summary>
    private void EnsureRuntimeInitialized()
    {
        if (_runtime.SkillState == null)
        {
            _runtime.Initialize(this);
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
    /// 使用统一的 SkillTargetFinder
    /// </summary>
    /// <returns></returns>
    public virtual ChampionController FindAvailableTarget()
    {
        EnsureRuntimeInitialized();
        return _targetFinder.FindAvailableTarget(_runtime, true);
    }

    public virtual ChampionController GetOwner()
    {
        return _owner;
    }
    /// <summary>
    /// 这个技能释放后的充能时间,充能结束之后才会结束技能的释放,但如果是在技能链的最后一个技能(最后非null节点),则不需要这个充能时间
    /// </summary>
    public void StartCastCdCutDown()
    {
        if(_isStartCd) return;
        _isStartCd = true;
        _skillContext.CdCutDown = _owner.attributesController.castDelay.GetTrueValue(GetSkillCastDelay());
        _skillContext.CurAllCutDown = _skillContext.CdCutDown;
    }

    public bool IsStartCd()
    {
        return _isStartCd;
    }

    public List<ChampionController> GetTargetList()
    {
        return _targetResult?.Targets;
    }
    
    /// <summary>
    /// 获取技能运行时数据（用于新的技能系统）
    /// </summary>
    public SkillRuntime GetRuntime()
    {
        EnsureRuntimeInitialized();
        return _runtime;
    }

    /// <summary>
    /// 当前技能是否可被中断（默认可中断，可在子类覆盖）
    /// </summary>
    public virtual bool CanBeInterrupted => true;

    /// <summary>
    /// 中断当前技能
    /// </summary>
    /// <param name="source">中断来源（如控制效果ID）</param>
    /// <returns>是否成功中断</returns>
    public virtual bool Interrupt(int source = 0)
    {
        if (!CanBeInterrupted)
            return false;

        _runtime.CurrentPhase = SkillPhase.Interrupted;
        return true;
    }
    #endregion
}