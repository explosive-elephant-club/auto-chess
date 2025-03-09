using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;
using Game;


public class SkillPopup : Popup
{
    public List<SkillDamageInfo> damageInfos;

    #region 自动绑定
    private Image _imgCount;
    private UICustomText _textNameText;
    private UICustomText _textLevelText;
    private UICustomText _textDescriptionText;
    private UICustomText _textCastDelayText;
    private UICustomText _textCargingDelayText;
    private UICustomText _textManaCostText;
    private UICustomText _textCountValueText;
    private UICustomText _textDistanceText;
    private UICustomText _textRangeText;
    private HorizontalLayoutGroup _layoutGroupCount;
    private GridLayoutGroup _layoutGroupDamageContent;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _imgCount = transform.Find("Attributes/Panel/Count_Auto").GetComponent<Image>();
        _textNameText = transform.Find("Name/NameText_Auto").GetComponent<UICustomText>();
        _textLevelText = transform.Find("Name/LevelText _Auto").GetComponent<UICustomText>();
        _textDescriptionText = transform.Find("Description/DescriptionText_Auto").GetComponent<UICustomText>();
        _textCastDelayText = transform.Find("Attributes/Panel/CastDelay/BG/CastDelayText_Auto").GetComponent<UICustomText>();
        _textCargingDelayText = transform.Find("Attributes/Panel/ChargingDelay/BG/CargingDelayText_Auto").GetComponent<UICustomText>();
        _textManaCostText = transform.Find("Attributes/Panel/ManaCost/BG/ManaCostText_Auto").GetComponent<UICustomText>();
        _textCountValueText = transform.Find("Attributes/Panel/Count_Auto/BG/CountValueText_Auto").GetComponent<UICustomText>();
        _textDistanceText = transform.Find("Attributes/Panel/Distance/BG/DistanceText_Auto").GetComponent<UICustomText>();
        _textRangeText = transform.Find("Attributes/Panel/Range/BG/RangeText_Auto").GetComponent<UICustomText>();
        _layoutGroupCount = transform.Find("Attributes/Panel/Count_Auto").GetComponent<HorizontalLayoutGroup>();
        _layoutGroupDamageContent = transform.Find("Damage/DamageContent_Auto").GetComponent<GridLayoutGroup>();
    }
    #endregion




    private void Start()
    {
        foreach (Transform child in _layoutGroupDamageContent.transform)
        {
            damageInfos.Add(child.GetComponent<SkillDamageInfo>());
        }
    }

    public void Show(SkillData skillData, GameObject targetUI, Vector3 dir)
    {
        _textNameText.text = skillData.name;
        _textLevelText.text = " Lvl." + skillData.Level;
        _textDescriptionText.text = skillData.description;
        _textCastDelayText.text = skillData.castDelay.ToString();
        _textCargingDelayText.text = skillData.chargingDelay.ToString();
        _textManaCostText.text = skillData.manaCost.ToString();
        if (skillData.usableCount != -1)
        {
            _imgCount.gameObject.SetActive(true);
            _textCountValueText.text = skillData.usableCount.ToString();
        }
        else
        {
            _imgCount.gameObject.SetActive(false);
        }

        _textDistanceText.text = skillData.distance.ToString();
        _textRangeText.text = skillData.range.ToString();
        UpdateDamageInfo(skillData);
        base.Show(targetUI, dir);
    }

    void UpdateDamageInfo(SkillData skillData)
    {
        if (skillData.damageData[0].dmg != 0)
        {
            _layoutGroupDamageContent.transform.parent.gameObject.SetActive(true);
            for (int i = 0; i < damageInfos.Count; i++)
            {
                damageInfos[i].SetUIActive(false);
                if (i < skillData.damageData.Length)
                {
                    damageInfos[i].Init(skillData.damageData[i]);
                    damageInfos[i].SetUIActive(true);
                }
            }
        }
        else
        {
            _layoutGroupDamageContent.transform.parent.gameObject.SetActive(false);
        }

    }
}
