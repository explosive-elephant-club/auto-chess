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
    Image icon;
    InventoryController inventoryController;
    ConstructorBaseData constructorData;

    // Start is called before the first frame update
    void Awake()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
    }

    public void Init(ConstructorBaseData _constructorData, InventoryController _inventoryController)
    {
        constructorData = _constructorData;
        inventoryController = _inventoryController;

        Texture2D tex = AssetPreview.GetAssetPreview(Resources.Load<GameObject>(constructorData.prefab));
        Debug.Log(constructorData.prefab);
        icon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);

        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        onPointerDownEvent.AddListener(OnPointerDownEvent);
        onPointerUpEvent.AddListener(OnPointerUpEvent);
        onDragEvent.AddListener(OnDragEvent);
    }

    public void AddActivatedSkill()
    {

    }
    public void AddDeactivatedSkill()
    {

    }


    public void OnPointerDownEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(false);
        draggedUI.Init(icon.sprite, null);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(true);
        draggedUI.OnPointerUp(eventData);

    }

    public void OnDragEvent(PointerEventData eventData)
    {
        draggedUI.OnDrag(eventData);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        inventoryController.pointEnterInventorySlot = this;
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        inventoryController.pointEnterInventorySlot = null;
    }

}
