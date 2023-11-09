using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;
using System.Diagnostics;

public class ChampionInfoController : MonoBehaviour
{
    //State1
    public GameObject armorBar;
    public GameObject mechBar;
    public GameObject manaBar;

    //types
    public GameObject typesBar;


    //State2
    public GameObject state2;



    //Skill
    public List<SkillSlot> activatedSkillSlots = new List<SkillSlot>();
    public List<SkillSlot> deactivatedSkillSlots = new List<SkillSlot>();

    public SkillSlot pointEnterSlot;

    CanvasGroup canvasGroup;

    ChampionController championController;
    ChampionAttributesController attributesController;
    SkillController skillController;


    // Start is called before the first frame update
    void Awake()
    {
        foreach (Transform child in transform.Find("Panel/ActivatedSkill/SkillSlot"))
        {
            activatedSkillSlots.Add(child.gameObject.GetComponent<SkillSlot>());
        }
        foreach (Transform child in transform.Find("Panel/DeactivatedSkill/SkillSlot"))
        {
            deactivatedSkillSlots.Add(child.gameObject.GetComponent<SkillSlot>());
        }
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    public void UpdateUI()
    {
        if (GamePlayController.Instance.ownChampionManager.pickedChampion != null)
        {
            championController = GamePlayController.Instance.ownChampionManager.pickedChampion;
            attributesController = championController.attributesController;
            skillController = championController.skillController;

            SetUIActive(true);
            UpdateArmorBar();
            UpdateMechBar();
            UpdateManaBar();
            UpdateTypesBar();
            UpdateAttributeData();
            UpdateSkillSlot();
        }
        else
        {
            SetUIActive(false);
        }
    }

    public void SetUIActive(bool isActive)
    {
        if (isActive)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

    }

    public void OnEnterCombat()
    {
        transform.Find("Panel/ActivatedSkill").GetComponent<CanvasGroup>().blocksRaycasts = false;
        transform.Find("Panel/DeactivatedSkill").gameObject.SetActive(false);
    }

    public void OnEnterPreparation()
    {
        transform.Find("Panel/ActivatedSkill").GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.Find("Panel/DeactivatedSkill").gameObject.SetActive(true);
    }

    public void UpdateArmorBar()
    {
        armorBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            attributesController.curArmor + "/" + attributesController.maxArmor.GetTrueLinearValue();

        armorBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curArmor / attributesController.maxArmor.GetTrueLinearValue();
    }

    public void UpdateMechBar()
    {
        manaBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            attributesController.curMana + "/" + attributesController.maxMana.GetTrueLinearValue();

        manaBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curMana / attributesController.maxMana.GetTrueLinearValue();
    }

    public void UpdateManaBar()
    {
        armorBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            attributesController.curArmor + "/" + attributesController.maxArmor.GetTrueLinearValue();

        armorBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curArmor / attributesController.maxArmor.GetTrueLinearValue();
    }

    public void UpdateTypesBar()
    {
        int i = 0;
        //iterate bonuses
        foreach (KeyValuePair<ConstructorBonusType, int> m in GamePlayController.Instance.ownChampionManager.pickedChampion.constructorTypeCount)
        {
            typesBar.transform.GetChild(i).gameObject.SetActive(true);
            typesBar.transform.GetChild(i).gameObject.name = m.Key.name;
            typesBar.transform.GetChild(i).GetComponent<TypeSlot>().UpdateUI(m.Key, m.Value);
            i++;
        }
        for (int k = i; k < typesBar.transform.childCount; k++)
        {
            typesBar.transform.GetChild(k).gameObject.SetActive(false);

        }
    }

    public void UpdateAttributeData()
    {
        state2.transform.Find("Panel1/moveSpeed/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.moveSpeed.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel1/addRange/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.addRange.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel1/electricPower/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.electricPower.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel1/castDelay/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.castDelay.GetTrueLinearValue() * (1 - attributesController.castDelayDecr.GetTrueMultipleValue())).ToString();
        state2.transform.Find("Panel1/chargingDelay/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.chargingDelay.GetTrueLinearValue() * (1 - attributesController.chargingDelayDecr.GetTrueMultipleValue())).ToString();
        state2.transform.Find("Panel1/dodgeChange/Text_Value").GetComponent<TextMeshProUGUI>().text =
                    (attributesController.dodgeChange.GetTrueMultipleValue() * 100f).ToString() + "%";

        state2.transform.Find("Panel2/critChange/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.critChange.GetTrueMultipleValue() * 100f).ToString() + "%";
        state2.transform.Find("Panel2/critMultiple/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.critMultiple.GetTrueLinearValue() * 100f).ToString() + "%";
        state2.transform.Find("Panel2/armorRegeneration/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.armorRegeneration.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel2/manaRegeneration/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.manaRegeneration.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel2/takeDamageMultiple/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.takeDamageMultiple.GetTrueLinearValue() * 100f).ToString() + "%";
        state2.transform.Find("Panel2/applyDamageMultiple/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.applyDamageMultiple.GetTrueLinearValue() * 100f).ToString() + "%";

        state2.transform.Find("Panel3/physicalDamage/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.physicalDamage.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel3/fireDamage/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.fireDamage.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel3/iceDamage/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.iceDamage.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel3/lightingDamage/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.lightingDamage.GetTrueLinearValue().ToString();
        state2.transform.Find("Panel3/acidDamage/Text_Value").GetComponent<TextMeshProUGUI>().text =
            attributesController.acidDamage.GetTrueLinearValue().ToString();

        state2.transform.Find("Panel4/physicalDefenseRate/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.physicalDefenseRate.GetTrueMultipleValue() * 100f).ToString() + "%";
        state2.transform.Find("Panel4/fireDefenseRate/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.fireDefenseRate.GetTrueMultipleValue() * 100f).ToString() + "%";
        state2.transform.Find("Panel4/iceDefenseRate/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.iceDefenseRate.GetTrueMultipleValue() * 100f).ToString() + "%";
        state2.transform.Find("Panel4/lightingDefenseRate/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.lightingDefenseRate.GetTrueMultipleValue() * 100f).ToString() + "%";
        state2.transform.Find("Panel4/acidDefenseRate/Text_Value").GetComponent<TextMeshProUGUI>().text =
            (attributesController.acidDefenseRate.GetTrueMultipleValue() * 100f).ToString() + "%";

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
    }
}
