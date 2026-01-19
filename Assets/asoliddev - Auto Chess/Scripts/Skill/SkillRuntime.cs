using System.Collections.Generic;
using ExcelConfig;
using UnityEngine;

/// <summary>
/// 技能运行时数据
/// 包含技能执行过程中需要的所有运行时信息
/// </summary>
public class SkillRuntime
{
    #region 基础引用
    /// <summary>
    /// 技能状态接口
    /// </summary>
    public ISkillState SkillState { get; private set; }
    
    /// <summary>
    /// 技能配置数据
    /// </summary>
    public SkillData SkillData { get; private set; }
    
    /// <summary>
    /// 技能拥有者
    /// </summary>
    public ChampionController Owner { get; private set; }
    
    /// <summary>
    /// 技能所属部件
    /// </summary>
    public ConstructorBase Constructor { get; private set; }
    
    /// <summary>
    /// 技能上下文（UI用）
    /// </summary>
    public SkillContext Context { get; private set; }
    #endregion

    #region 目标相关
    /// <summary>
    /// 技能目标类型
    /// </summary>
    public SkillTargetType SkillTargetType => Context.SkillTargetType;
    
    /// <summary>
    /// 技能范围选择器类型
    /// </summary>
    public SkillRangeSelectorType SkillRangeSelectorType => Context.SkillRangeSelectorType;
    
    /// <summary>
    /// 技能目标选择器类型
    /// </summary>
    public SkillTargetSelectorType SkillTargetSelectorType => Context.SkillTargetSelectorType;
    
    /// <summary>
    /// 技能射程
    /// </summary>
    public int Distance => SkillData.distance;
    
    /// <summary>
    /// 技能范围
    /// </summary>
    public int Range => SkillData.range;
    
    /// <summary>
    /// 获取当前目标列表（从 SkillState 获取）
    /// </summary>
    public List<ChampionController> GetTargetList() => SkillState?.GetTargetList();
    
    /// <summary>
    /// 主目标
    /// </summary>
    public ChampionController PrimaryTarget
    {
        get
        {
            var list = SkillState?.GetTargetList();
            return list != null && list.Count > 0 ? list[0] : null;
        }
    }
    #endregion

    #region 执行状态
    /// <summary>
    /// 当前技能阶段
    /// </summary>
    public SkillPhase CurrentPhase { get; set; } = SkillPhase.Idle;
    
    /// <summary>
    /// 当前持续时间
    /// </summary>
    public float CurrentDuration { get; set; }
    
    /// <summary>
    /// 当前生效次数
    /// </summary>
    public uint CurrentEffectCount { get; set; }
    
    /// <summary>
    /// 当前生效间隔计时
    /// </summary>
    public float CurrentIntervalTime { get; set; }
    
    /// <summary>
    /// 当前发射点索引
    /// </summary>
    public int CurrentCastPointIndex { get; set; }
    
    /// <summary>
    /// 技能是否已进入CD
    /// </summary>
    public bool HasStartedCD { get; set; }
    
    /// <summary>
    /// 技能是否可以结束（用于伤害后消失类技能）
    /// </summary>
    public bool CanFinish { get; set; }
    
    /// <summary>
    /// 未完成计数器（用于多弹道技能）
    /// </summary>
    public int PendingCounter { get; set; }
    #endregion

    #region 伤害相关
    /// <summary>
    /// 总命中次数限制
    /// </summary>
    public int TotalHitCount { get; set; } = 1;
    
    /// <summary>
    /// 当前命中次数
    /// </summary>
    public int CurrentHitCount { get; set; }
    #endregion

    #region 技能实例
    /// <summary>
    /// 已生成的技能实例列表
    /// </summary>
    public List<SkillInstance> EffectInstances { get; } = new();
    #endregion

    #region 技能逻辑数据
    /// <summary>
    /// 攻击类型
    /// </summary>
    public SkillHelper.SkillAttackType AttackType { get; private set; }
    
    /// <summary>
    /// 逻辑数据
    /// </summary>
    public SkillHelper.SkillLogicData LogicData { get; private set; }
    
    /// <summary>
    /// 是否为伤害后消失类技能
    /// </summary>
    public bool IsDamageDestroySkill => SkillHelper.CheckIsDamageDestroySkill(AttackType);
    
    /// <summary>
    /// 是否需要持续施法
    /// </summary>
    public bool NeedsContinuousCasting => SkillHelper.CheckIsNeedContinuousCasting(AttackType);
    #endregion

    /// <summary>
    /// 初始化运行时数据
    /// </summary>
    public void Initialize(ISkillState skillState)
    {
        SkillState = skillState;
        SkillData = skillState.GetSkillCfg();
        Owner = skillState.GetOwner();
        Constructor = skillState.GetConstructor();
        Context = skillState.GetContext();
        
        AttackType = (SkillHelper.SkillAttackType)SkillData.AttackType;
        LogicData = SkillHelper.AllLogicDataDic[AttackType];
        
        Reset();
    }

    /// <summary>
    /// 重置运行时状态
    /// </summary>
    public void Reset()
    {
        CurrentPhase = SkillPhase.Idle;
        CurrentDuration = 0;
        CurrentEffectCount = 0;
        CurrentIntervalTime = 0;
        CurrentCastPointIndex = 0;
        HasStartedCD = false;
        CanFinish = false;
        PendingCounter = 0;
        TotalHitCount = 1;
        CurrentHitCount = 0;
        EffectInstances.Clear();
    }

    /// <summary>
    /// 更新持续时间
    /// </summary>
    public void UpdateDuration(float deltaTime)
    {
        CurrentDuration += deltaTime;
    }

    /// <summary>
    /// 获取当前发射点
    /// </summary>
    public Transform GetCurrentCastPoint()
    {
        if (Constructor.skillCastPoints == null || Constructor.skillCastPoints.Length == 0)
            return Owner.transform;
            
        return Constructor.skillCastPoints[CurrentCastPointIndex];
    }

    /// <summary>
    /// 移动到下一个发射点
    /// </summary>
    public void MoveToNextCastPoint()
    {
        if (Constructor.skillCastPoints != null && Constructor.skillCastPoints.Length > 0)
        {
            CurrentCastPointIndex = (CurrentCastPointIndex + 1) % Constructor.skillCastPoints.Length;
        }
    }

    /// <summary>
    /// 设置技能是否可以结束
    /// </summary>
    public void SetCanFinish(bool canFinish)
    {
        if (canFinish)
        {
            PendingCounter--;
        }
        else
        {
            PendingCounter++;
        }
        CanFinish = PendingCounter <= 0;
    }

    /// <summary>
    /// 检查技能是否应该结束
    /// </summary>
    public bool ShouldFinish()
    {
        if (IsDamageDestroySkill)
        {
            return CanFinish;
        }
        else
        {
            var targetList = GetTargetList();
            return targetList != null && targetList.Count > 0 && CurrentDuration >= SkillData.duration + SkillData.delay;
        }
    }

    /// <summary>
    /// 检查生效次数是否已达上限
    /// </summary>
    public bool IsEffectCountComplete()
    {
        return CurrentEffectCount >= SkillData.effectCounts;
    }

    /// <summary>
    /// 获取移动速度
    /// </summary>
    public float GetMoveSpeed()
    {
        return SkillData.MoveSpeed;
    }
}
