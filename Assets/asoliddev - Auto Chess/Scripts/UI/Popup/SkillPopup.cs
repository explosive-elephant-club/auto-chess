using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;

public class SkillPopup : Popup
{
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI description;

    public TextPair CastDelay;
    public TextPair ChargingDelay;
    public TextPair ManaCost;
    public TextPair Count;
    public TextPair Distance;
    public TextPair Range;

    public Transform damageContent;
    public List<GameObject> damageInfo;
    public Transform buffContent;
    public List<GameObject> buffInfo;



    public override void Show(Skill skill, Vector3 slotPositon, float length, Vector3 dir)
    {
        skillName.text = skill.skillData.name;
        description.text = skill.skillData.description;
        CastDelay.value.text = skill.skillData.castDelay.ToString();
        ChargingDelay.value.text = skill.skillData.chargingDelay.ToString();
        ManaCost.value.text = skill.skillData.manaCost.ToString();
        Count.value.text = skill.skillData.count.ToString();
        Distance.value.text = skill.skillData.distance.ToString();
        Range.value.text = skill.skillData.range.ToString();
        base.Show(skill, slotPositon, length, dir);
    }

}
