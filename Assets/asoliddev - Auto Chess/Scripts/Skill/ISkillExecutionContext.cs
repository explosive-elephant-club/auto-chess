using System;
using UnityEngine;

/// <summary>
/// 技能执行上下文接口
/// 用于 SkillInstance 和 SkillDamageMethods 访问技能执行状态
/// </summary>
public interface ISkillExecutionContext
{
    /// <summary>
    /// 技能逻辑数据
    /// </summary>
    SkillHelper.SkillLogicData LogicData { get; }
    
    /// <summary>
    /// 攻击类型
    /// </summary>
    SkillHelper.SkillAttackType AttackType { get; }
    
    /// <summary>
    /// 技能目标类型
    /// </summary>
    SkillTargetType SkillTargetType { get; }
    
    /// <summary>
    /// 所属队伍
    /// </summary>
    ChampionTeam Team { get; }
    
    /// <summary>
    /// 获取移动速度
    /// </summary>
    float GetMoveSpeed();
    
    /// <summary>
    /// 重新获取目标
    /// </summary>
    Transform ReGetTarget();
    
    /// <summary>
    /// 设置技能是否可以结束
    /// </summary>
    void SetCanFinish(bool canFinish);
    
    /// <summary>
    /// 处理技能命中效果
    /// </summary>
    void SkillHitEffect(Collider collider, ChampionController target, bool onlyEffect = false);
}
