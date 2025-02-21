using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;
using System.Diagnostics;

public class ChampionInfoController : BaseControllerUI
{
    //State1
    public GameObject armorBar;
    public GameObject mechBar;
    public GameObject manaBar;

    //types
    public GameObject typesBar;


    //State2
    public GameObject state2;

    public GameObject activatedSkillSlotContent;
    public GameObject deactivatedSkillSlotContent;

    //Skill
    public List<SkillSlot> activatedSkillSlots = new List<SkillSlot>();
    public List<SkillSlot> deactivatedSkillSlots = new List<SkillSlot>();

    public SkillSlot pointEnterSlot;

    RebuildAllLayout rebuildAllLayout;

    ChampionController championController;
    ChampionAttributesController attributesController;
    SkillController skillController;

    void Awake()
    {
        Init();
        foreach (Transform child in activatedSkillSlotContent.transform)
        {
            activatedSkillSlots.Add(child.gameObject.GetComponent<SkillSlot>());
        }
        foreach (Transform child in deactivatedSkillSlotContent.transform)
        {
            deactivatedSkillSlots.Add(child.gameObject.GetComponent<SkillSlot>());
        }
        rebuildAllLayout = gameObject.GetComponent<RebuildAllLayout>();
    }

    // Update is called once per frame
    public override void UpdateUI()
    {
        StartCoroutine(AsyncUpdate());
    }

