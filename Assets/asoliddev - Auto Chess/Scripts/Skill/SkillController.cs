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
    public List<ISkillState> skillList = new ();
    /// <summary>
    /// 已激活的技能
    /// </summary>
    public List<ISkillState> activedSkillList = new ();
    
    private SkillExeProcess _skillExeProcess;
    
    /// <summary>
    /// 当前正在使用的技能索引
    /// </summary>
    private int _curSkillIndex;
    // /// <summary>
    // /// 单个技能释放后的延迟(单个技能的冷却时间)
    // /// </summary>
    // public float curCastDelay = 0;
    // /// <summary>
    // /// 整个技能链条释放后的延迟
    // /// </summary>
    // public float curChargingDelay = 0;
    // /// <summary>
    // /// 当前的延迟
    // /// </summary>
    // public float cdTimer = 0;
    /// <summary>
    /// 当前护盾技能
    /// </summary>
    public VoidShieldEffect curVoidShieldEffect;

    ChampionController championController;

    public SkillController(ChampionController _championController)
    {
        championController = _championController;
        _skillExeProcess = new SkillExeProcess();
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
        _curSkillIndex = 0;
        //战斗开始时的技能链充能时间
        foreach (var skill in activedSkillList)
        {
            if(skill != null)
                _usedSkillList.Add(skill);
        }
        StartSkillChainCd();
    }

    
    public void Tick(out bool needFindTarget)
    {
        needFindTarget = false;
        /*if (cdTimer > 0)
        {
            cdTimer -= Time.deltaTime;
        }
        //如果当前技能处于持续释放状态，调用 OnCastingUpdateFunc() 处理技能持续效果
        if (curSkillIndex != -1 && activedSkillList[curSkillIndex] != null)
            if (activedSkillList[curSkillIndex].state == SkillState.Casting)
            {
                activedSkillList[curSkillIndex].OnCastingUpdateFunc();
            }*/
        _skillExeProcess.ExecuteSkill();
        switch (_skillExeProcess.GetCurState())
        {
            case ProcessExeState.Done:
            case ProcessExeState.None:
                var nextSkill = GetNextSkillState();
                if (nextSkill != null)
                {
                    needFindTarget = nextSkill.HaveTargetInRange();
                    _skillExeProcess.SetCurSkill(nextSkill);
                }
                break;
            case ProcessExeState.Ing:
                break;
        }
    }

    public void TickSellSkill()
    {
        _skillExeProcess.ExecuteSellSkill();
    }

    public void TickSkillCd()
    {
        foreach (var skill in activedSkillList)
        {
            if(skill == null) continue;
            skill.TickCd();
        }
    }

    private ISkillState GetNextSkillState()
    {
        var index = _curSkillIndex;
        for (int i = 0; i < activedSkillList.Count; i++)
        {
            index = ++index % activedSkillList.Count;
            if(activedSkillList[index] == null) continue;
            if (activedSkillList[index].IsPrepared())
            {
                if(index <= _curSkillIndex)
                    StartSkillChainCd();
                _curSkillIndex = index;
                return activedSkillList[index];
            }
        }
        return null;
    }

    public ISkillState GetNextActiveSkillState()
    {
        return _curSkillIndex < 0 ? null : activedSkillList[_curSkillIndex];
    }
    
    // 这里有个问题，如果没蓝了，即使是最后一个技能，也会返回false, 走充能时间，而不是技能链CD
    public bool CheckIsLastSkill()
    {
        var index = _curSkillIndex;
        foreach (var _ in activedSkillList)
        {
            index = ++index % activedSkillList.Count;
            if(activedSkillList[index] == null) continue;
            if (activedSkillList[index].IsPrepared())
            {
                if(index <= _curSkillIndex)
                    return true;
            }
        }
        return false;
    }

    private void StartSkillChainCd()
    {
        var chainCd = GetSkillChargingDelay();
        foreach (var skill in activedSkillList)
        {
            if(skill == null) continue;
            skill.StartSkillChainCdCutDown(chainCd);
        }
    }

    /// <summary>
    /// 获取整个技能链释放延迟
    /// </summary>
    /// <returns>延迟</returns>
    private List<ISkillState> _usedSkillList = new();
    float GetSkillChargingDelay()
    {
        float cd = 0;
        foreach (var s in _usedSkillList)
        {
            cd += s.GetSkillChargingDelay();
        }
        _usedSkillList.Clear();
        championController.attributesController.chargingDelay.GetTrueValue(cd);
        if (cd > 0)
            return cd;
        else
            return 0;
    }

    public void AddUsedSkill(ISkillState skill)
    {
        _usedSkillList.Add(skill);
    }
    

    // /// <summary>
    // /// 是否正在持续施法
    // /// </summary>
    // /// <returns></returns>
    // public bool isCasting()
    // {
    //     if (_curSkillIndex != -1 && activedSkillList[_curSkillIndex] != null)//等待持续施法
    //     {
    //         if (activedSkillList[_curSkillIndex].ExeState() == SkillExeResult.Ing)
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }

    #region UI操作相关
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
        // Skill skill = new Skill();
        // skill.Init(skillData, championController, _constructor);
        /*foreach (var d in skill.skillDecorators)
        {
            if (!d.hasDecorated)
            {
                skill = d.Decorate(skill);
            }
        }*/
        var skill = SkillFactory.Create(skillData, championController, _constructor);
        skill.ResetSkillContext(true);
        skillList.Add(skill);
    }
    /// <summary>
    /// 移除技能
    /// </summary>
    /// <param name="_constructor">拥有此技能的部件</param>
    public void RemoveSkill(ConstructorBase _constructor)
    {
        for (int i = skillList.Count - 1; i >= 0; i--)
        {
            if (skillList[i].GetConstructor() == _constructor)
            {
                RemoveSkill(skillList[i]);
            }
        }
    }
    /// <summary>
    /// 移除技能
    /// </summary>
    /// <param name="skill">被移除的技能</param>
    public void RemoveSkill(ISkillState skill)
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
        (skillList[index1], skillList[index2]) = (skillList[index2], skillList[index1]);
    }
    
    /// <summary>
    /// UI操作 交换两个被激活的技能
    /// </summary>
    /// <param name="index1">技能1在列表中的位置</param>
    /// <param name="index2">技能2在列表中的位置</param>
    public void SwitchActivedSkill(int index1, int index2)
    {
        (activedSkillList[index1], activedSkillList[index2]) = (activedSkillList[index2], activedSkillList[index1]);
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
            activedSkillList[addIndex].GetContext().State = SkillState.Disable;
        }
        activedSkillList[addIndex] = skillList[sourceIndex];
        activedSkillList[addIndex].GetContext().State = SkillState.Activied;
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
            if (s.GetSkillCfg().ID == skillID && s.GetContext().State == SkillState.Disable)
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
        activedSkillList[index].GetContext().State = SkillState.Disable;
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
                s.ResetSkillContext(true);
        }
    }
    #endregion
}
