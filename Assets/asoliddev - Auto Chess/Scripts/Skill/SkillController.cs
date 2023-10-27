using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using System.Linq;

public class SkillController : MonoBehaviour
{
    [SerializeField]//所有的技能表
    public List<Skill> skillList = new List<Skill>();

    [SerializeField]//已激活的技能表
    public List<Skill> activedSkillList = new List<Skill>();


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

    public void OnEnterCombat()
    {
        curSkillIndex = -1;
        nextSkillIndex = 0;
        curChargingDelay = GetSkillChargingDelay();
    }

    public void OnUpdateCombat()
    {
        if (curSkillIndex != -1)
            if (activedSkillList[curSkillIndex].state == SkillState.Casting)
            {
                activedSkillList[curSkillIndex].OnCastingUpdate();
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
            cd += s.skillData.chargingDelay;
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
            if (activedSkillList[curSkillIndex].state == SkillState.Casting)
                return;
        if (curCastDelay <= 0)//释放
        {
            Debug.Log("释放 curCastDelay " + curCastDelay);
            if (activedSkillList[nextSkillIndex].IsPrepared())
                activedSkillList[nextSkillIndex].Cast();
            curCastDelay = GetSkillCastDelay(activedSkillList[nextSkillIndex]);
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
        return activedSkillList[nextSkillIndex].skillData.distance;
    }

    public void AddSkill(int skillID, ConstructorBase _constructor)
    {
        AddSkill(GameData.Instance.skillDatasArray.Find(s => s.ID == skillID), _constructor);
    }

    public void AddSkill(SkillData skillData, ConstructorBase _constructor)
    {
        if (skillList.Exists(s => s.skillData == skillData))
            return;
        Skill skill = new Skill(skillData, championController, _constructor);
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
        skillList.Remove(skill);
    }

    public void SetAttackSkill(int skillID)
    {
        activedSkillList.Add(skillList.Find(s => s.skillData.ID == skillID));
    }

    public void Reset()
    {
        foreach (var s in activedSkillList)
        {
            s.Reset();
        }
    }
}
