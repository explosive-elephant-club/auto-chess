using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ExcelConfig;
using UnityEditor;
using UnityEngine.EventSystems;

public class InventorySlot : ContainerSlot
{
    int cost;
    public Image[] images;
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI buyCostText;
    public GameObject[] typeIconArray;

    public Transform slotContent;
    public List<GameObject> slotInfo;

    public GameObject pointTip;
    public InventoryConstructor inventoryConstructor;

    private void Awake()
    {
        foreach (Transform child in slotContent)
        {
            slotInfo.Add(child.gameObject);
        }
    }

    public void Init(InventoryConstructor _inventoryConstructor)
    {
        inventoryConstructor = _inventoryConstructor;

        cost = Mathf.CeilToInt
     (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(inventoryConstructor.constructorBaseData.type).cost *
      GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(inventoryConstructor.constructorBaseData.level).cost);
        foreach (var img in images)
        {
            img.color = GameConfig.Instance.levelColors[inventoryConstructor.constructorBaseData.level - 1];
        }
        pointTip.SetActive(inventoryConstructor.isNew);
        LoadIcon();
        nameText.text = inventoryConstructor.constructorBaseData.name;
        typeText.text = inventoryConstructor.constructorBaseData.type.ToString();
        buyCostText.text = cost.ToString();
        UpdateType();
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
        icon.sprite = _icon;
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

    public void UpdateType()
    {
        List<ConstructorBonusType> types = GamePlayController.Instance.GetAllChampionTypes(inventoryConstructor.constructorBaseData);

        for (int i = 0; i < typeIconArray.Length; i++)
        {
            typeIconArray[i].SetActive(false);
            if (i < types.Count && types[i] != null)
            {
                typeIconArray[i].SetActive(true);
                typeIconArray[i].GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>(types[i].icon);
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
        slotContent.gameObject.SetActive(slotInfo.Count > 0);
    }

    public void OnPointerDownEvent(PointerEventData eventData)
    {
        //icon.gameObject.SetActive(false);
        draggedUI.Init(icon.sprite, gameObject);
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
