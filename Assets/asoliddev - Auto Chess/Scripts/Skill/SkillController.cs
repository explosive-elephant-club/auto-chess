using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using System.Linq;

public class SkillController : MonoBehaviour
{
    public int[] defSkillIDList;

    [SerializeField]
    public List<Skill> skillList = new List<Skill>();

    public List<Skill> skillCheckList = new List<Skill>();
    public Skill curSkill = null;
    public EventCenter eventCenter = new EventCenter();

    public Transform skillCastPoint;

    public UnityAction onSkillAnimFinish;
    ChampionController championController;

    // Start is called before the first frame update
    void Start()
    {
        championController = gameObject.GetComponent<ChampionController>();
        CheckAllUnLockRequire();
        curSkill = null;
    }

    public void OnUpdateCombat()
    {
        foreach (var s in skillList)
        {
            s.CDTick();
        }

        if (curSkill == null)
        {
            for (int i = 0; i < skillCheckList.Count; i++)
            {
                if (skillCheckList[i].IsPrepared())
                {
                    curSkill = skillCheckList[i];
                    skillCheckList.RemoveAt(i);
                    skillCheckList.Add(curSkill);
                    curSkill.Cast(skillCastPoint);
                    onSkillAnimFinish = new UnityAction(() =>
                    {
                        curSkill.OnFinish();
                    });
                    break;
                }
            }
        }
        else
        {
            if (curSkill.state == SkillState.Casting)
            {
                curSkill.OnCastingUpdate();
            }
        }

    }

    public void AddSkill(int skillID)
    {
        AddSkill(GameData.Instance.skillDatasArray.Find(s => s.ID == skillID));
    }

    public void AddSkill(SkillData skillData)
    {
        if (skillList.Exists(s => s.skillData == skillData))
            return;
        Skill skill = new Skill(skillData, championController);
        skillList.Add(skill);
    }

    public void RemoveSkill(Skill skill)
    {
        skillList.Remove(skill);
    }

    public void LoadSkillOrder()
    {
        skillCheckList = new List<Skill>();
        for (int i = 0; i < skillList.Count; i++)
        {
            skillCheckList.Add(skillList[i]);
        }
        curSkill = null;
    }


    public void CheckAllUnLockRequire()
    {
        foreach (var id in defSkillIDList)
        {
            SkillData skillData = GameData.Instance.skillDatasArray.Find(s => s.ID == id);
            Skill skill = new Skill(skillData, championController);
            if (skill.CheckUnLockRequire())
            {
                AddSkill(skillData);
            }
        }
        LoadSkillOrder();
    }

    public void Reset()
    {
        CheckAllUnLockRequire();
        foreach (var s in skillList)
        {
            s.Reset();
        }
    }
}
