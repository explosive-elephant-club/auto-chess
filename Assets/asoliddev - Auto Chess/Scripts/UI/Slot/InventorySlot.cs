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
    RectTransform rect;
    Image icon;
    Transform[] slots;
    public ConstructorBaseData constructorData;

    // Start is called before the first frame update
    void Awake()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
        //slots = transform.Find("Panel").GetComponentsInChildren<Transform>();
    }

    public void Init(ConstructorBaseData _constructorData)
    {
        constructorData = _constructorData;

        LoadIcon();
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        onPointerDownEvent.AddListener(OnPointerDownEvent);
        onPointerUpEvent.AddListener(OnPointerUpEvent);
        onDragEvent.AddListener(OnDragEvent);
    }

    public void LoadIcon()
    {

        string iconPath = constructorData.prefab.Substring(0, constructorData.prefab.IndexOf(constructorData.type));
        iconPath = iconPath + constructorData.type + "/Icon/";
        string namePath = constructorData.prefab.Substring(constructorData.prefab.IndexOf(constructorData.type) + constructorData.type.Length + 1);

        Sprite _icon = Resources.Load<Sprite>(iconPath + namePath);
        icon.sprite = _icon;
    }

    public void AttachConstructor(ConstructorTreeViewSlot constructorTreeViewSlot)
    {
        if (constructorTreeViewSlot.AttachConstructor(constructorData))
        {
            UIController.Instance.inventoryController.RemoveConstructor(constructorData);
        }
    }

    public void AddConstructor(GridInfo grid)
    {
        if (grid.gridType == GridType.Inventory && grid.occupyChampion == null)
        {
            GamePlayController.Instance.ownChampionManager.AddChampionToInventory(constructorData, grid);
            UIController.Instance.inventoryController.RemoveConstructor(constructorData);
        }
    }

    public void OnPointerDownEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(false);
        draggedUI.Init(icon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(true);
        draggedUI.OnPointerUp(eventData);
        if (UIController.Instance.constructorAssembleController.pointEnterTreeViewSlot != null)
        {
            AttachConstructor(UIController.Instance.constructorAssembleController.pointEnterTreeViewSlot);
        }
        else if (InputController.Instance.gridInfo != null && constructorData.type == ConstructorType.Chassis.ToString())
        {
            AddConstructor(InputController.Instance.gridInfo);
        }
    }

    public void OnDragEvent(PointerEventData eventData)
    {
        draggedUI.OnDrag(eventData);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.inventoryController.OnPointEnterSlot(this);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.inventoryController.OnPointLeaveSlot();
    }

}
