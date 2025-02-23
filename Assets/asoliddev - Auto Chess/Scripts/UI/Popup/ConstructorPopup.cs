using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.EventSystems;

public class ConstructorPopup : Popup
{
    public Image iconImg;
    public Image nameAndTypeImg;
    public Text constructorName;
    public Text typeName;
    public TextPair cost;

    public Transform typeContent;
    public List<SingleTypeSlot> types;
    public Transform attributeContent;
    public List<GameObject> attributeInfo;
    public Transform slotContent;
    public List<GameObject> slotInfo;
    public Transform skillContent;
    public List<GameObject> skillInfo;

    int costValue;

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
        Sprite _icon = Resources.Load<Sprite>(GamePlayController.Instance.GetConstructorIconPath(constructorData));
        iconImg.sprite = _icon;
        Color tempColor1 = GameConfig.Instance.levelColors[constructorData.level - 1];
        nameAndTypeImg.color = tempColor1;
        Color tempColor2 = Color.Lerp(tempColor1, Color.white, .7f); //new Color(tempColor1.r, tempColor1.g, tempColor1.b, tempColor1.a);
        costValue = Mathf.CeilToInt
            (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(constructorData.type).cost *
            GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(constructorData.level).cost);
        constructorName.text = constructorData.name.ToString();
        constructorName.color = tempColor2;
        typeName.text = constructorData.type.ToString();
        typeName.color = tempColor2;
        cost.value.text = costValue.ToString();
        UpdateTypesInfo(constructorData);
        UpdateAttributeInfo(constructorData);
        UpdateSlotInfo(constructorData);
        UpdateSkillInfo(constructorData);
        base.Show(targetUI, dir);
    }

    public void Show(ConstructorBase constructor, GameObject targetUI, Vector3 dir)
    {
        Show(constructor.constructorData, targetUI, dir);
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
                string[] element = constructorData.valueChanges[i].Split(' ');
                attributeInfo[i].transform.Find("Name").GetComponent<Text>().text
                    = element[0] + ":";
                attributeInfo[i].transform.Find("Value").GetComponent<Text>().text
                   = element[1] + element[2];
                attributeInfo[i].SetActive(true);
            }
        }
        attributeContent.gameObject.SetActive(attributeInfo.Count > 0);
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
        slotContent.gameObject.SetActive(slotInfo.Count > 0);
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
                slot.ConstructorPopupInit(data, constructorData);
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
        skillContent.gameObject.SetActive(skillInfo.Count > 0);
    }
}
