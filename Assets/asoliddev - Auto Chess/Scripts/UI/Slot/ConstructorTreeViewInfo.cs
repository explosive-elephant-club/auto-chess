using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;
using UnityEditor;
using System;

public class ConstructorTreeViewInfo : ContainerSlot
{
    ConstructorTreeViewSlot treeViewSlot;
    public void Init(ConstructorTreeViewSlot _treeViewSlot)
    {
        treeViewSlot = _treeViewSlot;
        ClearAllListener();
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        treeViewSlot.icon.gameObject.SetActive(true);
        draggedUI.OnPointerUp(eventData);
        if (UIController.Instance.inventoryController.pointEnterInventorySlot != null)
        {
            treeViewSlot.AttachConstructor(UIController.Instance.inventoryController.pointEnterInventorySlot.constructorData);
        }
        else if (UIController.Instance.inventoryController.viewport == InputController.Instance.ui)
        {
            treeViewSlot.RemoveConstructor();
        }

    }
    public void OnPointerDownEvent(PointerEventData eventData)
    {
        treeViewSlot.icon.gameObject.SetActive(false);
        draggedUI.Init(treeViewSlot.icon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }


    public void OnDragEvent(PointerEventData eventData)
    {
        draggedUI.OnDrag(eventData);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        treeViewSlot.controller.pointEnterTreeViewSlot = treeViewSlot;
        if (treeViewSlot.constructor != null)
        {
            UIController.Instance.popupController.constructorPopup.Show
                         (treeViewSlot.constructor.constructorData, this.gameObject, Vector3.right);
        }

    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        treeViewSlot.controller.pointEnterTreeViewSlot = null;
        UIController.Instance.popupController.constructorPopup.Clear();
    }
}
