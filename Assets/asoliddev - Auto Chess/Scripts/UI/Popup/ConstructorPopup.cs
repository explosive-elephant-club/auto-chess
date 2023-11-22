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
    public TextPair cost;

    public Transform typeContent;
    public List<GameObject> typeIcons;
    public Transform attributeContent;
    public List<GameObject> attributeInfo;
    public Transform skillContent;
    public List<GameObject> skillInfo;

    void Start()
    {
        foreach (Transform child in typeContent)
        {
            typeIcons.Add(child.gameObject);
        }
        foreach (Transform child in attributeContent)
        {
            attributeInfo.Add(child.gameObject);
        }
        foreach (Transform child in skillContent)
        {
            skillInfo.Add(child.gameObject);
        }
    }

    public void Show(ConstructorBaseData constructorData, Vector3 slotPositon, float length, Vector3 dir)
    {
        constructorName.text = constructorData.name.ToString();
        cost.value.text = constructorData.cost.ToString();
        UpdateTypesInfo(constructorData);
        UpdateAttributeInfo(constructorData);
        UpdateSkillInfo(constructorData);
        base.Show(slotPositon, length, dir);
    }

    void UpdateTypesInfo(ConstructorBaseData constructorData)
    {
        List<ConstructorBonusType> bonus = GamePlayController.Instance.GetAllChampionTypes(constructorData);
        for (int i = 0; i < typeIcons.Count; i++)
        {
            typeIcons[i].SetActive(false);
            if (i < bonus.Count)
            {
                if (bonus[i] != null)
                {
                    typeIcons[i].GetComponent<Image>().sprite = Resources.Load<Sprite>(bonus[i].icon);
                    typeIcons[i].SetActive(true);
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


    void UpdateSkillInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < skillInfo.Count; i++)
        {
            skillInfo[i].SetActive(false);
            if (i < constructorData.skillID.Length && constructorData.skillID[0] != 0)
            {
                SkillData data = GameData.Instance.skillDatasArray.Find(d => d.ID == constructorData.skillID[i]);
                skillInfo[i].SetActive(true);
                SkillSlot slot = skillInfo[i].GetComponent<SkillSlot>();
                slot.PopupShow(data);
                slot.onPointerEnterEvent.AddListener((PointerEventData eventData) =>
                {
                    UIController.Instance.popupController.skillPopup.Show
                        (data, slot.transform.position, this.GetComponent<RectTransform>().rect.height, Vector3.up);
                });
                slot.onPointerExitEvent.AddListener((PointerEventData eventData) =>
                {
                    UIController.Instance.popupController.skillPopup.Clear();
                });



            }
        }
    }
}
