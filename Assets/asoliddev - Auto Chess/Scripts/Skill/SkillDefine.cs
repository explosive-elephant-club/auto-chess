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
        /// <summary>
        /// 无弹道
        /// </summary>
        None = 0,                     
        /// <summary>
        /// 波形静态弹道
        /// </summary>
        WaveformStaticTrajectory = 1,
        /// <summary>
        /// 波形动态弹道
        /// </summary>
        WaveformDynamicTrajectory = 2, 
        /// <summary>
        /// 范围弹道
        /// </summary>
        RangeTrajectory = 3,          
        /// <summary>
        /// 轨道环绕弹道
        /// </summary>
        OrbitAroundTrajectory = 4,
        /// <summary>
        /// 火箭弹道
        /// </summary>
        TrajectoryOfRocket = 5, 
        /// <summary>
        /// 激光持续弹道
        /// </summary>
        LaserSustainedTrajectory = 6,
        /// <summary>
        /// 激光瞬间弹道
        /// </summary>
        InstantaneousTrajectoryOfLaser = 7,
        /// <summary>
        /// 投掷物弹道
        /// </summary>
        TrajectoryOfProjectile = 8,
        /// <summary>
        /// 子弹散弹弹道
        /// </summary>
        TheBulletRicocheted = 9,
        /// <summary>
        /// 子弹直线弹道
        /// </summary>
        BulletLinearTrajectory = 10,
    }
    
    public enum CreateLogic
    {
        /// <summary>
        /// 枪口位置生成 朝向最近的敌人 缩放弹道和碰撞盒
        /// </summary>
        GunPosAndTurnToTarget = 0,        
    }
    
    public enum MoveLogic
    {
        /// <summary>
        /// 生成后不移动
        /// </summary>
        None = 0,
        /// <summary>
        /// 生成后移动并缩放
        /// </summary>
        MoveAndScale = 1,            
        /// <summary>
        /// 跟随自身并转向目标
        /// </summary>
        FollowSelfAndTurnToTarget = 2,
        /// <summary>
        /// 跟随自身并环绕运动
        /// </summary>
        FollowSelfAndTurnAround = 3,
        /// <summary>
        /// 向上一段距离后 寻找目标运动
        /// </summary>
        UpAndFindToTarget = 4,       
        /// <summary>
        /// 持续横扫前方扇形区域
        /// </summary>
        DurationSweep = 5,      
        /// <summary>
        /// 无弹道，只有特效
        /// </summary>
        OnlyEffect = 6,
        /// <summary>
        /// 抛物线运动，碰到地面后小幅反弹
        /// </summary>
        ParabolaAndRebound = 7,
        /// <summary>
        /// 向前直线运动
        /// </summary>
        MoveForward = 8,
    }
    
    public enum DamageLogic
    {
        /// <summary>
        /// 一次碰撞触发一次伤害 之后间隔触发伤害
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 只有碰撞触发伤害
        /// </summary>
        OnlyCollision = 1,
        /// <summary>
        /// 持续时间结束后触发一次范围伤害
        /// </summary>
        DurationAfterDamage = 2,
        /// <summary>
        /// 护盾类型
        /// </summary>
        Shield = 3,
    }
    
    public struct SkillLogicData
    {
        public MoveLogic MoveLogic;
        public DamageLogic DamageLogic;
        public bool DestroyOnColliderShield;
        /// <summary>
        /// 是否以自身中心为生成点
        /// </summary>
        public bool IsCreateInSelf;
    }

    /// <summary>
    /// 后续考虑拆到配置表里
    /// </summary>
    public static readonly Dictionary<SkillAttackType, SkillLogicData> AllLogicDataDic = new()
    {
        {
            SkillAttackType.WaveformStaticTrajectory,
            new SkillLogicData() { MoveLogic = MoveLogic.None, DamageLogic = DamageLogic.Normal }
        },
        {
            SkillAttackType.WaveformDynamicTrajectory,
            new SkillLogicData() { MoveLogic = MoveLogic.MoveAndScale, DamageLogic = DamageLogic.Normal, DestroyOnColliderShield = true }
        },
        {
            SkillAttackType.RangeTrajectory,
            new SkillLogicData() { MoveLogic = MoveLogic.FollowSelfAndTurnToTarget, DamageLogic = DamageLogic.Normal }
        },
        {
            SkillAttackType.OrbitAroundTrajectory,
            new SkillLogicData() { MoveLogic = MoveLogic.FollowSelfAndTurnAround, DamageLogic = DamageLogic.Normal, IsCreateInSelf = true, }
        },
        {
            SkillAttackType.TrajectoryOfRocket,
            new SkillLogicData() { MoveLogic = MoveLogic.UpAndFindToTarget, DamageLogic = DamageLogic.OnlyCollision }
        },
        {
            SkillAttackType.LaserSustainedTrajectory,
            new SkillLogicData() { MoveLogic = MoveLogic.DurationSweep, DamageLogic = DamageLogic.Normal }
        },
        {
            SkillAttackType.InstantaneousTrajectoryOfLaser,
            new SkillLogicData() { MoveLogic = MoveLogic.OnlyEffect, DamageLogic = DamageLogic.OnlyCollision, DestroyOnColliderShield = true }
        },
        {
            SkillAttackType.TrajectoryOfProjectile,
            new SkillLogicData() { MoveLogic = MoveLogic.ParabolaAndRebound, DamageLogic = DamageLogic.DurationAfterDamage }
        },
        {
            SkillAttackType.TheBulletRicocheted,
            new SkillLogicData() { MoveLogic = MoveLogic.MoveForward, DamageLogic = DamageLogic.OnlyCollision, DestroyOnColliderShield = true }
        },
        {
            SkillAttackType.BulletLinearTrajectory,
            new SkillLogicData() { MoveLogic = MoveLogic.MoveForward, DamageLogic = DamageLogic.OnlyCollision, DestroyOnColliderShield = true }
        },
    };

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
