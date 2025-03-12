using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using General;
using System.Diagnostics;
using Game;

public class ChampionInfoController : BaseControllerUI
{
    //Skill
    public List<SkillSlot> activatedSkillSlots = new List<SkillSlot>();
    public List<SkillSlot> deactivatedSkillSlots = new List<SkillSlot>();

    public SkillSlot pointEnterSlot;

    Dictionary<string, ChampionAttributeInfo> championAttributeInfos = new Dictionary<string, ChampionAttributeInfo>();

    ChampionController championController;
    ChampionAttributesController attributesController;
    SkillController skillController;

    public override void Awake()
    {
        base.Awake();
        Init();
        foreach (Transform child in _layoutGroupActivatedSkillContent.transform)
        {
            activatedSkillSlots.Add(child.gameObject.GetComponent<SkillSlot>());
        }
        foreach (Transform child in _layoutGroupDeactivatedSkilltContent.transform)
        {
            deactivatedSkillSlots.Add(child.gameObject.GetComponent<SkillSlot>());
        }
        foreach (var attributeInfo in _layoutGroupAttributesContent.transform.GetComponentsInChildren<ChampionAttributeInfo>())
        {
            championAttributeInfos.Add(attributeInfo.gameObject.name, attributeInfo);
        }
    }

    #region 自动绑定

