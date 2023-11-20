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
    InventoryController inventoryController;
    public ConstructorBaseData constructorData;

    // Start is called before the first frame update
    void Awake()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
    }

    public void Init(ConstructorBaseData _constructorData, InventoryController _inventoryController)
    {
        constructorData = _constructorData;
        inventoryController = _inventoryController;


        StartCoroutine(LoadIcon());
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        onPointerDownEvent.AddListener(OnPointerDownEvent);
        onPointerUpEvent.AddListener(OnPointerUpEvent);
        onDragEvent.AddListener(OnDragEvent);
    }

    public IEnumerator LoadIcon()
    {
        GameObject prefab = Resources.Load<GameObject>(constructorData.prefab);

        yield return new WaitUntil(() => AssetPreview.GetAssetPreview(prefab) != null);
        Texture2D tex = AssetPreview.GetAssetPreview(prefab);
        icon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        yield return 0;
    }

    public void AttachConstructor(ConstructorTreeViewSlot constructorTreeViewSlot)
    {
        if (constructorTreeViewSlot.AttachConstructor(constructorData))
        {
            inventoryController.RemoveConstructor(constructorData);
        }
    }

    public void AddConstructor(GridInfo grid)
    {
        if (grid.gridType == GridType.Inventory && grid.occupyChampion == null)
        {
            GamePlayController.Instance.ownChampionManager.AddChampionToInventory(constructorData, grid);
            inventoryController.RemoveConstructor(constructorData);
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
        if (UIController.Instance.constructorAssembleController.pointEnterInventorySlot != null)
        {
            AttachConstructor(UIController.Instance.constructorAssembleController.pointEnterInventorySlot);
        }
        else if (InputController.Instance.gridInfo != null && constructorData.type == ConstructorType.Base.ToString())
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
        if (inventoryController.pointEnterInventorySlot == null)
            inventoryController.pointEnterInventorySlot = this;
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        inventoryController.pointEnterInventorySlot = null;
    }

}
