using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using ExcelConfig;
using UnityEditor;
using UnityEngine.EventSystems;
using Game;

public class InventorySlot : ContainerSlot
{
    int cost;
    public Image[] BGImages;
    public List<SingleMFInfo> singleMFInfoList;
    public List<SlotInfo> slotInfoList;

    public GameObject pointTip;
    public InventoryConstructor inventoryConstructor;

    #region 自动绑定
    private Image _imgPanel;
    private Image _imgIconAndName;
    private Image _imgIcon;
    private Image _imgPointTip;
    private Image _imgType;
    private Image _imgCost;
    private Image _imgBonusTypeBar;
    private Image _imgSlotsBar;
    private UICustomText _textNameText;
    private UICustomText _textTypeText;
    private UICustomText _textCostText;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _imgPanel = transform.Find("Panel_Auto").GetComponent<Image>();
        _imgIconAndName = transform.Find("Panel_Auto/IconAndName_Auto").GetComponent<Image>();
        _imgIcon = transform.Find("Panel_Auto/IconAndName_Auto/Bg/Icon_Auto").GetComponent<Image>();
        _imgPointTip = transform.Find("Panel_Auto/IconAndName_Auto/PointTip_Auto").GetComponent<Image>();
        _imgType = transform.Find("Panel_Auto/Type_Auto").GetComponent<Image>();
        _imgCost = transform.Find("Panel_Auto/Cost_Auto").GetComponent<Image>();
        _imgBonusTypeBar = transform.Find("Panel_Auto/BonusTypeBar_Auto").GetComponent<Image>();
        _imgSlotsBar = transform.Find("Panel_Auto/SlotsBar_Auto").GetComponent<Image>();
        _textNameText = transform.Find("Panel_Auto/IconAndName_Auto/NameText_Auto").GetComponent<UICustomText>();
        _textTypeText = transform.Find("Panel_Auto/Type_Auto/TypeText_Auto").GetComponent<UICustomText>();
        _textCostText = transform.Find("Panel_Auto/Cost_Auto/CostText_Auto").GetComponent<UICustomText>();
    }
    #endregion

    public override void Awake()
    {
        base.Awake();
        foreach (Transform child in _imgBonusTypeBar.transform)
        {
            singleMFInfoList.Add(child.GetComponent<SingleMFInfo>());
        }
        foreach (Transform child in _imgSlotsBar.transform)
        {
            slotInfoList.Add(child.GetComponent<SlotInfo>());
        }
        BGImages = new Image[] { _imgIconAndName, _imgType, _imgCost, _imgBonusTypeBar, _imgSlotsBar };
    }

    public void Init(InventoryConstructor _inventoryConstructor)
    {
        inventoryConstructor = _inventoryConstructor;

        cost = Mathf.CeilToInt
     (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(inventoryConstructor.constructorBaseData.type).cost *
      GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(inventoryConstructor.constructorBaseData.level).cost);
        foreach (var img in BGImages)
        {
            img.color = GameConfig.Instance.levelColors[inventoryConstructor.constructorBaseData.level - 1];
        }
        pointTip.SetActive(inventoryConstructor.isNew);
        LoadIcon();
        _textNameText.text = inventoryConstructor.constructorBaseData.name;
        _textTypeText.text = inventoryConstructor.constructorBaseData.type.ToString();
        _textCostText.text = cost.ToString();
        UpdateMFInfo();
        UpdateSlotInfo(inventoryConstructor.constructorBaseData);
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        onPointerDownEvent.AddListener(OnPointerDownEvent);
        onPointerUpEvent.AddListener(OnPointerUpEvent);
        onDragEvent.AddListener(OnDragEvent);
    }

    public void LoadIcon()
    {
        Sprite _icon = Resources.Load<Sprite>(GamePlayController.Instance.GetConstructorIconPath(inventoryConstructor.constructorBaseData));
        _imgIcon.sprite = _icon;
    }

    public void AttachConstructor(ConstructorTreeViewSlot constructorTreeViewSlot)
    {
        if (constructorTreeViewSlot.AttachConstructor(inventoryConstructor.constructorBaseData))
        {
            UIController.Instance.inventoryController.RemoveConstructor(inventoryConstructor);
        }
    }

    public void AddConstructor(MapContainer mapContainer, Vector3 pos)
    {
        if (mapContainer.containerType == ContainerType.Inventory)
        {
            GamePlayController.Instance.ownChampionManager.AddChampionToInventory(inventoryConstructor.constructorBaseData, pos);
            UIController.Instance.inventoryController.RemoveConstructor(inventoryConstructor);
        }
    }

    public void UpdateMFInfo()
    {
        List<ConstructorBonusType> types = GamePlayController.Instance.GetAllChampionTypes(inventoryConstructor.constructorBaseData);

        for (int i = 0; i < singleMFInfoList.Count; i++)
        {
            singleMFInfoList[i].SetUIActive(false);
            if (i < types.Count && types[i] != null)
            {
                singleMFInfoList[i].Init(types[i]);
                singleMFInfoList[i].ClearAllListener();
                singleMFInfoList[i].SetUIActive(true);
            }
        }
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
        //_imgSlotsBar.gameObject.SetActive(slotInfoArray.Count > 0);
    }

    public void OnPointerDownEvent(PointerEventData eventData)
    {
        //icon.gameObject.SetActive(false);
        draggedUI.Init(_imgIcon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        //icon.gameObject.SetActive(true);
        draggedUI.OnPointerUp(eventData);
        if (UIController.Instance.constructorAssembleController.pointEnterTreeViewSlot != null)
        {
            AttachConstructor(UIController.Instance.constructorAssembleController.pointEnterTreeViewSlot);
        }
        else if (InputController.Instance.mapContainer != null &&
            (inventoryConstructor.constructorBaseData.type == ConstructorType.Chassis.ToString() ||
            inventoryConstructor.constructorBaseData.type == ConstructorType.Isolate.ToString()))
        {
            AddConstructor(InputController.Instance.mapContainer, InputController.Instance.mousePosition);
        }
    }

    public void OnDragEvent(PointerEventData eventData)
    {
        draggedUI.OnDrag(eventData);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        if (inventoryConstructor.isNew)
        {
            inventoryConstructor.isNew = false;
            pointTip.SetActive(inventoryConstructor.isNew);
            UIController.Instance.inventoryController.UpdateNewCount();
        }

        UIController.Instance.inventoryController.OnPointEnterSlot(this);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.inventoryController.OnPointLeaveSlot();
    }

}