    private VerticalLayoutGroup _layoutGroupArmor;
    private VerticalLayoutGroup _layoutGroupHP;
    private VerticalLayoutGroup _layoutGroupMP;
    private GridLayoutGroup _layoutGroupManufacturersContent;
    private GridLayoutGroup _layoutGroupFeaturesContent;
    private HorizontalLayoutGroup _layoutGroupAttributesContent;
    private GridLayoutGroup _layoutGroupActivatedSkillContent;
    private GridLayoutGroup _layoutGroupDeactivatedSkilltContent;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _layoutGroupArmor = transform.Find("Panel/State/Bar/Armor_Auto").GetComponent<VerticalLayoutGroup>();
        _layoutGroupHP = transform.Find("Panel/State/Bar/HP_Auto").GetComponent<VerticalLayoutGroup>();
        _layoutGroupMP = transform.Find("Panel/State/Bar/MP_Auto").GetComponent<VerticalLayoutGroup>();
        _layoutGroupManufacturersContent = transform.Find("Panel/Manufacturers/ManufacturersContent_Auto").GetComponent<GridLayoutGroup>();
        _layoutGroupFeaturesContent = transform.Find("Panel/Features/FeaturesContent_Auto").GetComponent<GridLayoutGroup>();
        _layoutGroupAttributesContent = transform.Find("Panel/Attributes/AttributesContent_Auto").GetComponent<HorizontalLayoutGroup>();
        _layoutGroupActivatedSkillContent = transform.Find("Panel/Skill/ActivatedSkill/ActivatedSkillContent_Auto").GetComponent<GridLayoutGroup>();
        _layoutGroupDeactivatedSkilltContent = transform.Find("Panel/Skill/DeactivatedSkill/DeactivatedSkilltContent_Auto").GetComponent<GridLayoutGroup>();
    }
    #endregion


    // Update is called once per frame
    public override void UpdateUI()
    {
        if (GamePlayController.Instance.pickedChampion != null)
        {
            championController = GamePlayController.Instance.pickedChampion;
            attributesController = championController.attributesController;
            skillController = championController.skillController;

            UpdateArmorBar();
            UpdateMechBar();
            UpdateManaBar();
            UpdateBonusBar();
            UpdateAttributeData();
            UpdateSkillSlot();
            GeneralMethod.ForceRefreshContentSizeFitterUpwards(_layoutGroupActivatedSkillContent.transform);
            GeneralMethod.ForceRefreshContentSizeFitterUpwards(_layoutGroupDeactivatedSkilltContent.transform);
            SetUIActive(true);
        }
        else
        {
            SetUIActive(false);
        }
    }


    public void OnPointEnterSlot(SkillSlot skillSlot)
    {
        pointEnterSlot = skillSlot;
        if (pointEnterSlot.skill != null)
        {
            if (!pointEnterSlot.isActivated && pointEnterSlot.skill.state != SkillState.Disable)
                return;
            UIController.Instance.popupController.skillPopup.Show
                (pointEnterSlot.skill.skillData, pointEnterSlot.gameObject, Vector3.right);
        }

    }

    public void OnPointLeaveSlot()
    {
        pointEnterSlot = null;
        UIController.Instance.popupController.skillPopup.Clear();
    }

    public void OnEnterCombat()
    {
        _layoutGroupActivatedSkillContent.GetComponentInParent<CanvasGroup>().blocksRaycasts = false;
        _layoutGroupDeactivatedSkilltContent.gameObject.SetActive(false);
    }

    public void OnEnterPreparation()
    {
        _layoutGroupActivatedSkillContent.GetComponentInParent<CanvasGroup>().blocksRaycasts = true;
        _layoutGroupDeactivatedSkilltContent.gameObject.SetActive(true);
    }

    public void OnUpdatePreparation()
    {
        foreach (var item in championAttributeInfos)
        {
            item.Value.UpdateUI();
        }
    }

    public void OnUpdateCombat()
    {
        if (championController != null)
        {
            UpdateArmorBar();
            UpdateMechBar();
            UpdateManaBar();
        }

    }

    public void UpdateArmorBar()
    {

        _layoutGroupArmor.transform.Find("TextPanel/ValueText").GetComponent<Text>().text =
            Mathf.Floor(attributesController.curArmor) + "/" +
                Mathf.Floor(attributesController.maxArmor.GetTrueValue());

        _layoutGroupArmor.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curArmor / attributesController.maxArmor.GetTrueValue();
    }

    public void UpdateMechBar()
    {
        _layoutGroupHP.transform.Find("TextPanel/ValueText").GetComponent<Text>().text =
            Mathf.Floor(attributesController.curHealth) + "/" +
                Mathf.Floor(attributesController.maxHealth.GetTrueValue());

        _layoutGroupHP.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curHealth / attributesController.maxHealth.GetTrueValue();
    }

    public void UpdateManaBar()
    {
        _layoutGroupMP.transform.Find("TextPanel/ValueText").GetComponent<Text>().text =
            Mathf.Floor(attributesController.curMana) + "/" +
                Mathf.Floor(attributesController.maxMana.GetTrueValue());

        _layoutGroupMP.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curMana / attributesController.maxMana.GetTrueValue();
    }

    public void UpdateBonusBar()
    {
        int i = 0;
        //iterate bonuses
        foreach (KeyValuePair<ConstructorBonus, int> m in GamePlayController.Instance.pickedChampion.manufacturerBonus)
        {
            _layoutGroupManufacturersContent.transform.GetChild(i).gameObject.SetActive(true);
            _layoutGroupManufacturersContent.transform.GetChild(i).gameObject.name = m.Key.name;
            _layoutGroupManufacturersContent.transform.GetChild(i).GetComponent<MFInfo>().Init(m.Key, m.Value, true);
            i++;
        }
        for (int k = i; k < _layoutGroupManufacturersContent.transform.childCount; k++)
        {
            _layoutGroupManufacturersContent.transform.GetChild(k).gameObject.SetActive(false);

        }
        i = 0;
        foreach (KeyValuePair<ConstructorBonus, int> m in GamePlayController.Instance.pickedChampion.championManeger.featureBonus)
        {
            _layoutGroupFeaturesContent.transform.GetChild(i).gameObject.SetActive(true);
            _layoutGroupFeaturesContent.transform.GetChild(i).gameObject.name = m.Key.name;
            _layoutGroupFeaturesContent.transform.GetChild(i).GetComponent<MFInfo>().Init(m.Key, m.Value, true);
            i++;
        }
        for (int k = i; k < _layoutGroupFeaturesContent.transform.childCount; k++)
        {
            _layoutGroupFeaturesContent.transform.GetChild(k).gameObject.SetActive(false);

        }
    }

    public void UpdateAttributeData()
    {
        foreach (var item in championAttributeInfos)
        {
            item.Value.Init(attributesController, item.Key);
        }
        /*
        _layoutGroupAttributesContent.transform.Find("Panel1/moveSpeed").GetComponent<ChampionAttributeInfo>().Init(attributesController.moveSpeed);
        _layoutGroupAttributesContent.transform.Find("Panel1/addRange").GetComponent<ChampionAttributeInfo>().Init(attributesController.addRange);
        _layoutGroupAttributesContent.transform.Find("Panel1/electricPower").GetComponent<ChampionAttributeInfo>().Init(attributesController.electricPower);
        _layoutGroupAttributesContent.transform.Find("Panel1/castDelay").GetComponent<ChampionAttributeInfo>().Init(attributesController.castDelay);
        _layoutGroupAttributesContent.transform.Find("Panel1/chargingDelay").GetComponent<ChampionAttributeInfo>().Init(attributesController.chargingDelay);
        _layoutGroupAttributesContent.transform.Find("Panel1/dodgeChange").GetComponent<ChampionAttributeInfo>().Init(attributesController.dodgeChange);

        _layoutGroupAttributesContent.transform.Find("Panel2/critChange").GetComponent<ChampionAttributeInfo>().Init(attributesController.critChange);
        _layoutGroupAttributesContent.transform.Find("Panel2/critMultiple").GetComponent<ChampionAttributeInfo>().Init(attributesController.critMultiple);
        _layoutGroupAttributesContent.transform.Find("Panel2/armorRegeneration").GetComponent<ChampionAttributeInfo>().Init(attributesController.armorRegeneration);
        _layoutGroupAttributesContent.transform.Find("Panel2/manaRegeneration").GetComponent<ChampionAttributeInfo>().Init(attributesController.manaRegeneration);
        _layoutGroupAttributesContent.transform.Find("Panel2/takeDamageMultiple").GetComponent<ChampionAttributeInfo>().Init(attributesController.takeDamageMultiple);
        _layoutGroupAttributesContent.transform.Find("Panel2/applyDamageMultiple").GetComponent<ChampionAttributeInfo>().Init(attributesController.damageDefenceRate);

        _layoutGroupAttributesContent.transform.Find("Panel3/physicalDamage").GetComponent<ChampionAttributeInfo>().Init(attributesController.physicalDamage);
        _layoutGroupAttributesContent.transform.Find("Panel3/fireDamage").GetComponent<ChampionAttributeInfo>().Init(attributesController.fireDamage);
        _layoutGroupAttributesContent.transform.Find("Panel3/iceDamage").GetComponent<ChampionAttributeInfo>().Init(attributesController.iceDamage);
        _layoutGroupAttributesContent.transform.Find("Panel3/lightingDamage").GetComponent<ChampionAttributeInfo>().Init(attributesController.lightingDamage);
        _layoutGroupAttributesContent.transform.Find("Panel3/acidDamage").GetComponent<ChampionAttributeInfo>().Init(attributesController.acidDamage);

        _layoutGroupAttributesContent.transform.Find("Panel4/physicalDamageApplyRate").GetComponent<ChampionAttributeInfo>().Init(attributesController.physicalDefenceRate);
        _layoutGroupAttributesContent.transform.Find("Panel4/fireDamageApplyRate").GetComponent<ChampionAttributeInfo>().Init(attributesController.fireDefenceRate);
        _layoutGroupAttributesContent.transform.Find("Panel4/iceDamageApplyRate").GetComponent<ChampionAttributeInfo>().Init(attributesController.iceDefenceRate);
        _layoutGroupAttributesContent.transform.Find("Panel4/lightingDamageApplyRate").GetComponent<ChampionAttributeInfo>().Init(attributesController.lightingDefenceRate);
        _layoutGroupAttributesContent.transform.Find("Panel4/acidDamageApplyRate").GetComponent<ChampionAttributeInfo>().Init(attributesController.acidDefenceRate);
        */
    }

    public void UpdateSkillSlot()
    {
        for (int i = 0; i < activatedSkillSlots.Count; i++)
        {
            activatedSkillSlots[i].gameObject.SetActive(false);

            if (i < skillController.activedSkillList.Count)
            {
                activatedSkillSlots[i].gameObject.SetActive(true);
                activatedSkillSlots[i].Init(skillController.activedSkillList[i], true);
            }
        }
        for (int i = 0; i < deactivatedSkillSlots.Count; i++)
        {
            deactivatedSkillSlots[i].gameObject.SetActive(false);
            if (i < skillController.skillList.Count)
            {
                deactivatedSkillSlots[i].gameObject.SetActive(true);
                deactivatedSkillSlots[i].Init(skillController.skillList[i], false);
            }
        }
    }

    public void OnSkillSlotDragEnd(SkillSlot skillSlot)
    {
        if (pointEnterSlot == null)
            return;

        if (skillSlot.isActivated)
        {
            int index1 = activatedSkillSlots.IndexOf(skillSlot);
            if (pointEnterSlot.isActivated)
            {

                int index2 = activatedSkillSlots.IndexOf(pointEnterSlot);
                skillController.SwitchActivedSkill(index1, index2);
            }
            else
            {
                int index2 = deactivatedSkillSlots.IndexOf(pointEnterSlot);
                int index3 = skillController.skillList.IndexOf(pointEnterSlot.skill);

                if (pointEnterSlot.skill.state == SkillState.CD)
                {
                    skillController.RemoveActivedSkill(index1);
                    skillController.SwitchDeactivedSkill(index3, index2);
                }
                else
                {
                    skillController.AddActivedSkill(index1, index2);
                    skillController.SwitchDeactivedSkill(index3, index2);
                }
            }
        }
        else
        {
            int index2 = deactivatedSkillSlots.IndexOf(skillSlot);
            if (pointEnterSlot.isActivated)
            {
                int index1 = activatedSkillSlots.IndexOf(pointEnterSlot);
                if (pointEnterSlot.skill == null)
                {
                    skillController.AddActivedSkill(index1, index2);
                }
                else
                {
                    int index3 = skillController.skillList.IndexOf(pointEnterSlot.skill);

                    skillController.AddActivedSkill(index1, index2);
                    skillController.SwitchDeactivedSkill(index3, index2);
                }

            }
            else
            {
                int index1 = deactivatedSkillSlots.IndexOf(pointEnterSlot);
                skillController.SwitchDeactivedSkill(index1, index2);
            }
        }

        UpdateSkillSlot();
        GeneralMethod.ForceRefreshContentSizeFitterUpwards(skillSlot.transform);
    }
}
