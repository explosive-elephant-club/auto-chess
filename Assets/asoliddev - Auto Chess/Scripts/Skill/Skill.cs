using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ExcelConfig;
using System.Linq;
using UnityEngine.Events;

/// <summary>
/// 描述技能当前的状态
/// </summary>
public enum SkillState
{
    Disable,
    Casting,
    CD
}
/// <summary>
/// 技能的全部数据和行为
/// </summary>
public class Skill
{
    /// <summary>
    /// 技能的配置数据
    /// </summary>
    public SkillData skillData;
    /// <summary>
    /// 技能持续时间的计时器
    /// </summary>
    public float curTime = 0;
    /// <summary>
    /// 生效间隔
    /// </summary>
    public float intervalTime = 0;
    /// <summary>
    /// 生效间隔的计时器
    /// </summary>
    public float curIntervalTime = 0;
    /// <summary>
    /// 生效的计数器
    /// </summary>
    public float curEffectCount = 0;

    public SkillTargetType skillTargetType;
    public SkillRangeSelectorType skillRangeSelectorType;
    public SkillTargetSelectorType skillTargetSelectorType;
    /// <summary>
    /// 技能的拥有者
    /// </summary>
    public ChampionController owner;
    /// <summary>
    /// 技能的拥有者的管理器
    /// </summary>
    public ChampionManager manager;
    /// <summary>
    /// 技能的来源部件
    /// </summary>
    public ConstructorBase constructor;
    /// <summary>
    /// 技能释放次数的计数器
    /// </summary>
    public int countRemain;

    public string VFXPath;
    /// <summary>
    /// 发射特效预制体
    /// </summary>
    public GameObject emitPrefab;
    /// <summary>
    /// 投射物预制体
    /// </summary>
    public GameObject effectPrefab;
    /// <summary>
    /// 击中特效预制体
    /// </summary>
    public GameObject hitFXPrefab;
    public Sprite icon;
    /// <summary>
    /// 目标选择器
    /// </summary>
    public SkillTargetsSelector targetsSelector;
    /// <summary>
    /// 被选中的目标
    /// </summary>
    public SelectorResult selectorResult;
    /// <summary>
    /// 技能的状态
    /// </summary>
    public SkillState state;
    /// <summary>
    /// 技能实例化的所有特效
    /// </summary>
    public List<SkillEffect> effectInstances = new List<SkillEffect>();
    /// <summary>
    /// 技能管理器
    /// </summary>
    public SkillController skillController;
    /// <summary>
    /// 技能修饰器
    /// </summary>
    public List<SkillDecorator> skillDecorators = new List<SkillDecorator>();


    /// <summary>
    /// 目前技能特效生成点
    /// </summary>
    public int curCastPointIndex;

