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

public class ConstructorTreeViewSlot : ContainerSlot
{
    Image icon;
    TextMeshProUGUI text;
    Toggle expandToggle;
    public Transform subTab;

    RectTransform slotRect;
    RectTransform subTabRect;



    ConstructorAssembleController controller;

    public Sprite emptyIcon;
    public Sprite constructorIcon;

    public ConstructorSlot constructorSlot;
    public ConstructorBase constructor;
    public List<ConstructorTreeViewSlot> children = new List<ConstructorTreeViewSlot>();
    public ConstructorTreeViewSlot parent;

    // Start is called before the first frame update
    void Awake()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
        emptyIcon = icon.sprite;
        text = transform.Find("TypeText").GetComponent<TextMeshProUGUI>();
        expandToggle = transform.Find("Toggle").GetComponent<Toggle>();
        subTab = transform.parent.Find("SubTab");
        slotRect = transform.parent.GetComponent<RectTransform>();
        subTabRect = subTab.GetComponent<RectTransform>();
    }

    public void Init(ConstructorAssembleController _controller, ConstructorTreeViewSlot _parent, ConstructorSlot _constructorSlot)
    {
        controller = _controller;
        parent = _parent;
        constructorSlot = _constructorSlot;
        constructor = constructorSlot.constructorInstance;

        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);

        if (constructor != null)
        {
            expandToggle.onValueChanged.RemoveAllListeners();
            onPointerDownEvent.AddListener(OnPointerDownEvent);
            onPointerUpEvent.AddListener(OnPointerUpEvent);
            onDragEvent.AddListener(OnDragEvent);

            StartCoroutine(LoadIcon(constructor.gameObject));
            text.text = constructor.parentConstructor.slots.Find(s => s.constructorInstance == constructor).slotTrans.name;
            if (constructor.slots.Count > 0)
            {
                expandToggle.gameObject.SetActive(true);
                expandToggle.onValueChanged.AddListener((bool var) =>
                {
                    if (var)
                        ExpandSubSlot();
                    else
                        ClearSubSlot();
                });
            }
            else
            {
                expandToggle.gameObject.SetActive(false);
            }
        }
        else
        {
            text.text = _constructorSlot.slotTrans.name;

            expandToggle.gameObject.SetActive(false);
            icon.gameObject.SetActive(false);
        }

    }

    public void Init(ConstructorAssembleController _controller, ConstructorBase _constructor)
    {
        controller = _controller;
        constructor = _constructor;
        ClearAllListener();
        expandToggle.onValueChanged.RemoveAllListeners();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        //onPointerDownEvent.AddListener(OnPointerDownEvent);
        //onPointerUpEvent.AddListener(OnPointerUpEvent);
        //onDragEvent.AddListener(OnDragEvent);

        StartCoroutine(LoadIcon(constructor.gameObject));
        text.text = _constructor.type.ToString();

        if (constructor.slots.Count > 0)
        {
            expandToggle.gameObject.SetActive(true);
            expandToggle.onValueChanged.AddListener((bool var) =>
            {
                if (var)
                    ExpandSubSlot();
                else
                    ClearSubSlot();
            });
        }
        else
        {
            expandToggle.gameObject.SetActive(false);
        }

    }


    public IEnumerator LoadIcon(GameObject obj)
    {
        icon.gameObject.SetActive(true);
        yield return new WaitUntil(() => AssetPreview.GetAssetPreview(obj) != null);
        Texture2D tex = AssetPreview.GetAssetPreview(obj);
        constructorIcon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        icon.sprite = constructorIcon;
        yield return 0;
    }

    public void ExpandSubSlot()
    {
        foreach (var s in constructor.slots)
        {
            GameObject obj = controller.NewConstructorSlot();
            ConstructorTreeViewSlot treeViewSlot = obj.transform.Find("ConstructorInfo").GetComponentInChildren<ConstructorTreeViewSlot>();
            treeViewSlot.Init(controller, this, s);
            treeViewSlot.transform.parent.SetParent(subTab);
            children.Add(treeViewSlot);
        }
        StartCoroutine(controller.AllLayoutRebuilder(this));
    }

    public void ClearSubSlot()
    {
        foreach (var c in children)
        {
            c.ClearSubSlot();
            c.Clear();
            controller.RecyclingConstructorSlot(c);
        }
        children.Clear();
        if (controller != null)
            StartCoroutine(controller.AllLayoutRebuilder(this));
    }

    public void Clear()
    {
        icon.sprite = emptyIcon;
        text.text = "";
        ClearAllListener();
        expandToggle.isOn = false;
    }

    public IEnumerator UpdateRectSize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(subTabRect);
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(slotRect);
        yield return new WaitForEndOfFrame();
        if (parent != null)
        {
            parent.UpdateRectSize();
        }
    }

    public void OnPointerDownEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(false);
        draggedUI.Init(icon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
    }

    public bool AttachConstructor(ConstructorBaseData constructorData)
    {
        ConstructorType type = (ConstructorType)Enum.Parse(typeof(ConstructorType), constructorData.type);
        if (constructorSlot.adaptTypes.Contains(type) && constructorSlot.isAble)
        {
            if (constructor != null)
                RemoveConstructor();
            parent.constructor.attachConstructor(constructorData, constructorSlot);
            Init(controller, parent, constructorSlot);
            return true;
        }
        return false;
    }

    public void RemoveConstructor()
    {
        List<ConstructorBaseData> removedData = parent.constructor.removeConstructor(constructorSlot);
        ClearSubSlot();
        Clear();
        Init(controller, parent, constructorSlot);
        UIController.Instance.inventoryController.AddConstructors(removedData);
    }

    public void OnPointerUpEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(true);
        draggedUI.OnPointerUp(eventData);
        if (UIController.Instance.inventoryController.pointEnterInventorySlot != null)
        {
            AttachConstructor(UIController.Instance.inventoryController.pointEnterInventorySlot.constructorData);
        }
        else if (UIController.Instance.inventoryController.viewport == InputController.Instance.ui)
        {
            RemoveConstructor();
        }

    }

    public void OnDragEvent(PointerEventData eventData)
    {
        draggedUI.OnDrag(eventData);
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        controller.pointEnterInventorySlot = this;
        if (constructor != null)
            UIController.Instance.popupController.constructorPopup.Show
                   (constructor.constructorData, this.gameObject, Vector3.right);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        controller.pointEnterInventorySlot = null;
        UIController.Instance.popupController.constructorPopup.Clear();
    }
}
