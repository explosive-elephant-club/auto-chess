using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;

public class SkillController : MonoBehaviour
{
    [SerializeField]
    public List<Skill> skillList = new List<Skill>();

    List<Skill> skillCheckList = new List<Skill>();
    Skill curSkill = null;
    public EventCenter eventCenter = new EventCenter();

    public Transform skillCastPoint;

    public UnityAction onSkillAnimFinish;
    ChampionController championController;

    // Start is called before the first frame update
    void Start()
    {
        championController = gameObject.GetComponent<ChampionController>();
    }

    // Update is called once per frame
    void Update()
    {
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

    public void AddSkill(int skillID, ChampionController _caster)
    {
        AddSkill(GameData.Instance.skillDatasArray.Find(s => s.ID == skillID), _caster);
    }

    public void AddSkill(SkillData skillData, ChampionController _caster)
    {
        if (skillList.Exists(s => s.skillData == skillData))
            return;
        Skill skill = new Skill(skillData, championController, _caster);
        skillList.Add(skill);
    }

    public void RemoveSkill(Skill skill)
    {
        skillList.Remove(skill);
    }

    public void LoadSkillOrder()
    {
        for (int i = 0; i < skillList.Count; i++)
        {
            skillCheckList.Add(skillList[i]);
        }
        curSkill = null;
    }
}
