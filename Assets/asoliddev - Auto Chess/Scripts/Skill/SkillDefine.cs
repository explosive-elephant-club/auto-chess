using System;
using System.Collections.Generic;
using ExcelConfig;
using UnityEngine;

public interface ISkillState
{
    public void ResetSkillContext(bool isResetCount = false);
    public SkillExeResult ExeState();
    public SkillData GetSkillCfg();
    public float GetSkillCastDelay();
    public float GetSkillChargingDelay();
    public bool CheckIsSellSkill();
    public bool IsPrepared();
    public bool HaveTargetInRange();
    public SkillContext GetContext();
    public void TickCd();
    public void StartSkillChainCdCutDown(float cd);
    public ConstructorBase GetConstructor();
    public ChampionController FindAvailableTarget();
}

/// <summary>
/// 技能执行结果
/// </summary>
public enum SkillExeResult
{
    None,
    ChargeEnergy, // 充能
    Prepare, // 准备(寻敌 -> 检测蓝量)
    Ing,  // 执行中
    Done, // 结束
    Fail  // 失败
}

/// <summary>
/// 技能执行器状态
/// </summary>
public enum ProcessExeState
{
    None,
    FindTarget,  //寻找目标
    Ing,         //执行中  
    Done,        //结束
}

/// <summary>
/// 技能执行过程中的上下文(执行过程的全局数据,用于修改技能的行为)
/// </summary>
public class SkillExeContext
{
    
}

/// <summary>
/// 技能上下文(每个技能的数据)
/// </summary>
public class SkillContext
{
    /// <summary>
    /// 技能当前状态
    /// </summary>
    public SkillState State;
    /// <summary>
    /// 当前已生效(释放)次数
    /// </summary>
    public uint CurEffectCount = 0;
    /// <summary>
    /// 持续时间
    /// </summary>
    public float Duration;
    /// <summary>
    /// 技能生效间隔
    /// </summary>
    public float IntervalTime = 0;
    /// <summary>
    /// 技能当前生效间隔
    /// </summary>
    public float CurIntervalTime;
    /// <summary>
    /// 技能当前执行时间
    /// </summary>
    public float CurDurationTime;
    // 基础伤害比例
    public int DamageProportion;
    // 效果范围
    public float Range;
    // 剩余可用次数
    public int CountRemain;
    //技能是否脱手
    public bool IsSell;
    // 效果目标类型
    public SkillTargetType SkillTargetType;
    // 效果目标选中方式
    public SkillRangeSelectorType SkillRangeSelectorType;
    // 效果目标选择器类型
    public SkillTargetSelectorType SkillTargetSelectorType;
    
    public float CurAllCutDown;
    public float CdCutDown;
}

public static class SkillHelper
{
    public static Dictionary<int, System.Type> IndexToSkillType = new()
    {
        { 1, typeof(SkillBase) }
    };
    
    public static string GetVFXPath(int skillID)
    {
        return $"Prefab/Projectile/Skill/{skillID}/";
    }
}


public static class SkillFactory
{
    public static ISkillState Create(SkillData skillData, ChampionController championController, ConstructorBase constructor)
    {
        return (ISkillState)System.Activator.CreateInstance(SkillHelper.IndexToSkillType[1], skillData,
            championController, constructor);
    }

}