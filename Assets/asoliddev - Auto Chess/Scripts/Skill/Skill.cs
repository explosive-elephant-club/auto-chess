using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;



public enum SkillTargetType//目标类型
{
    [Tooltip("自身")]
    Self,
    [Tooltip("队友")]
    Teammate,
    [Tooltip("敌人")]
    Enemy
}
public enum SkillRangeSelectorType//范围选择类型
{
    [Tooltip("范围内所有友军")]
    TeammatesInRange,
    [Tooltip("范围内所有敌人")]
    EnemiesInRange,
    [Tooltip("范围内所有棋格")]
    MapHexInRange,
}

public enum SkillTargetSelectorType//目标选择类型
{
    [Tooltip("任一")]
    Any,
    [Tooltip("最近的")]
    Nearest,
    [Tooltip("最远的")]
    Farthest,
    [Tooltip("伤害最高的")]
    HighestDPS,
    [Tooltip("等级最高的")]
    HighestLevel,
    [Tooltip("生命值最多的")]
    MostHP,
    [Tooltip("生命值最少的")]
    LeastHP,
    [Tooltip("周围友军最多的")]
    MostTeammatesSurrounded,
    [Tooltip("周围敌人最多的")]
    MostEnemiesSurrounded,
}


public class Skill
{
    public SkillData skillData;

    public SkillTargetType skillTargetType;
    public SkillRangeSelectorType skillRangeSelectorType;
    public SkillTargetSelectorType skillTargetSelectorType;

    public ChampionController owner;//技能的拥有者
    public ChampionController caster;//技能的施加者

    public float cdRemain;

    public GameObject effectPrefab;
    public GameObject hitFXPrefab;
    public Sprite icon;

    public SkillBehaviour skillBehaviour;
    public GameObject[] targets;
}
