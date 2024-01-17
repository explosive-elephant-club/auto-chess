using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.EventSystems;

public class ConstructorPopup : Popup
{
    public TextMeshProUGUI constructorName;
    public TextMeshProUGUI typeName;
    public TextPair cost;

    public Transform typeContent;
    public List<SingleTypeSlot> types;
    public Transform attributeContent;
    public List<GameObject> attributeInfo;
    public Transform slotContent;
    public List<GameObject> slotInfo;
    public Transform skillContent;
    public List<GameObject> skillInfo;

    void Start()
    {
        foreach (Transform child in typeContent)
        {
            types.Add(child.GetComponent<SingleTypeSlot>());
        }
        foreach (Transform child in attributeContent)
        {
            attributeInfo.Add(child.gameObject);
        }
        foreach (Transform child in slotContent)
        {
            slotInfo.Add(child.gameObject);
        }
        foreach (Transform child in skillContent)
        {
            skillInfo.Add(child.gameObject);
        }
    }

    public void Show(ConstructorBaseData constructorData, GameObject targetUI, Vector3 dir)
    {
        constructorName.text = constructorData.name.ToString();
        typeName.text = constructorData.type.ToString();
        cost.value.text = constructorData.cost.ToString();
        UpdateTypesInfo(constructorData);
        UpdateAttributeInfo(constructorData);
        UpdateSlotInfo(constructorData);
        UpdateSkillInfo(constructorData);
        base.Show(targetUI, dir);
    }

    public void Show(ConstructorBase constructor, GameObject targetUI, Vector3 dir)
    {
        constructorName.text = constructor.constructorData.name.ToString();
        typeName.text = constructor.constructorData.type.ToString();
        cost.value.text = constructor.constructorData.cost.ToString();
        UpdateTypesInfo(constructor.constructorData);
        UpdateAttributeInfo(constructor.constructorData);
        UpdateSlotInfo(constructor);
        UpdateSkillInfo(constructor.constructorData);
        base.Show(targetUI, dir);
    }

    void UpdateTypesInfo(ConstructorBaseData constructorData)
    {
        List<ConstructorBonusType> bonus = GamePlayController.Instance.GetAllChampionTypes(constructorData);
        for (int i = 0; i < types.Count; i++)
        {
            types[i].gameObject.SetActive(false);
            if (i < bonus.Count)
            {
                if (bonus[i] != null)
                {
                    types[i].gameObject.SetActive(true);
                    types[i].Init(bonus[i]);
                }
            }
        }
    }

    void UpdateAttributeInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < attributeInfo.Count; i++)
        {
            attributeInfo[i].SetActive(false);
            if (i < constructorData.valueChanges.Length && !string.IsNullOrEmpty(constructorData.valueChanges[0]))
            {
                attributeInfo[i].GetComponent<TextMeshProUGUI>().text = constructorData.valueChanges[i].Replace(" ", "");
                attributeInfo[i].SetActive(true);
            }
        }
    }

    void UpdateSlotInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < slotInfo.Count; i++)
        {
            slotInfo[i].SetActive(false);
            if (i < constructorData.slots.Length && constructorData.slots[0] != 0)
            {
                slotInfo[i].GetComponent<ConstructorSlotSlot>().Init(constructorData.slots[i]);
                slotInfo[i].SetActive(true);
            }
        }
    }
    void UpdateSlotInfo(ConstructorBase constructor)
    {
        for (int i = 0; i < slotInfo.Count; i++)
        {
            slotInfo[i].SetActive(false);
            if (i < constructor.slots.Count)
            {
                slotInfo[i].GetComponent<ConstructorSlotSlot>().Init(constructor.slots[i]);
                slotInfo[i].SetActive(true);
            }
        }
    }


    void UpdateSkillInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < skillInfo.Count; i++)
        {
            skillInfo[i].SetActive(false);
            if (i < constructorData.skillID.Length && constructorData.skillID[0] != 0)
            {
                SkillData data = GameExcelConfig.Instance.skillDatasArray.Find(d => d.ID == constructorData.skillID[i]);
                skillInfo[i].SetActive(true);
                SkillSlot slot = skillInfo[i].GetComponent<SkillSlot>();
                slot.PopupShow(data);
                slot.onPointerEnterEvent.AddListener((PointerEventData eventData) =>
                {
                    UIController.Instance.popupController.skillPopup.Show
                        (data, this.gameObject, Vector3.right);
                });
                slot.onPointerExitEvent.AddListener((PointerEventData eventData) =>
                {
                    UIController.Instance.popupController.skillPopup.Clear();
                });
            }
        }
    }
}
