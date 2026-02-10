using System;
using System.Collections.Generic;
using System.ComponentModel;
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
/// 描述技能当前的状态（UI用）
/// </summary>
public enum SkillState
{
    Disable,
    Activied,
}

/// <summary>
/// 统一的技能执行阶段枚举
/// 替代原有的 SkillState、SkillExeResult、ProcessExeState
/// </summary>
public enum SkillPhase
{
    /// <summary>
    /// 空闲/未激活
    /// </summary>
    Idle,
    /// <summary>
    /// 等待CD
    /// </summary>
    WaitingCD,
    /// <summary>
    /// 寻找目标
    /// </summary>
    FindingTarget,
    /// <summary>
    /// 施法动作中
    /// </summary>
    Casting,
    /// <summary>
    /// 效果执行中（脱手后）
    /// </summary>
    Executing,
    /// <summary>
    /// 本次释放完成
    /// </summary>
    Finished,
    /// <summary>
    /// 释放失败
    /// </summary>
    Failed
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
        [Description("无弹道")]
        None = 0,                     
        [Description("波形静态弹道")]
        WaveformStaticTrajectory = 1,
        [Description("波形动态弹道")]
        WaveformDynamicTrajectory = 2, 
        [Description("范围弹道")]
        RangeTrajectory = 3,          
        [Description("轨道环绕弹道")]
        OrbitAroundTrajectory = 4,
        [Description("火箭弹道")]
        TrajectoryOfRocket = 5, 
        [Description("激光持续弹道")]
        LaserSustainedTrajectory = 6,
        [Description("激光瞬间弹道")]
        InstantaneousTrajectoryOfLaser = 7,
        [Description("投掷物弹道")]
        TrajectoryOfProjectile = 8,
        [Description("子弹散弹弹道")]
        TheBulletRicocheted = 9,
        [Description("子弹直线弹道")]
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

    
    /// <summary>
    /// 根据传入的技能类型,判断是伤害后消失(结束) 还是持续时间结束消失(结束)
    /// true => 伤害后结束
    /// false => 持续时间结束结束
    /// </summary>
    /// <returns></returns>
    public static bool CheckIsDamageDestroySkill(SkillAttackType type)
    {
        //测试代码
        // return true;
        return type is SkillAttackType.TheBulletRicocheted or SkillAttackType.TrajectoryOfRocket
            or SkillAttackType.BulletLinearTrajectory;
    }
    
    /// <summary>
    /// 根据传入的技能类型,判断是否需要持续施法
    /// true => 需要持续施法 false => 不需要持续施法
    /// </summary>
    /// <returns></returns>
    public static bool CheckIsNeedContinuousCasting(SkillAttackType type)
    {
        return false;
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
