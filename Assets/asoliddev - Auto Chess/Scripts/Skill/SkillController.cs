using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using System.Linq;
using UnityEngine.PlayerLoop;
using System;

/// <summary>
/// 技能管理器
/// </summary>
public class SkillController
{
    /// <summary>
    /// 拥有的所有技能
    /// </summary>
    public List<Skill> skillList = new List<Skill>();
    /// <summary>
    /// 已激活的技能
    /// </summary>
    public List<Skill> activedSkillList = new List<Skill>();
    /// <summary>
    /// 当前正在使用的技能索引
    /// </summary>
    public int curSkillIndex = -1;
    /// <summary>
    /// 单个技能释放后的延迟
    /// </summary>
    public float curCastDelay = 0;
    /// <summary>
    /// 整个技能链条释放后的延迟
    /// </summary>
    public float curChargingDelay = 0;
    /// <summary>
    /// 当前的延迟
    /// </summary>
    public float cdTimer = 0;
    /// <summary>
    /// 当前护盾技能
    /// </summary>
    public VoidShieldEffect curVoidShieldEffect;

    ChampionController championController;

    public SkillController(ChampionController _championController)
    {
        championController = _championController;
    }

    /// <summary>
    /// 技能容量更新
    /// </summary>
    public void UpdateSkillCapacity()
    {
        int capacity = (int)championController.attributesController.electricPower.GetTrueValue();
        //如果激活技能超过上限，则从末尾删除多余技能
        if (activedSkillList.Count > capacity)
        {
            for (int i = capacity; i < activedSkillList.Count; i++)
            {
                activedSkillList.RemoveAt(activedSkillList.Count - 1);
            }
        }
        //如果激活技能不足，则填充null以补足容量
        else if (activedSkillList.Count < capacity)
        {
            int add = capacity - activedSkillList.Count;
            for (int i = 0; i < add; i++)
            {
                activedSkillList.Add(null);
            }
        }
    }

    public void OnEnterCombat()
    {
        //重置当前技能索引
        curSkillIndex = -1;
        //战斗开始时的技能充能时间
        curChargingDelay = GetSkillChargingDelay();
    }

    public void OnUpdateCombat()
    {
        if (cdTimer > 0)
        {
            cdTimer -= Time.deltaTime;
        }
        //如果当前技能处于持续释放状态，调用 OnCastingUpdateFunc() 处理技能持续效果
        if (curSkillIndex != -1 && activedSkillList[curSkillIndex] != null)
            if (activedSkillList[curSkillIndex].state == SkillState.Casting)
            {
                activedSkillList[curSkillIndex].OnCastingUpdateFunc();
            }
    }

    /// <summary>
    /// 获取单个技能释放延迟
    /// </summary>
    /// <param name="skill">技能</param>
    /// <returns>延迟</returns>
    float GetSkillCastDelay(Skill skill)
    {
        float cd = championController.attributesController.castDelay.GetTrueValue(skill.skillData.castDelay);
        if (cd > 0)
            return cd;
        else
            return 0;
    }
    /// <summary>
    /// 获取整个技能链释放延迟
    /// </summary>
    /// <returns>延迟</returns>
    float GetSkillChargingDelay()
    {
        float cd = 0;
        foreach (var s in activedSkillList)
        {
            if (s != null)
                cd += s.skillData.chargingDelay;
        }
        championController.attributesController.chargingDelay.GetTrueValue(cd);
        if (cd > 0)
            return cd;
        else
            return 0;
    }

    /// <summary>
    /// 获取下一个技能索引
    /// </summary>
    public int GetNextSkillIndex()
    {
        return (curSkillIndex + 1) % activedSkillList.Count;
    }
    /// <summary>
    /// 获取拥有下一个技能的部件
    /// </summary>
    public ConstructorBase GetNextSkillConstructor()
    {
        if (activedSkillList[GetNextSkillIndex()] != null)
            return activedSkillList[GetNextSkillIndex()].constructor;
        else
            return null;
    }

