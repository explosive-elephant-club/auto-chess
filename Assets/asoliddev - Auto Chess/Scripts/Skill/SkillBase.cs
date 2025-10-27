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
    /// 目标选择器
    /// </summary>
    private readonly SkillTargetsSelector _targetsSelector;
    /// <summary>
    /// 被选中的目标
    /// </summary>
    private SelectorResult _selectorResult;
    private bool _isStartCd;
    #endregion
    
    public SkillBase(SkillData skillData, ChampionController championController, ConstructorBase constructor)
    {
        _skillCfg = skillData;
        _skillContext = new SkillContext();
        _targetsSelector = new SkillTargetsSelector();  
        //目标选择器加载
        _selectorResult = new SelectorResult();
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
        return _selectorResult?.targets;
    }
    #endregion
}