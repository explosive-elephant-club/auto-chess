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
    public Transform subTab;

    RectTransform slotRect;
    RectTransform subTabRect;
    Camera cam;


    ConstructorAssembleController controller;

    public Sprite constructorIcon;

    public ConstructorSlot constructorSlot;
    public ConstructorBase constructor;
    public List<ConstructorTreeViewSlot> children = new List<ConstructorTreeViewSlot>();
    public ConstructorTreeViewSlot parent;

    // Start is called before the first frame update
    void Awake()
    {
        icon = transform.Find("ConstructorInfo/FrameMask/Icon").GetComponent<Image>();
        cam = GameObject.Find("3DToUICamera").GetComponent<Camera>();
    }
    private void Update()
    {
        if (constructorSlot != null)
        {
            if (constructorSlot.slotTrans != null)
                transform.position = GetScreenPosition(constructorSlot.slotTrans) + controller.GetComponent<RectTransform>().transform.position;
        }
        else
        {
            if (constructor != null)
            {
                transform.position = GetScreenPosition(constructor.transform) + controller.GetComponent<RectTransform>().transform.position;
            }
        }


    }

    public Vector3 GetScreenPosition(Transform target)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);
        RectTransform canvasRtm = controller.GetComponent<RectTransform>();
        Vector2 uguiPos = Vector2.zero;
        uguiPos.x = (viewportPos.x - 0.5f) * canvasRtm.sizeDelta.x * 3f;
        uguiPos.y = (viewportPos.y - 0.5f) * canvasRtm.sizeDelta.y * 3f;
        return uguiPos;
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
            onPointerDownEvent.AddListener(OnPointerDownEvent);
            onPointerUpEvent.AddListener(OnPointerUpEvent);
            onDragEvent.AddListener(OnDragEvent);

            StartCoroutine(LoadIcon(constructor.gameObject));
            if (constructor.slots.Count > 0)
            {
                ExpandSubSlot();
            }
        }
        else
        {
            icon.gameObject.SetActive(false);
        }
    }

    public void ChassisConstructorInit(ConstructorAssembleController _controller, ConstructorBase _constructor)
    {
        controller = _controller;
        constructor = _constructor;
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        //onPointerDownEvent.AddListener(OnPointerDownEvent);
        //onPointerUpEvent.AddListener(OnPointerUpEvent);
        //onDragEvent.AddListener(OnDragEvent);

        StartCoroutine(LoadIcon(constructor.gameObject));

        if (constructor.slots.Count > 0)
        {
            ExpandSubSlot();
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
        Debug.Log("ExpandSubSlot " + constructor.gameObject);
        foreach (var s in constructor.slots)
        {
            Debug.Log(s.slotTrans.name);
            GameObject obj = controller.NewConstructorSlot();
            obj.transform.SetParent(subTab);
            ConstructorTreeViewSlot treeViewSlot = obj.GetComponent<ConstructorTreeViewSlot>();
            treeViewSlot.Init(controller, this, s);

            children.Add(treeViewSlot);
        }

    }

    public void ClearSubSlot()
    {
        foreach (var c in children)
        {
            c.ClearSubSlot();
            c.Reset();
            controller.RecyclingConstructorSlot(c);
        }
        children.Clear();
    }
    public void Reset()
    {
        constructorSlot = null;
        constructor = null;
        parent = null;
        icon.gameObject.SetActive(false);
        ClearAllListener();
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
    public void OnPointerDownEvent(PointerEventData eventData)
    {
        icon.gameObject.SetActive(false);
        draggedUI.Init(icon.sprite, gameObject);
        draggedUI.transform.position = transform.position;
        draggedUI.OnPointerDown(eventData);
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
        Reset();
        Init(controller, parent, constructorSlot);
        UIController.Instance.inventoryController.AddConstructors(removedData);
    }

    /*
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
    

    
     */
}
