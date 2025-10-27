using System;
using System.Collections.Generic;
using ExcelConfig;

public interface ISkillState
{
    public void ResetSkillContext(bool isResetCount = false);
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
    public ChampionController GetOwner();
    public void StartCastCdCutDown();
    public bool IsStartCd();
    public List<ChampionController> GetTargetList();
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
/// 技能全局数据,用于修改技能的行为,判断技能是否可释放等
/// </summary>
public class SkillContext
{
    /// <summary>
    /// 技能当前状态
    /// </summary>
    public SkillState State;
    /// <summary>
    /// 技能生效间隔
    /// </summary>
    public float IntervalTime = 0;
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
    public enum SkillAttackType
    {
        None = 0,                     //无弹道
        WaveformStaticTrajectory = 1,  //波形静态弹道
        WaveformDynamicTrajectory = 2, //波形动态弹道
        RangeTrajectory = 3,           //范围弹道
        OrbitAroundTrajectory = 4,     //轨道环绕弹道
        TrajectoryOfRocket = 5,        //火箭弹道
        LaserSustainedTrajectory = 6,  //激光持续弹道
        InstantaneousTrajectoryOfLaser = 7, //激光瞬间弹道
        TrajectoryOfProjectile = 8,    //投掷物弹道
        TheBulletRicocheted = 9,       //子弹散弹弹道
        BulletLinearTrajectory = 10,   //子弹直线弹道
    }

    public static Dictionary<int, System.Type> IndexToSkillType = new()
    {
        { 1, typeof(SkillBase) }
    };
    
    public static string GetVFXPath(int skillID)
    {
        return $"Prefab/Projectile/Skill/{skillID}/";
    }
}

public static class SkillExeExtension
{
    public static void SkillMove(this ISkillState skillState)
    {
        // skillState.GetContext().AttackType 
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
