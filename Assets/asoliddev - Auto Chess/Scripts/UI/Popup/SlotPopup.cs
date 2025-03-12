using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using Game;


public class SlotPopup : Popup
{
    public List<TypeInfo> adaptTypes;
    public List<SlotInfo> forbiddenChildrenTypes;
    public List<SlotInfo> forbiddenParentsTypes;

    ConstructorSlotType slotType;


    #region 自动绑定
    private Image _imgIcon;
    private UICustomText _textNameText;
    private UICustomText _textFCSText;
    private UICustomText _textFPSText;
    private VerticalLayoutGroup _layoutGroupAdaptContent;
    private GridLayoutGroup _layoutGroupFCSContent;
    private GridLayoutGroup _layoutGroupFPSContent;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _imgIcon = transform.Find("NameAndIcon/Icon_Auto").GetComponent<Image>();
        _textNameText = transform.Find("NameAndIcon/NameText_Auto").GetComponent<UICustomText>();
        _textFCSText = transform.Find("ForbiddenChildrenSlot/FCSText_Auto").GetComponent<UICustomText>();
        _textFPSText = transform.Find("ForbiddenParentsSlot/FPSText_Auto").GetComponent<UICustomText>();
        _layoutGroupAdaptContent = transform.Find("Adapt/AdaptContent_Auto").GetComponent<VerticalLayoutGroup>();
        _layoutGroupFCSContent = transform.Find("ForbiddenChildrenSlot/FCSContent_Auto").GetComponent<GridLayoutGroup>();
        _layoutGroupFPSContent = transform.Find("ForbiddenParentsSlot/FPSContent_Auto").GetComponent<GridLayoutGroup>();
    }
    #endregion


    void Start()
    {
        foreach (Transform child in _layoutGroupAdaptContent.transform)
        {
            adaptTypes.Add(child.GetComponentInChildren<TypeInfo>());
        }
        foreach (Transform child in _layoutGroupFCSContent.transform)
        {
            forbiddenChildrenTypes.Add(child.GetComponentInChildren<SlotInfo>());
        }
        foreach (Transform child in _layoutGroupFPSContent.transform)
        {
            forbiddenParentsTypes.Add(child.GetComponentInChildren<SlotInfo>());
        }
    }

    public void Show(ConstructorSlotType _slotType, GameObject targetUI, Vector3 dir)
    {
        slotType = _slotType;
        _textNameText.text = slotType.name.ToString();
        _imgIcon.sprite = ResourceManager.LoadResource<Sprite>(slotType.icon);
        UpdateAdaptTypes();
        UpdateForbiddenChildrenTypes();
        UpdateForbiddenParentsTypes();
        base.Show(targetUI, dir);
    }


    void UpdateAdaptTypes()
    {
        if (!string.IsNullOrEmpty(slotType.adaptTypes[0]))
        {
            _layoutGroupAdaptContent.transform.parent.gameObject.SetActive(true);
            for (int i = 0; i < adaptTypes.Count; i++)
            {
                adaptTypes[i].SetUIActive(false);
                if (i < slotType.adaptTypes.Length)
                {
                    adaptTypes[i].Init(slotType.adaptTypes[i]);
                    adaptTypes[i].SetUIActive(true);
                }
            }
        }
        else
        {
            _layoutGroupAdaptContent.transform.parent.gameObject.SetActive(false);
        }
    }

    void UpdateForbiddenChildrenTypes()
    {
        if (slotType.isForbiddenAllChildrenSlots)
        {
            _textFCSText.gameObject.SetActive(true);
            _layoutGroupFCSContent.gameObject.SetActive(false);
            return;
        }
        else
        {
            _textFCSText.gameObject.SetActive(false);
            _layoutGroupFCSContent.gameObject.SetActive(true);
        }

        if (slotType.forbiddenChildrenSlotTypes[0] != 0)
        {
            _layoutGroupFCSContent.transform.parent.gameObject.SetActive(true);
            for (int i = 0; i < forbiddenChildrenTypes.Count; i++)
            {
                forbiddenChildrenTypes[i].SetUIActive(false);
                if (i < slotType.forbiddenChildrenSlotTypes.Length && slotType.forbiddenChildrenSlotTypes[0] != 0)
                {
                    ConstructorSlotType forbiddenSlotType = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == slotType.forbiddenChildrenSlotTypes[i]);
                    forbiddenChildrenTypes[i].Init(slotType.forbiddenChildrenSlotTypes[i]);
                    forbiddenChildrenTypes[i].SetUIActive(true);
                }

            }
        }
        else
        {
            _layoutGroupFCSContent.transform.parent.gameObject.SetActive(false);
        }

    }

    void UpdateForbiddenParentsTypes()
    {
        if (slotType.isForbiddenAllParentsSlots)
        {
            _textFPSText.gameObject.SetActive(true);
            _layoutGroupFPSContent.gameObject.SetActive(false);
            return;
        }
        else
        {
            _textFPSText.gameObject.SetActive(false);
            _layoutGroupFPSContent.gameObject.SetActive(true);
        }

        if (slotType.forbiddenParentsSlotTypes[0] != 0)
        {
            _layoutGroupFPSContent.transform.parent.gameObject.SetActive(true);
            for (int i = 0; i < forbiddenParentsTypes.Count; i++)
            {
                forbiddenParentsTypes[i].SetUIActive(false);
                if (i < slotType.forbiddenParentsSlotTypes.Length && slotType.forbiddenParentsSlotTypes[0] != 0)
                {
                    ConstructorSlotType forbiddenSlotType = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == slotType.forbiddenParentsSlotTypes[i]);
                    forbiddenParentsTypes[i].Init(slotType.forbiddenParentsSlotTypes[i]);
                    forbiddenParentsTypes[i].SetUIActive(true);
                }

            }
        }
        else
        {
            _layoutGroupFPSContent.transform.parent.gameObject.SetActive(false);
        }
    }

}
