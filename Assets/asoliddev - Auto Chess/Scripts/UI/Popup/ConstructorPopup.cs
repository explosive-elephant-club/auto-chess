using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.EventSystems;
using Game;

public class ConstructorPopup : Popup
{
    public List<SingleMFInfo> singleMFInfoList;
    public List<AttributeInfo> attributeInfoList;
    public List<SlotInfo> slotInfoList;
    public List<SkillSlot> skillSlotList;

    int costValue;

    #region 自动绑定
    private Image _imgNameAndTypeBG;
    private Image _imgIconImage;
    private Image _imgAttribute;
    private UICustomText _textNameText;
    private UICustomText _textTypeText;
    private UICustomText _textCostValue;
    private GridLayoutGroup _layoutGroupMFContent;
    private GridLayoutGroup _layoutGroupAttribute;
    private GridLayoutGroup _layoutGroupSlotContent;
    private GridLayoutGroup _layoutGroupSkillContent;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _imgNameAndTypeBG = transform.Find("NameAndTypeBG_Auto").GetComponent<Image>();
        _imgIconImage = transform.Find("IconAndFeature/IconImage_Auto").GetComponent<Image>();
        _imgAttribute = transform.Find("Attribute_Auto").GetComponent<Image>();
        _textNameText = transform.Find("NameAndTypeBG_Auto/NameText_Auto").GetComponent<UICustomText>();
        _textTypeText = transform.Find("NameAndTypeBG_Auto/TypeText_Auto").GetComponent<UICustomText>();
        _textCostValue = transform.Find("IconAndFeature/Cost/CostValue_Auto").GetComponent<UICustomText>();
        _layoutGroupMFContent = transform.Find("NameAndTypeBG_Auto/MFContent_Auto").GetComponent<GridLayoutGroup>();
        _layoutGroupAttribute = transform.Find("Attribute_Auto").GetComponent<GridLayoutGroup>();
        _layoutGroupSlotContent = transform.Find("Slot/SlotContent_Auto").GetComponent<GridLayoutGroup>();
        _layoutGroupSkillContent = transform.Find("Skill/SkillContent_Auto").GetComponent<GridLayoutGroup>();
    }
    #endregion



    void Start()
    {
        foreach (Transform child in _layoutGroupMFContent.transform)
        {
            singleMFInfoList.Add(child.GetComponent<SingleMFInfo>());
        }
        foreach (Transform child in _layoutGroupAttribute.transform)
        {
            attributeInfoList.Add(child.GetComponent<AttributeInfo>());
        }
        foreach (Transform child in _layoutGroupSlotContent.transform)
        {
            slotInfoList.Add(child.GetComponent<SlotInfo>());
        }
        foreach (Transform child in _layoutGroupSkillContent.transform)
        {
            skillSlotList.Add(child.GetComponent<SkillSlot>());
        }
    }

    public void Show(ConstructorBaseData constructorData, GameObject targetUI, Vector3 dir)
    {
        Sprite _icon = Resources.Load<Sprite>(GamePlayController.Instance.GetConstructorIconPath(constructorData));
        _imgIconImage.sprite = _icon;
        Color tempColor1 = GameConfig.Instance.levelColors[constructorData.level - 1];
        _imgNameAndTypeBG.color = tempColor1;
        Color tempColor2 = Color.Lerp(tempColor1, Color.white, .7f); //new Color(tempColor1.r, tempColor1.g, tempColor1.b, tempColor1.a);
        costValue = Mathf.CeilToInt
            (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(constructorData.type).cost *
            GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(constructorData.level).cost);
        _textNameText.text = constructorData.name.ToString();
        _textNameText.color = tempColor2;
        _textTypeText.text = constructorData.type.ToString();
        _textTypeText.color = tempColor2;
        _textCostValue.text = costValue.ToString();
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
        for (int i = 0; i < singleMFInfoList.Count; i++)
        {
            singleMFInfoList[i].gameObject.SetActive(false);
            if (i < bonus.Count)
            {
                if (bonus[i] != null)
                {
                    singleMFInfoList[i].gameObject.SetActive(true);
                    singleMFInfoList[i].Init(bonus[i]);
                }
            }
        }
    }

    void UpdateAttributeInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < attributeInfoList.Count; i++)
        {
            attributeInfoList[i].SetUIActive(false);
            if (i < constructorData.valueChanges.Length && !string.IsNullOrEmpty(constructorData.valueChanges[0]))
            {
                string[] element = constructorData.valueChanges[i].Split(' ');
                attributeInfoList[i].Init(element[0] + ":", element[1] + element[2]);
                attributeInfoList[i].SetUIActive(true);
            }
        }
        _layoutGroupAttribute.gameObject.SetActive(attributeInfoList.Count > 0);
    }

    void UpdateSlotInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < slotInfoList.Count; i++)
        {
            slotInfoList[i].SetUIActive(false);
            if (i < constructorData.slots.Length && constructorData.slots[0] != 0)
            {
                slotInfoList[i].Init(constructorData.slots[i]);
                slotInfoList[i].SetUIActive(true);
            }
        }
        _layoutGroupSlotContent.gameObject.SetActive(slotInfoList.Count > 0);
    }

    void UpdateSkillInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < skillSlotList.Count; i++)
        {
            skillSlotList[i].SetUIActive(false);
            if (i < constructorData.skillID.Length && constructorData.skillID[0] != 0)
            {
                SkillData data = GameExcelConfig.Instance.skillDatasArray.Find(d => d.ID == constructorData.skillID[i]);
                skillSlotList[i].SetUIActive(true);
                skillSlotList[i].ConstructorPopupInit(data, constructorData);
                skillSlotList[i].onPointerEnterEvent.AddListener((PointerEventData eventData) =>
                {
                    UIController.Instance.popupController.skillPopup.Show
                        (data, this.gameObject, Vector3.right);
                });
                skillSlotList[i].onPointerExitEvent.AddListener((PointerEventData eventData) =>
                {
                    UIController.Instance.popupController.skillPopup.Clear();
                });
            }
        }
        _layoutGroupSkillContent.gameObject.SetActive(skillSlotList.Count > 0);
    }
}
