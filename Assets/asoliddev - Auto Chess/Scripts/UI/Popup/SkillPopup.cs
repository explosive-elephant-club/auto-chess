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

    private void Start()
    {
        foreach (Transform child in damageContent)
        {
            damageInfo.Add(child.gameObject);
        }
    }

    public void Show(SkillData skillData, GameObject targetUI, Vector3 dir)
    {
        skillName.text = skillData.name;
        description.text = skillData.description;
        CastDelay.value.text = skillData.castDelay.ToString();
        ChargingDelay.value.text = skillData.chargingDelay.ToString();
        ManaCost.value.text = skillData.manaCost.ToString();
        Count.value.text = skillData.count.ToString();
        Distance.value.text = skillData.distance.ToString();
        Range.value.text = skillData.range.ToString();
        base.Show(targetUI, dir);

        UpdateDamageInfo(skillData);
    }

    void UpdateDamageInfo(SkillData skillData)
    {
        for (int i = 0; i < damageInfo.Count; i++)
        {
            damageInfo[i].SetActive(false);
            if (i < skillData.damageData.Length)
            {
                damageInfo[i].transform.Find("DamageValue").GetComponent<TextMeshProUGUI>().text = skillData.damageData[i].dmg.ToString();
                damageInfo[i].transform.Find("CorrectionValue").GetComponent<TextMeshProUGUI>().text = skillData.damageData[i].correction.ToString();
                damageInfo[i].SetActive(true);
            }
        }
    }
}