    IEnumerator AsyncUpdate()
    {
        if (GamePlayController.Instance.pickedChampion != null)
        {
            championController = GamePlayController.Instance.pickedChampion;
            attributesController = championController.attributesController;
            skillController = championController.skillController;

            UpdateArmorBar();
            UpdateMechBar();
            UpdateManaBar();
            UpdateTypesBar();
            UpdateAttributeData();
            UpdateSkillSlot();
            yield return StartCoroutine(rebuildAllLayout.RebuildAllSizeFitterRects());
            //gameObject.SendMessage("RebuildAll");
            SetUIActive(true);
        }
        else
        {
            StopAllCoroutines();
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
        activatedSkillSlotContent.GetComponentInParent<CanvasGroup>().blocksRaycasts = false;
        deactivatedSkillSlotContent.gameObject.SetActive(false);
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

    public void OnEnterPreparation()
    {
        activatedSkillSlotContent.GetComponentInParent<CanvasGroup>().blocksRaycasts = true;
        deactivatedSkillSlotContent.gameObject.SetActive(true);
    }

    public void UpdateArmorBar()
    {

        armorBar.transform.Find("Slider/ValueText").GetComponent<TextMeshProUGUI>().text =
            Mathf.Floor(attributesController.curArmor) + "/" +
                Mathf.Floor(attributesController.maxArmor.GetTrueValue());

        armorBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curArmor / attributesController.maxArmor.GetTrueValue();
    }

    public void UpdateMechBar()
    {
        mechBar.transform.Find("Slider/ValueText").GetComponent<TextMeshProUGUI>().text =
            Mathf.Floor(attributesController.curHealth) + "/" +
                Mathf.Floor(attributesController.maxHealth.GetTrueValue());

        mechBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curHealth / attributesController.maxHealth.GetTrueValue();
    }

    public void UpdateManaBar()
    {
        manaBar.transform.Find("Slider/ValueText").GetComponent<TextMeshProUGUI>().text =
            Mathf.Floor(attributesController.curMana) + "/" +
                Mathf.Floor(attributesController.maxMana.GetTrueValue());

        manaBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curMana / attributesController.maxMana.GetTrueValue();
    }

    public void UpdateTypesBar()
    {
        int i = 0;
        //iterate bonuses
        foreach (KeyValuePair<ConstructorBonusType, int> m in GamePlayController.Instance.pickedChampion.bonus)
        {
            typesBar.transform.GetChild(i).gameObject.SetActive(true);
            typesBar.transform.GetChild(i).gameObject.name = m.Key.name;
            typesBar.transform.GetChild(i).GetComponent<TypeSlot>().Init(m.Key, m.Value, true);
            i++;
        }
        for (int k = i; k < typesBar.transform.childCount; k++)
        {
            typesBar.transform.GetChild(k).gameObject.SetActive(false);

        }
    }

    public void UpdateAttributeData()
    {
        state2.transform.Find("Panel1/moveSpeed").GetComponent<AttributeSlot>().Init("MoveSpeed", attributesController.moveSpeed.GetTrueValue());
        state2.transform.Find("Panel1/addRange").GetComponent<AttributeSlot>().Init("AddRange", attributesController.addRange.GetTrueValue());
        state2.transform.Find("Panel1/electricPower").GetComponent<AttributeSlot>().Init("ElectricPower", attributesController.electricPower.GetTrueValue());
        state2.transform.Find("Panel1/castDelay").GetComponent<AttributeSlot>().Init("CastDelay", attributesController.castDelay.GetTrueValue());
        state2.transform.Find("Panel1/chargingDelay").GetComponent<AttributeSlot>().Init("ChargingDelay", attributesController.chargingDelay.GetTrueValue());
        state2.transform.Find("Panel1/dodgeChange").GetComponent<AttributeSlot>().Init("DodgeChange", 1 - attributesController.hitRate.GetTrueValue(), false);

        state2.transform.Find("Panel2/critChange").GetComponent<AttributeSlot>().Init("CritChange", 1 - attributesController.nonCritChange.GetTrueValue(), false);
        state2.transform.Find("Panel2/critMultiple").GetComponent<AttributeSlot>().Init("CritMultiple", attributesController.critMultiple.GetTrueValue());
        state2.transform.Find("Panel2/armorRegeneration").GetComponent<AttributeSlot>().Init("ArmorRegeneration", attributesController.armorRegeneration.GetTrueValue());
        state2.transform.Find("Panel2/manaRegeneration").GetComponent<AttributeSlot>().Init("ManaRegeneration", attributesController.manaRegeneration.GetTrueValue());
        state2.transform.Find("Panel2/takeDamageMultiple").GetComponent<AttributeSlot>().Init("TakeDamageMultiple", attributesController.takeDamageMultiple.GetTrueValue(), false);
        state2.transform.Find("Panel2/applyDamageMultiple").GetComponent<AttributeSlot>().Init("DamageDefenceRate", 1 - attributesController.applyDamageMultiple.GetTrueValue(), false);

        state2.transform.Find("Panel3/physicalDamage").GetComponent<AttributeSlot>().Init("PhysicalDamage", attributesController.physicalDamage.GetTrueValue());
        state2.transform.Find("Panel3/fireDamage").GetComponent<AttributeSlot>().Init("FireDamage", attributesController.fireDamage.GetTrueValue());
        state2.transform.Find("Panel3/iceDamage").GetComponent<AttributeSlot>().Init("IceDamage", attributesController.iceDamage.GetTrueValue());
        state2.transform.Find("Panel3/lightingDamage").GetComponent<AttributeSlot>().Init("LightingDamage", attributesController.lightingDamage.GetTrueValue());
        state2.transform.Find("Panel3/acidDamage").GetComponent<AttributeSlot>().Init("AcidDamage", attributesController.acidDamage.GetTrueValue());

        state2.transform.Find("Panel4/physicalDamageApplyRate").GetComponent<AttributeSlot>().Init("PhysicalDefenceRate", 1 - attributesController.physicalDamageApplyRate.GetTrueValue(), false);
        state2.transform.Find("Panel4/fireDamageApplyRate").GetComponent<AttributeSlot>().Init("FireDefenceRate", 1 - attributesController.fireDamageApplyRate.GetTrueValue(), false);
        state2.transform.Find("Panel4/iceDamageApplyRate").GetComponent<AttributeSlot>().Init("IceDefenceRate", 1 - attributesController.iceDamageApplyRate.GetTrueValue(), false);
        state2.transform.Find("Panel4/lightingDamageApplyRate").GetComponent<AttributeSlot>().Init("LightingDefenceRate", 1 - attributesController.lightingDamageApplyRate.GetTrueValue(), false);
        state2.transform.Find("Panel4/acidDamageApplyRate").GetComponent<AttributeSlot>().Init("AcidDefenceRate", 1 - attributesController.acidDamageApplyRate.GetTrueValue(), false);
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
        gameObject.SendMessage("RebuildAll");
    }
}
