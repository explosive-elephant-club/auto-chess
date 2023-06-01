using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

public class SkillController : MonoBehaviour
{
    [SerializeField]
    public List<Skill> skillList = new List<Skill>();
    public EventCenter eventCenter = new EventCenter();
    ChampionController championController;

    // Start is called before the first frame update
    void Start()
    {
        championController = gameObject.GetComponent<ChampionController>();
    }

    // Update is called once per frame
    void Update()
    {

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
}