    //一系列函数委托 使得后续能够用修饰器统一修改相应操作
    #region 
    public Func<bool> IsFindTargetFunc;
    public Action CastFunc;
    public Action EffectFunc;
    public Action DirectEffectFunc;
    public Action InstanceEffectFunc;
    public Action<ChampionController> AddBuffToTargetFunc;
    public Action<ChampionController> AddDMGToTargetFunc;
    public Action OnCastingUpdateFunc;
    public Func<bool> IsFinishFunc;
    public Action DestroyEffectFunc;
    public Action OnFinishFunc;
    public Action ResetFunc;
    public Action PlayCastAnimFunc;
    public Action PlayEndAnimFunc;
    #endregion

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="_skillData">技能配置数据</param>
    /// <param name="_owner">拥有单位</param>
    /// <param name="_constructor">来源部件</param>
    public void Init(SkillData _skillData, ChampionController _owner, ConstructorBase _constructor)
    {
        skillData = _skillData;


        skillTargetType = (SkillTargetType)Enum.Parse(typeof(SkillTargetType), skillData.skillTargetType);
        skillRangeSelectorType = (SkillRangeSelectorType)Enum.Parse(typeof(SkillRangeSelectorType), skillData.skillRangeSelectorType);
        skillTargetSelectorType = (SkillTargetSelectorType)Enum.Parse(typeof(SkillTargetSelectorType), skillData.skillTargetSelectorType);

        owner = _owner;
        manager = owner.championManeger;
        constructor = _constructor;
        //根据技能持续时间和效果触发次数计算出intervalTime，初始化剩余使用次数countRemain（-1 表示无限制）。
        intervalTime = skillData.duration / skillData.effectCounts;
        countRemain = skillData.usableCount;
        curEffectCount = 0;
        curCastPointIndex = 0;
        //目标选择器加载
        selectorResult = new SelectorResult();
        targetsSelector = new SkillTargetsSelector();
        //特效加载
        VFXPath = "Prefab/Projectile/Skill/" + skillData.ID + "/";
        if (skillData.emitFXPrefab)
            emitPrefab = Resources.Load<GameObject>(VFXPath + "Emit");
        if (skillData.effectPrefab)
        {
            effectPrefab = Resources.Load<GameObject>(VFXPath + "Effect");
        }
        if (skillData.hitFXPrefab)
            hitFXPrefab = Resources.Load<GameObject>(VFXPath + "Hit");

        icon = Resources.Load<Sprite>(skillData.icon);

        state = SkillState.Disable;
        skillController = owner.skillController;
        //调用GetDecorator方法通过反射创建装饰器实例
        if (!string.IsNullOrEmpty(skillData.skillDecorators[0]))
        {
            foreach (var d in skillData.skillDecorators)
            {
                GetDecorator(d);
            }
        }

        BindFunc();
    }

