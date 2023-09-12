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
    void Start()
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
                    if (s.countRemain > 0)
                        s.CDTick();
                    if (s.IsPrepared() && curCastingSkill == null)
                    {

                        s.Cast();
                    }

                    break;
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

    public void RemoveSkill(Skill skill)
    {
        skillList.Remove(skill);
    }

    public void Reset()
    {
        foreach (var s in skillList)
        {
            s.Reset();
        }
    }
}
