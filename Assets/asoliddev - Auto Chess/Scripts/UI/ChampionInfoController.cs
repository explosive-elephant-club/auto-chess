using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;

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
    public GameObject[] activatedSkillSlots;
    public GameObject[] deactivatedSkillSlots;




    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateArmorBar()
    {
        ChampionAttributesController attributesController =
            GamePlayController.Instance.ownChampionManager.draggedChampion.attributesController;

        armorBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            attributesController.curArmor + "/" + attributesController.maxArmor.GetTrueLinearValue();

        armorBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curArmor / attributesController.maxArmor.GetTrueLinearValue();
    }

    public void UpdateMechBar()
    {
        ChampionAttributesController attributesController =
            GamePlayController.Instance.ownChampionManager.draggedChampion.attributesController;

        manaBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            attributesController.curMana + "/" + attributesController.maxMana.GetTrueLinearValue();

        manaBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curMana / attributesController.maxMana.GetTrueLinearValue();
    }

    public void UpdateManaBar()
    {
        ChampionAttributesController attributesController =
            GamePlayController.Instance.ownChampionManager.draggedChampion.attributesController;

        armorBar.transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>().text =
            attributesController.curArmor + "/" + attributesController.maxArmor.GetTrueLinearValue();

        armorBar.transform.Find("Slider").GetComponent<Slider>().value =
            attributesController.curArmor / attributesController.maxArmor.GetTrueLinearValue();
    }

    public void UpdateTypesBar()
    {
        int i = 0;
        //iterate bonuses
        foreach (KeyValuePair<ConstructorBonusType, int> m in GamePlayController.Instance.ownChampionManager.draggedChampion.constructorTypeCount)
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
        ChampionAttributesController attributesController =
           GamePlayController.Instance.ownChampionManager.draggedChampion.attributesController;

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
}
