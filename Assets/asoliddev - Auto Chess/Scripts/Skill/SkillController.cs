using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using System.Linq;
using UnityEngine.PlayerLoop;
using System;


[Serializable]
public class SkillContainer
{
    public Skill skill;
    public int slotIndex;

    public SkillContainer(Skill _skill, int _slotIndex)
    {
        skill = _skill;
        slotIndex = _slotIndex;
    }
}

public class SkillController : MonoBehaviour
{
    //所有的技能表
    public List<SkillContainer> skillList = new List<SkillContainer>();

    //已激活的技能表
    public List<SkillContainer> activedSkillList = new List<SkillContainer>();

    int curSkillIndex = -1;
    int nextSkillIndex = 0;
    float curCastDelay = 0;
    float curChargingDelay = 0;

    ChampionController championController;

    // Start is called before the first frame update
    void Awake()
    {
        championController = gameObject.GetComponent<ChampionController>();
    }

    public void UpdateSkillCapacity()
    {
        int capacity = (int)championController.attributesController.electricPower.GetTrueLinearValue();
        if (activedSkillList.Count > capacity)
        {
            for (int i = capacity; i < activedSkillList.Count; i++)
            {
                RemoveActivedSkill(activedSkillList[i].skill);
            }
        }
    }

    public void OnEnterCombat()
    {
        curSkillIndex = -1;
        nextSkillIndex = 0;
        curChargingDelay = GetSkillChargingDelay();
    }

    public void OnUpdateCombat()
    {
        if (curSkillIndex != -1)
            if (activedSkillList[curSkillIndex].skill.state == SkillState.Casting)
            {
                activedSkillList[curSkillIndex].skill.OnCastingUpdate();
            }
    }

    float GetSkillCastDelay(Skill skill)
    {
        float cd = championController.attributesController.castDelay.GetTrueLinearValue()
         + skill.skillData.castDelay;
        cd *= 1 - championController.attributesController.castDelayDecr.GetTrueMultipleValue();
        if (cd > 0)
            return cd;
        else
            return 0;
    }

    float GetSkillChargingDelay()
    {
        float cd = championController.attributesController.chargingDelay.GetTrueLinearValue();
        foreach (var s in activedSkillList)
        {
            cd += s.skill.skillData.chargingDelay;
        }
        cd *= 1 - championController.attributesController.chargingDelayDecr.GetTrueMultipleValue();
        if (cd > 0)
            return cd;
        else
            return 0;
    }

    public void TryCastSkill()
    {
        //Debug.Log("TryCastSkill" + curCastDelay);

        if (curSkillIndex != -1)
            if (activedSkillList[curSkillIndex].skill.state == SkillState.Casting)
                return;
        if (curCastDelay <= 0)//释放
        {
            if (activedSkillList[nextSkillIndex].skill.IsPrepared())
                activedSkillList[nextSkillIndex].skill.Cast();
            curCastDelay = GetSkillCastDelay(activedSkillList[nextSkillIndex].skill);
            curSkillIndex = nextSkillIndex;
            if (nextSkillIndex + 1 < activedSkillList.Count)//顺序释放
            {
                nextSkillIndex++;
            }
            else//一轮技能释放完毕,充能
            {
                if (curCastDelay < curChargingDelay)
                {
                    curCastDelay = curChargingDelay;
                    nextSkillIndex = 0;
                }
            }

        }
        else
        {
            //Debug.Log("curCastDelay " + curCastDelay);
            curCastDelay -= Time.deltaTime;
        }
    }

    public int GetNextSkillRange()
    {
        return activedSkillList[nextSkillIndex].skill.skillData.distance;
    }

    public void AddSkill(int skillID, ConstructorBase _constructor)
    {
        AddSkill(GameData.Instance.skillDatasArray.Find(s => s.ID == skillID), _constructor);
    }

    public void AddSkill(SkillData skillData, ConstructorBase _constructor)
    {
        int index = 0;
        for (int i = 0; i < skillList.Count; i++)
        {
            SkillContainer sc = skillList.Find(s => s.slotIndex == i);
            if (sc == null)
            {
                index = i;
                break;
            }
        }
        Skill skill = new Skill(skillData, championController, _constructor);
        SkillContainer skillContainer = new SkillContainer(skill, index);
        skillList.Add(skillContainer);
    }

    public void RemoveSkill(ConstructorBase _constructor)
    {
        for (int i = 0; i < skillList.Count; i++)
        {
            if (skillList[i].skill.constructor == _constructor)
            {
                RemoveSkill(skillList[i]);
            }
        }
    }

    public void RemoveSkill(SkillContainer skillContainer)
    {
        RemoveActivedSkill(skillContainer.skill);
        skillList.Remove(skillContainer);
    }

    public void AddActivedSkill(SkillContainer skillContainer, int index)
    {
        if (skillList.Contains(skillContainer))
        {
            skillContainer.skill.state = SkillState.CD;

            activedSkillList.Add(new SkillContainer(skillContainer.skill, index));
        }
    }

    public void RemoveActivedSkill(Skill skill)
    {
        SkillContainer sc = activedSkillList.Find(s => s.skill == skill);
        if (sc != null)
        {
            skill.state = SkillState.Disable;
            activedSkillList.Remove(sc);
        }
    }

    public void Reset()
    {
        foreach (var s in activedSkillList)
        {
            s.skill.Reset();
        }
    }
}
