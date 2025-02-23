using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;


public class SkillPopup : Popup
{
    public Text skillName;
    public Text description;

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
        skillName.text = skillData.name + " Lvl." + skillData.Level;
        description.text = skillData.description;
        CastDelay.value.text = skillData.castDelay.ToString();
        ChargingDelay.value.text = skillData.chargingDelay.ToString();
        ManaCost.value.text = skillData.manaCost.ToString();
        if (skillData.usableCount != -1)
        {
            Count.value.transform.parent.gameObject.SetActive(true);
            Count.value.text = skillData.usableCount.ToString();
        }
        else
        {
            Count.value.transform.parent.gameObject.SetActive(false);
        }

        Distance.value.text = skillData.distance.ToString();
        Range.value.text = skillData.range.ToString();
        UpdateDamageInfo(skillData);
        base.Show(targetUI, dir);
    }

    void UpdateDamageInfo(SkillData skillData)
    {
        if (skillData.damageData[0].dmg != 0)
        {
            damageContent.parent.gameObject.SetActive(true);
            for (int i = 0; i < damageInfo.Count; i++)
            {
                damageInfo[i].SetActive(false);
                if (i < skillData.damageData.Length)
                {
                    //damageInfo[i].transform.Find("DamageType").GetComponent<Text>().text = string.Format("<sprite=\"AtributeIcon\" name=\"{0}\">", skillData.damageData[i].type);
                    damageInfo[i].transform.Find("DamageType/DamageValue").GetComponent<Text>().text = skillData.damageData[i].dmg.ToString() + "(+" + skillData.damageData[i].correction.ToString() + ")";
                    damageInfo[i].SetActive(true);
                }
            }
        }
        else
        {
            damageContent.parent.gameObject.SetActive(false);
        }

    }
}
