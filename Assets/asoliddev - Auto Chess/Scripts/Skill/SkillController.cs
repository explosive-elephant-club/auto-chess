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

    //默认攻击技能
    public Skill attackSkill;


    //正在释放的技能
    public Skill curCastingSkill;

    public EventCenter eventCenter = new EventCenter();


    ChampionController championController;

    // Start is called before the first frame update
    void Awake()
    {
        championController = gameObject.GetComponent<ChampionController>();
    }

    public void OnUpdateCombat()
    {
        foreach (var s in skillList)
        {
            switch (s.state)
            {
                case SkillState.Disable:
                    break;
                case SkillState.Casting:
                    s.OnCastingUpdate();
                    break;
                case SkillState.CD:
                    if (s.countRemain > 0 || s.countRemain == -1)
                        s.CDTick();
                    break;
            }
        }
    }

    public void TryCastAttackSkill()
    {
        Debug.Log("TryCast");
        Debug.Log(EnableCastNewSkill(attackSkill));
        Debug.Log(attackSkill.IsPrepared());
        if (attackSkill.IsPrepared() && EnableCastNewSkill(attackSkill))
        {
            attackSkill.Cast();
        }
    }

    public void TryCastOtherSkill()
    {
        foreach (var s in skillList)
        {
            if (s != attackSkill && s.state == SkillState.CD)
                if (s.IsPrepared() && EnableCastNewSkill(s))
                {
                    s.Cast();
                }

        }
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
        if (attackSkill == skill)
        {
            attackSkill = null;
        }
        skillList.Remove(skill);
    }

    public void SetAttackSkill(int skillID)
    {
        attackSkill.state = SkillState.CD;
        attackSkill = skillList.Find(s => s.skillData.ID == skillID);
    }

    public bool EnableCastNewSkill(Skill skill)
    {
        foreach (var animTrigger in skill.skillData.skillAnimTrigger)
        {
            foreach (var c in skill.constructor.GetAllParentConstructors(true))
            {
                if (c.type.ToString() == animTrigger.constructorType && c.animator != null)
                {
                    if (!c.enablePlayNewSkillAnim)
                        return false;
                }
            }
        }
        return true;
    }

    public bool EnableCastNewSkill()
    {
        foreach (var s in skillList)
        {
            if (s.state == SkillState.CD)
            {
                if (!EnableCastNewSkill(s))
                    return false;
            }
        }
        return true;
    }

    public void Reset()
    {
        curCastingSkill = null;
        foreach (var s in skillList)
        {
            s.Reset();
        }
    }
}