    //绑定函数委托
    public void BindFunc()
    {
        IsFindTargetFunc = IsFindTarget;

        CastFunc = Cast;
        EffectFunc = Effect;
        DirectEffectFunc = DirectEffect;
        InstanceEffectFunc = InstanceEffect;
        AddBuffToTargetFunc = AddBuffToTarget;
        AddDMGToTargetFunc = AddDMGToTarget;
        OnCastingUpdateFunc = OnCastingUpdate;
        IsFinishFunc = IsFinish;
        DestroyEffectFunc = DestroyEffect;
        OnFinishFunc = OnFinish;
        ResetFunc = Reset;
        PlayCastAnimFunc = PlayCastAnim;
        PlayEndAnimFunc = PlayEndAnim;
    }
    /// <summary>
    /// 判断技能是否可用
    /// </summary>
    /// <returns></returns>
    public bool IsAvailable()
    {
        //剩余使用次数（countRemain）大于零（或者等于 -1 表示无限制）且拥有者当前法力值足够支付技能消耗
        if (countRemain > 0 || countRemain == -1)
        {
            if (owner.attributesController.curMana >= skillData.manaCost)
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
    public bool IsPrepared()
    {
        //判断是否成功找到目标
        if (IsAvailable())
            return IsFindTargetFunc();
        return false;
    }
    /// <summary>
    /// 根据技能目标类型，利用目标选择器查找目标
    /// </summary>
    /// <returns></returns>
    public virtual ChampionController FindAvailableTarget()
    {
        if (skillTargetType != SkillTargetType.Self)
        {
            ChampionManager manager = targetsSelector.FindTargetsManagerByType(skillTargetType, owner.team);
            ChampionController c = targetsSelector.FindTargetBySelectorType(skillTargetSelectorType, manager, owner, 60);
            if (c == null)
            {
                return null;
            }
            SelectorResult _selectorResult = targetsSelector.FindTargetByRange(c, skillRangeSelectorType, skillData.range, owner.team);
            return _selectorResult.targets[0];
        }
        else
        {
            return owner;
        }
    }
    /// <summary>
    /// 用于检查是否能找到目标，同时将查找到的目标列表保存到selectorResult中
    /// </summary>
    /// <returns></returns>
    public virtual bool IsFindTarget()
    {
        if (skillTargetType != SkillTargetType.Self)
        {
            ChampionManager manager = targetsSelector.FindTargetsManagerByType(skillTargetType, owner.team);
            ChampionController c = targetsSelector.FindTargetBySelectorType(skillTargetSelectorType, manager, owner, skillData.distance);

            if (c == null)
            {
                return false;
            }

            selectorResult = targetsSelector.FindTargetByRange(c, skillRangeSelectorType, skillData.range, owner.team);
            return true;
        }
        else
        {
            selectorResult = new SelectorResult(new List<ChampionController>() { owner }, Vector3.zero);
            return true;
        }
    }
    /// <summary>
    /// 技能释放
    /// </summary>
    public virtual void Cast()
    {
        state = SkillState.Casting;
        owner.buffController.eventCenter.Broadcast(BuffActiveMode.BeforeCast.ToString());
        //重置计时器和计数器
        curTime = 0;
        curIntervalTime = skillData.delay;
        curEffectCount = 0;
        //扣除施放技能所需的法力值，更新剩余使用次数
        owner.attributesController.curMana -= skillData.manaCost;
        if (countRemain != -1)
            countRemain -= 1;
        //播放施放动画
        PlayCastAnimFunc();

        owner.buffController.eventCenter.Broadcast(BuffActiveMode.AfterCast.ToString());
    }
    /// <summary>
    /// 获取当前技能施放点
    /// </summary>
    /// <returns></returns>
    public Transform GetCastPoint()
    {
        return constructor.skillCastPoints[curCastPointIndex];
    }
    /// <summary>
    /// 技能生效
    /// </summary>
    public virtual void Effect()
    {
        //直接生效
        if (skillData.isDirectEffect)
        {
            DirectEffectFunc();
        }
        else  //生成技能特效弹道
        {
            InstanceEffectFunc();
        }
    }
    /// <summary>
    /// 直接生效 对目标直接造成伤害并施加buff
    /// </summary>
    public virtual void DirectEffect()
    {
        if (selectorResult.targets.Count == 0)
            return;
        if (selectorResult.targets != null)
        {
            InstantiateEmitInstance(GetCastPoint().position, GetCastPoint().rotation, 1.5f);
            curCastPointIndex = (curCastPointIndex + 1) % constructor.skillCastPoints.Length;
            foreach (ChampionController C in selectorResult.targets)
            {
                if (!C.isDead)
                {
                    InstantiateHitInstance(C.transform.position, Quaternion.FromToRotation(Vector3.up, Vector3.zero), 1.5f);
                    AddBuffToTargetFunc(C);
                    AddDMGToTargetFunc(C);
                }
            }
        }
    }
    /// <summary>
    /// 创建技能投射物特效实例
    /// </summary>
    public virtual void InstanceEffect()
    {
        GameObject obj = GameObject.Instantiate(effectPrefab);
        //obj.transform.parent = GetCastPoint();
        obj.transform.position = GetCastPoint().position;
        obj.transform.rotation = GetCastPoint().rotation;

        curCastPointIndex = (curCastPointIndex + 1) % constructor.skillCastPoints.Length;

        SkillEffect skillEffect = obj.GetComponent<SkillEffect>();
        skillEffect.Init(this, selectorResult.targets[0].transform);
        effectInstances.Add(skillEffect);
    }

    /// <summary>
    /// 对目标施加buff
    /// </summary>
    /// <param name="target">目标</param>
    public virtual void AddBuffToTarget(ChampionController target)
    {
        foreach (int buff_ID in skillData.addBuffs)
        {
            if (buff_ID != 0)
                target.buffController.AddBuff(buff_ID, owner);
        }
    }
    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    /// <param name="target">目标</param>
    public virtual void AddDMGToTarget(ChampionController target)
    {
        if (!string.IsNullOrEmpty(skillData.damageData[0].type))
        {
            owner.championCombatController.TakeDamage(target, skillData.damageData);
        }
    }
    /// <summary>
    /// 技能持续施法时Update
    /// </summary>
    public virtual void OnCastingUpdate()
    {
        curTime += Time.deltaTime;
        curIntervalTime -= Time.deltaTime;

        if (IsFinishFunc())
        {
            OnFinishFunc();
        }

        if (curIntervalTime <= 0 && curEffectCount < skillData.effectCounts)
        {
            curIntervalTime = intervalTime;
            curEffectCount++;
            EffectFunc();
        }
    }
    /// <summary>
    /// 根据技能总持续时间或目标状态（例如目标死亡）判断是否结束技能施放
    /// </summary>
    /// <returns></returns>
    public virtual bool IsFinish()
    {
        return (curTime >= skillData.duration + skillData.delay) || (selectorResult.targets[0] != null ? selectorResult.targets[0].isDead : false);
    }
    /// <summary>
    /// 销毁所有已生成的技能特效实例
    /// </summary>
    public virtual void DestroyEffect()
    {
        if (effectInstances.Count > 0)
        {
            foreach (var e in effectInstances)
            {
                if (e != null)
                    e.DestroySelf();
            }
            effectInstances.Clear();
        }
    }
    /// <summary>
    /// 技能结束
    /// </summary>
    public virtual void OnFinish()
    {
        state = SkillState.CD;
        PlayEndAnimFunc();
    }
    /// <summary>
    /// 技能重置
    /// </summary>
    public virtual void Reset()
    {
        state = SkillState.CD;
        curTime = 0;
        curIntervalTime = 0;
        curEffectCount = 0;
        selectorResult.Clear();
        countRemain = skillData.usableCount;
    }
    /// <summary>
    /// 播放释放动画
    /// </summary>
    public virtual void PlayCastAnim()
    {

        if (!string.IsNullOrEmpty(skillData.skillAnimTrigger[0].constructorType))
        {
            //触发动画
            foreach (var animTrigger in skillData.skillAnimTrigger)
            {
                foreach (var c in constructor.GetAllParentConstructors(true))
                {
                    if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                    {
                        c.enablePlayNewSkillAnim = false;
                        c.animator.SetTrigger(animTrigger.trigger);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 播放持续释放结束动画
    /// </summary>
    public virtual void PlayEndAnim()
    {
        //绑定动画结束时间
        if (!string.IsNullOrEmpty(skillData.skillAnimTrigger[0].constructorType))
        {
            //触发动画
            foreach (var animTrigger in skillData.skillAnimTrigger)
            {
                foreach (var c in constructor.GetAllParentConstructors(true))
                {
                    if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                    {
                        if (HasParameter(c.animator, "EndCasting"))
                            c.animator.SetTrigger("EndCasting");
                    }
                }
            }
        }
    }
    /// <summary>
    /// 实例化发射特效
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="rot">旋转</param>
    /// <param name="duration">持续时间</param>
    /// <returns></returns>
    public virtual GameObject InstantiateEmitInstance(Vector3 pos, Quaternion rot, float duration)
    {
        if (emitPrefab != null)
        {
            GameObject emitParticleInstance = GameObject.Instantiate(emitPrefab, pos, rot) as GameObject;
            GameObject.Destroy(emitParticleInstance, duration);
            return emitParticleInstance;
        }
        return null;
    }
    /// <summary>
    /// 实例化命中特效
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="rot">旋转</param>
    /// <param name="duration">持续时间</param>
    /// <returns></returns>
    public virtual GameObject InstantiateHitInstance(Vector3 pos, Quaternion rot, float duration)
    {
        if (hitFXPrefab != null)
        {
            GameObject hitParticleInstance = GameObject.Instantiate(hitFXPrefab, pos, rot) as GameObject;
            GameObject.Destroy(hitParticleInstance, duration);
            return hitParticleInstance;
        }
        return null;
    }
    /// <summary>
    /// 判断动画状态机中是否存在某个指定参数名
    /// </summary>
    /// <param name="animator">动画状态机</param>
    /// <param name="parameterName">参数名</param>
    /// <returns></returns>
    protected bool HasParameter(Animator animator, string parameterName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == parameterName)
                return true;
        }

        return false;
    }
    /// <summary>
    /// 通过传入装饰器名称，使用反射创建对应的SkillDecorator实例
    /// </summary>
    /// <param name="decoratorName">装饰器名称</param>
    public void GetDecorator(string decoratorName)
    {
        Type type = Type.GetType(decoratorName);
        SkillDecorator skillDecorator = (SkillDecorator)Activator.CreateInstance(type);
        skillDecorators.Add(skillDecorator);
    }
}


