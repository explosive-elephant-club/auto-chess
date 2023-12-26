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
    RebuildAllLayout rebuildAllLayout;

    ChampionController championController;
    ChampionAttributesController attributesController;
    SkillController skillController;

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
        rebuildAllLayout = gameObject.GetComponent<RebuildAllLayout>();
    }

    // Update is called once per frame
    public void UpdateUI()
    {
        StartCoroutine(AsyncUpdate());
    }

    IEnumerator AsyncUpdate()
    {
        if (GamePlayController.Instance.ownChampionManager.pickedChampion != null)
        {
            championController = GamePlayController.Instance.ownChampionManager.pickedChampion;
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

    public void OnPointEnterSlot(SkillSlot skillSlot)
    {
        pointEnterSlot = skillSlot;
        if (pointEnterSlot.skill != null)
            UIController.Instance.popupController.skillPopup.Show
            (pointEnterSlot.skill.skillData, pointEnterSlot.gameObject, Vector3.right);
    }

    public void OnPointLeaveSlot()
    {
        pointEnterSlot = null;
        UIController.Instance.popupController.skillPopup.Clear();
    }

    public void OnEnterCombat()
    {
        transform.Find("Panel/ActivatedSkill").GetComponent<CanvasGroup>().blocksRaycasts = false;
        transform.Find("Panel/DeactivatedSkill").gameObject.SetActive(false);
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
        transform.Find("Panel/ActivatedSkill").GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.Find("Panel/DeactivatedSkill").gameObject.SetActive(true);
    }

    public void UpdateArmorBar()
    {

        armorBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            Mathf.Floor(attributesController.curArmor) + "/" +
                Mathf.Floor(attributesController.maxArmor.GetTrueLinearValue());

        armorBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curArmor / attributesController.maxArmor.GetTrueLinearValue();
    }

    public void UpdateMechBar()
    {
        mechBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            Mathf.Floor(attributesController.curHealth) + "/" +
                Mathf.Floor(attributesController.maxHealth.GetTrueLinearValue());

        mechBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curHealth / attributesController.maxHealth.GetTrueLinearValue();
    }

    public void UpdateManaBar()
    {
        manaBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            Mathf.Floor(attributesController.curMana) + "/" +
                Mathf.Floor(attributesController.maxMana.GetTrueLinearValue());

        manaBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curMana / attributesController.maxMana.GetTrueLinearValue();
    }

    public void UpdateTypesBar()
    {
        int i = 0;
        //iterate bonuses
        foreach (KeyValuePair<ConstructorBonusType, int> m in GamePlayController.Instance.ownChampionManager.pickedChampion.constructorTypeCount)
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
        state2.transform.Find("Panel1/moveSpeed").GetComponent<AttributeSlot>().Init(attributesController.moveSpeed, null);
        state2.transform.Find("Panel1/addRange").GetComponent<AttributeSlot>().Init(attributesController.addRange, null);
        state2.transform.Find("Panel1/electricPower").GetComponent<AttributeSlot>().Init(attributesController.electricPower, null);
        state2.transform.Find("Panel1/castDelay").GetComponent<AttributeSlot>().Init(attributesController.castDelay, attributesController.castDelayDecr);
        state2.transform.Find("Panel1/chargingDelay").GetComponent<AttributeSlot>().Init(attributesController.chargingDelay, attributesController.chargingDelayDecr);
        state2.transform.Find("Panel1/dodgeChange").GetComponent<AttributeSlot>().Init(attributesController.dodgeChange, null, false);

        state2.transform.Find("Panel2/critChange").GetComponent<AttributeSlot>().Init(attributesController.critChange, null, false);
        state2.transform.Find("Panel2/critMultiple").GetComponent<AttributeSlot>().Init(attributesController.critMultiple, null, false);
        state2.transform.Find("Panel2/armorRegeneration").GetComponent<AttributeSlot>().Init(attributesController.armorRegeneration, null);
        state2.transform.Find("Panel2/manaRegeneration").GetComponent<AttributeSlot>().Init(attributesController.manaRegeneration, null);
        state2.transform.Find("Panel2/takeDamageMultiple").GetComponent<AttributeSlot>().Init(attributesController.takeDamageMultiple, null, false);
        state2.transform.Find("Panel2/applyDamageMultiple").GetComponent<AttributeSlot>().Init(attributesController.applyDamageMultiple, null, false);

        state2.transform.Find("Panel3/physicalDamage").GetComponent<AttributeSlot>().Init(attributesController.physicalDamage, null);
        state2.transform.Find("Panel3/fireDamage").GetComponent<AttributeSlot>().Init(attributesController.fireDamage, null);
        state2.transform.Find("Panel3/iceDamage").GetComponent<AttributeSlot>().Init(attributesController.iceDamage, null);
        state2.transform.Find("Panel3/lightingDamage").GetComponent<AttributeSlot>().Init(attributesController.lightingDamage, null);
        state2.transform.Find("Panel3/acidDamage").GetComponent<AttributeSlot>().Init(attributesController.acidDamage, null);

        state2.transform.Find("Panel4/physicalDefenseRate").GetComponent<AttributeSlot>().Init(attributesController.physicalDefenseRate, null, false);
        state2.transform.Find("Panel4/fireDefenseRate").GetComponent<AttributeSlot>().Init(attributesController.fireDefenseRate, null, false);
        state2.transform.Find("Panel4/iceDefenseRate").GetComponent<AttributeSlot>().Init(attributesController.iceDefenseRate, null, false);
        state2.transform.Find("Panel4/lightingDefenseRate").GetComponent<AttributeSlot>().Init(attributesController.lightingDefenseRate, null, false);
        state2.transform.Find("Panel4/acidDefenseRate").GetComponent<AttributeSlot>().Init(attributesController.acidDefenseRate, null, false);
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