    public bool IsAllNull()
    {
        foreach (var s in activedSkillList)
        {
            if (s != null)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 是否正在持续施法
    /// </summary>
    /// <returns></returns>
    public bool isCasting()
    {
        if (curSkillIndex != -1 && activedSkillList[curSkillIndex] != null)//等待持续施法
        {
            if (activedSkillList[curSkillIndex].state == SkillState.Casting)
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 跳过下一个空技能
    /// </summary>
    public void SkipEmptySkill()
    {
        curSkillIndex = (curSkillIndex + 1) % activedSkillList.Count;
        if (GetNextSkillIndex() == 0)//一轮释放完毕
        {
            if (curCastDelay < curChargingDelay)
            {
                curCastDelay = curChargingDelay;
                cdTimer = curChargingDelay;
            }
        }
    }

    /// <summary>
    /// 尝试释放技能
    /// </summary>
    public void TryCastSkill()
    {
        //检查冷却时间 是否为 0，如果不为0，则不能释放技能
        if (cdTimer <= 0)
        {
            //检查技能是否准备就绪
            if (activedSkillList[GetNextSkillIndex()].IsPrepared())
            {
                //应用技能装饰器附加效果
                foreach (var d in activedSkillList[GetNextSkillIndex()].skillDecorators)
                {
                    if (!d.hasDecorated)
                    {
                        d.Decorate(activedSkillList[GetNextSkillIndex()]);
                    }
                }
                //释放
                activedSkillList[GetNextSkillIndex()].CastFunc();
                //处理单个技能释放完毕后的充能时间
                curCastDelay = GetSkillCastDelay(activedSkillList[GetNextSkillIndex()]);
                cdTimer = curCastDelay;
            }
            //切换到下一个技能
            curSkillIndex = (curSkillIndex + 1) % activedSkillList.Count;
            //处理整轮技能链释放完毕后的充能时间
            if (GetNextSkillIndex() == 0)
            {
                if (curCastDelay < curChargingDelay)
                {
                    curCastDelay = curChargingDelay;
                    cdTimer = curChargingDelay;
                }
            }


        }
    }

    bool HasNextPreparedSkill()
    {
        int index = curSkillIndex;
        for (int i = 1; i < activedSkillList.Count; i++)
        {
            index = (index + i) % activedSkillList.Count;
            if (activedSkillList[index] != null)
            {
                if (activedSkillList[index].IsAvailable())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ApplySkillDecorator(Skill skill)
    {
        foreach (var d in skill.skillDecorators)
        {
            if (!d.hasDecorated)
            {
                skill = d.Decorate(skill);
            }
        }
    }

    /// <summary>
    /// 获取下一个不为null的可用技能
    /// </summary>
    /// <returns></returns>
    public Skill GetNextAvailableSkill()
    {
        int index = GetNextSkillIndex();
        for (int i = 0; i < activedSkillList.Count; i++)
        {
            index = (index + i) % activedSkillList.Count;
            if (activedSkillList[index] != null)
            {
                if (activedSkillList[index].countRemain > 0 || activedSkillList[index].countRemain == -1)
                {
                    return activedSkillList[index];
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 获取下一个技能
    /// </summary>
    /// <returns></returns>
    public Skill GetNextSkill()
    {
        return activedSkillList[GetNextSkillIndex()];
    }

    /// <summary>
    /// 添加技能 
    /// </summary>
    /// <param name="skillID">技能配置ID</param>
    /// <param name="_constructor">拥有此技能的部件</param>
    public void AddSkill(int skillID, ConstructorBase _constructor)
    {
        AddSkill(GameExcelConfig.Instance.skillDatasArray.Find(s => s.ID == skillID), _constructor);
    }
    /// <summary>
    /// 添加技能
    /// </summary>
    /// <param name="skillData">技能配置数据</param>
    /// <param name="_constructor">拥有此技能的部件</param>
    public void AddSkill(SkillData skillData, ConstructorBase _constructor)
    {
        Skill skill = new Skill();
        skill.Init(skillData, championController, _constructor);
        /*foreach (var d in skill.skillDecorators)
        {
            if (!d.hasDecorated)
            {
                skill = d.Decorate(skill);
            }
        }*/

        skillList.Add(skill);
    }
    /// <summary>
    /// 移除技能
    /// </summary>
    /// <param name="_constructor">拥有此技能的部件</param>
    public void RemoveSkill(ConstructorBase _constructor)
    {
        for (int i = 0; i < skillList.Count; i++)
        {
            if (skillList[i].constructor == _constructor)
            {
                RemoveSkill(skillList[i]);
            }
        }
    }
    /// <summary>
    /// 移除技能
    /// </summary>
    /// <param name="skill">被移除的技能</param>
    public void RemoveSkill(Skill skill)
    {
        if (activedSkillList.Contains(skill))
            RemoveActivedSkill(activedSkillList.IndexOf(skill));
        skillList.Remove(skill);
    }
    /// <summary>
    /// UI操作 交换两个未被激活的技能
    /// </summary>
    /// <param name="index1">技能1在列表中的位置</param>
    /// <param name="index2">技能2在列表中的位置</param>
    public void SwitchDeactivedSkill(int index1, int index2)
    {
        Skill tempSkill = skillList[index1];
        skillList[index1] = skillList[index2];
        skillList[index2] = tempSkill;
    }
    /// <summary>
    /// UI操作 交换两个被激活的技能
    /// </summary>
    /// <param name="index1">技能1在列表中的位置</param>
    /// <param name="index2">技能2在列表中的位置</param>
    public void SwitchActivedSkill(int index1, int index2)
    {
        Skill tempSkill = activedSkillList[index1];
        activedSkillList[index1] = activedSkillList[index2];
        activedSkillList[index2] = tempSkill;
    }
    /// <summary>
    /// UI操作 添加一个激活的技能
    /// </summary>
    /// <param name="addIndex">被添加的位置</param>
    /// <param name="sourceIndex">原本的位置</param>
    public void AddActivedSkill(int addIndex, int sourceIndex)
    {
        if (activedSkillList[addIndex] != null)
        {
            activedSkillList[addIndex].state = SkillState.Disable;
        }
        activedSkillList[addIndex] = skillList[sourceIndex];
        activedSkillList[addIndex].state = SkillState.CD;
    }
    /// <summary>
    /// UI操作 添加一个激活的技能
    /// </summary>
    /// <param name="skillID">技能配置ID</param>
    public void AddActivedSkill(int skillID)
    {
        int activedIndex = -1;
        int skillIndex = -1;
        foreach (var s in activedSkillList)
        {
            if (s == null)
                activedIndex = activedSkillList.IndexOf(s);
        }
        foreach (var s in skillList)
        {
            if (s.skillData.ID == skillID && s.state == SkillState.Disable)
                skillIndex = skillList.IndexOf(s);
        }
        if (activedIndex == -1 || skillIndex == -1)
            return;
        AddActivedSkill(activedIndex, skillIndex);
    }
    /// <summary>
    /// UI操作 移除一个激活的技能
    /// </summary>
    /// <param name="index">技能在列表中的位置</param>
    public void RemoveActivedSkill(int index)
    {
        activedSkillList[index].state = SkillState.Disable;
        activedSkillList[index] = null;
    }
    /// <summary>
    /// 重置所有激活技能的状态
    /// </summary>
    public void Reset()
    {
        foreach (var s in activedSkillList)
        {
            if (s != null)
                s.ResetFunc();
        }
    }
}
