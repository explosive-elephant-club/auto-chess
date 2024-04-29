using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using System.Linq;
using UnityEngine.PlayerLoop;
using System;


public class SkillController : MonoBehaviour
{
    //所有的技能表
    public List<Skill> skillList = new List<Skill>();

    //已激活的技能表
    public List<Skill> activedSkillList = new List<Skill>();

    public int curSkillIndex = -1;
    public float curCastDelay = 0;
    public float curChargingDelay = 0;
    public float cdTimer = 0;

    ChampionController championController;

    void Awake()
    {
        championController = gameObject.GetComponent<ChampionController>();
    }

    public void UpdateSkillCapacity()
    {
        int capacity = (int)championController.attributesController.electricPower.GetTrueValue();
        if (activedSkillList.Count > capacity)
        {
            for (int i = capacity; i < activedSkillList.Count; i++)
            {
                activedSkillList.RemoveAt(activedSkillList.Count - 1);
            }
        }
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
        curSkillIndex = -1;
        curChargingDelay = GetSkillChargingDelay();
    }

    public void OnUpdateCombat()
    {
        if (curSkillIndex != -1 && activedSkillList[curSkillIndex] != null)
            if (activedSkillList[curSkillIndex].state == SkillState.Casting)
            {
                activedSkillList[curSkillIndex].OnCastingUpdate();
            }
    }

    float GetSkillCastDelay(Skill skill)
    {
        float cd = championController.attributesController.castDelay.GetTrueValue(skill.skillData.castDelay);
        if (cd > 0)
            return cd;
        else
            return 0;
    }

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

    public int GetNextSkillIndex()
    {
        return (curSkillIndex + 1) % activedSkillList.Count;
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

    public void TryCastSkill()
    {
        if (curSkillIndex != -1 && activedSkillList[curSkillIndex] != null)//等待持续施法
        {

            if (activedSkillList[curSkillIndex].state == SkillState.Casting)
            {
                cdTimer -= Time.deltaTime;
                return;
            }

        }
        if (activedSkillList[GetNextSkillIndex()] == null)//跳过null技能
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
            return;
        }


        if (cdTimer <= 0)//释放
        {
            if (activedSkillList[GetNextSkillIndex()].IsPrepared())
            {
                foreach (var d in activedSkillList[GetNextSkillIndex()].skillDecorators)
                {
                    if (!d.hasDecorated)
                    {
                        activedSkillList[GetNextSkillIndex()] = d.Decorate(activedSkillList[GetNextSkillIndex()]);
                    }
                }
                activedSkillList[GetNextSkillIndex()].Cast();
                curCastDelay = GetSkillCastDelay(activedSkillList[GetNextSkillIndex()]);
                cdTimer = curCastDelay;
            }
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
        else
        {
            //Debug.Log("curCastDelay " + curCastDelay);
            cdTimer -= Time.deltaTime;
        }
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

    public int GetNextSkillRange()
    {
        int index = GetNextSkillIndex();
        while (activedSkillList[index] == null)
        {
            index = (index + 1) % activedSkillList.Count;
        }
        return activedSkillList[index].skillData.distance;
    }

    public Skill GetNextSkill()
    {
        return activedSkillList[GetNextSkillIndex()];
    }

    public void AddSkill(int skillID, ConstructorBase _constructor)
    {
        AddSkill(GameExcelConfig.Instance.skillDatasArray.Find(s => s.ID == skillID), _constructor);
    }

    public void AddSkill(SkillData skillData, ConstructorBase _constructor)
    {
        Skill skill = new Skill();
        skill.AddDecorators(skillData);
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

    public void RemoveSkill(Skill skill)
    {
        if (activedSkillList.Contains(skill))
            RemoveActivedSkill(activedSkillList.IndexOf(skill));
        skillList.Remove(skill);
    }

    public void SwitchDeactivedSkill(int index1, int index2)
    {
        Skill tempSkill = skillList[index1];
        skillList[index1] = skillList[index2];
        skillList[index2] = tempSkill;
    }

    public void SwitchActivedSkill(int index1, int index2)
    {
        Skill tempSkill = activedSkillList[index1];
        activedSkillList[index1] = activedSkillList[index2];
        activedSkillList[index2] = tempSkill;
    }

    public void AddActivedSkill(int addIndex, int sourceIndex)
    {
        if (activedSkillList[addIndex] != null)
        {
            activedSkillList[addIndex].state = SkillState.Disable;
        }
        activedSkillList[addIndex] = skillList[sourceIndex];
        activedSkillList[addIndex].state = SkillState.CD;
    }

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

    public void RemoveActivedSkill(int index)
    {
        activedSkillList[index].state = SkillState.Disable;
        activedSkillList[index] = null;
    }

    public void Reset()
    {
        foreach (var s in activedSkillList)
        {
            if (s != null)
                s.Reset();
        }
    }
}
