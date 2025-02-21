using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 技能投射物 (不一定是投射出去的物体,是技能生成的实例物体)
/// </summary>
public class SkillEffect : MonoBehaviour
{
    /// <summary>
    /// 技能对象
    /// </summary>
    protected Skill skill;
    /// <summary>
    /// 持续时间
    /// </summary>
    protected float duration;
    /// <summary>
    /// 已存在时间
    /// </summary>
    protected float curTime;
    /// <summary>
    /// 技能目标
    /// </summary>
    protected Transform target;
    /// <summary>
    /// 技能销毁时的延迟
    /// </summary>
    public float destroyDelay = 0;
    /// <summary>
    /// 已命中的单位列表，避免重复触发效果
    /// </summary>
    public List<ChampionController> hits;

    /// <summary>
    /// 命中特效实例
    /// </summary>
    public GameObject hitParticleInstance;
    /// <summary>
    /// 发射特效实例
    /// </summary>
    public GameObject emitParticleInstance;
    public virtual void Init(Skill _skill, Transform _target)
    {
        skill = _skill;
        target = _target;
        curTime = 0;
        duration = skill.skillData.duration;
        hits = new List<ChampionController>();
    }

    protected virtual void FixedUpdate()
    {
        curTime += Time.fixedDeltaTime;
    }
    /// <summary>
    /// 技能投射物销毁
    /// </summary>
    public virtual void DestroySelf()
    {
        if (hitParticleInstance != null)
            Destroy(hitParticleInstance);
        if (emitParticleInstance != null)
            Destroy(emitParticleInstance);
        Destroy(gameObject, destroyDelay);
    }
    /// <summary>
    /// 生成发射粒子特效
    /// </summary>
    protected virtual void InstantiateEmitEffect()
    {
        emitParticleInstance = skill.InstantiateEmitInstance(transform.position, transform.rotation, 1.5f);
    }
    /// <summary>
    /// 生成命中粒子特效
    /// </summary>
    /// <param name="pos"></param>
    protected virtual void InstantiateHitEffect(Vector3 pos)
    {
        hitParticleInstance = skill.InstantiateHitInstance(pos, Quaternion.FromToRotation(Vector3.up, Vector3.zero), 1.5f);
    }
    /// <summary>
    /// 让投射物朝向目标
    /// </summary>
    /// <param name="targetPos">目标位置</param>
    protected virtual void PointedAtTarget(Vector3 targetPos)
    {
        Vector3 relativePos = targetPos - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        this.transform.rotation = rotation;
    }
    /// <summary>
    /// 碰撞检测开始
    /// </summary>
    /// <param name="hit">碰撞对象</param>
    protected virtual void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "SkillEffectCol")
            return;
        //如果碰撞到护盾，调用 OnCollideShieldBegin(hit) 并中止技能
        if (hit.tag == "Shield")
        {
            InterceptShieldEffect shieldEffect = hit.GetComponentInParent<InterceptShieldEffect>();
            if (shieldEffect.skill.owner.team != skill.owner.team)
            {
                OnCollideShieldBegin(hit);
                return;
            }
        }
        ChampionController c = hit.gameObject.GetComponentInParent<ChampionController>();
        if (c == null)
            return;
        //如果碰撞到敌人/队友，则调用 OnCollideChampionBegin(c, pos)
        if (skill.skillTargetType == SkillTargetType.Teammate)
        {
            if (c.team == skill.owner.team)
            {
                OnCollideChampionBegin(c, hit.bounds.ClosestPoint(transform.position));
            }
        }
        else if (skill.skillTargetType == SkillTargetType.Enemy)
        {
            if (c.team != skill.owner.team)
            {
                OnCollideChampionBegin(c, hit.bounds.ClosestPoint(transform.position));
            }
        }
    }
    /// <summary>
    /// 碰撞检测结束
    /// </summary>
    /// <param name="hit">碰撞对象</param>
    protected virtual void OnTriggerExit(Collider hit)
    {
        ChampionController c = hit.gameObject.GetComponentInParent<ChampionController>();
        if (c == null)
            return;
        //如果碰撞离开敌人/队友，则调用 OnCollideChampionEnd(c, pos)
        if (skill.skillTargetType == SkillTargetType.Teammate)
        {
            if (c.team == skill.owner.team)
            {
                OnCollideChampionEnd(c, hit.bounds.ClosestPoint(transform.position));
            }
        }
        else if (skill.skillTargetType == SkillTargetType.Enemy)
        {
            if (c.team != skill.owner.team)
            {
                OnCollideChampionEnd(c, hit.bounds.ClosestPoint(transform.position));
            }
        }
    }
    /// <summary>
    /// 技能被护盾拦截时
    /// </summary>
    /// <param name="hit">护盾碰撞体</param>
    protected virtual void OnCollideShieldBegin(Collider hit)
    {
        InstantiateHitEffect(hit.bounds.ClosestPoint(transform.position));
        InterceptShieldEffect shieldEffect = hit.GetComponent<InterceptShieldEffect>();
        shieldEffect.OnGotHit(skill.owner, skill.skillData.damageData);
        Destroy(gameObject, destroyDelay);
    }
    /// <summary>
    /// 技能碰撞到目标时
    /// </summary>
    /// <param name="c">碰撞目标</param>
    /// <param name="colPos">碰撞点</param>
    protected virtual void OnCollideChampionBegin(ChampionController c, Vector3 colPos)
    {
        if (!hits.Contains(c))
        {
            hits.Add(c);
            InstantiateHitEffect(colPos);
        }

    }
    /// <summary>
    /// 目标离开技能碰撞范围时
    /// </summary>
    /// <param name="c">碰撞目标</param>
    /// <param name="colPos">碰撞点</param>
    protected virtual void OnCollideChampionEnd(ChampionController c, Vector3 colPos)
    {
        //从hits列表移除该单位
        if (hits.Contains(c))
        {
            hits.Remove(c);
        }
    }

    /// <summary>
    /// 使用查找技能选择范围内的目标
    /// </summary>
    /// <param name="c">选中的目标</param>
    /// <returns>范围内的目标</returns>
    protected virtual List<ChampionController> GetTargetsInRange(ChampionController c)
    {
        return skill.targetsSelector.FindTargetByRange(c, skill.skillRangeSelectorType, skill.skillData.range, skill.owner.team).targets;
    }
    
    /// <summary>
    /// 通过变量的名称在配置数据中获取变量的值
    /// </summary>
    /// <param name="name">变量的名称</param>
    /// <returns>变量的值</returns>
    public string GetParam(string name)
    {
        foreach (var p in skill.skillData.paramValues)
        {
            if (p.name == name)
            {
                return p.value;
            }
        }
        return null;
    }
}
