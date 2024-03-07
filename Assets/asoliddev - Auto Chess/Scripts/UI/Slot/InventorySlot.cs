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
    public Sprite[] levelFrames;
    public Image icon;
    public GameObject pointTip;
    public InventoryConstructor inventoryConstructor;

    public void Init(InventoryConstructor _inventoryConstructor)
    {
        inventoryConstructor = _inventoryConstructor;
        GetComponent<Image>().sprite = levelFrames[inventoryConstructor.constructorBaseData.level - 1];
        pointTip.SetActive(inventoryConstructor.isNew);
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

    public void AddConstructor(GridInfo grid)
    {
        if (grid.gridType == GridType.Inventory && grid.occupyChampion == null)
        {
            GamePlayController.Instance.ownChampionManager.AddChampionToInventory(inventoryConstructor.constructorBaseData, grid);
            UIController.Instance.inventoryController.RemoveConstructor(inventoryConstructor);
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
        else if (InputController.Instance.gridInfo != null && inventoryConstructor.constructorBaseData.type == ConstructorType.Chassis.ToString())
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
