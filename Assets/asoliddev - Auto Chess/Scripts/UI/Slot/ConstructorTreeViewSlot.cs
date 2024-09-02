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

public class ConstructorTreeViewSlot : MonoBehaviour
{
    public Image icon;
    public Image bgIMG;
    public Image pickedFrame;
    Camera cam;
    RectTransform screenCanvasRectTransform;
    [HideInInspector]
    public Image lineImage;
    public GameObject linePrefab;

    [HideInInspector]
    public ConstructorAssembleController controller;

    public ConstructorSlot constructorSlot;
    public ConstructorBase constructor;
    public List<ConstructorTreeViewSlot> children = new List<ConstructorTreeViewSlot>();
    public ConstructorTreeViewSlot parent;
    ConstructorTreeViewInfo constructorTreeViewInfo;


    // Start is called before the first frame update
    void Awake()
    {
        cam = GameObject.Find("3DToUICamera").GetComponent<Camera>();
        screenCanvasRectTransform = GameObject.Find("ScreenCanvas").GetComponent<RectTransform>();
        constructorTreeViewInfo = transform.Find("ConstructorInfo").GetComponent<ConstructorTreeViewInfo>();
    }
    void Start()
    {
        pickedFrame.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (constructorSlot != null)
        {
            if (constructorSlot.slotTrans != null)
                GetComponent<RectTransform>().anchoredPosition = GetScreenPosition(constructorSlot.slotTrans);
        }
        else
        {
            if (constructor != null)
            {
                GetComponent<RectTransform>().anchoredPosition = GetScreenPosition(constructor.transform);
            }
        }
        if (parent != null)
        {
            SetLine(transform.position, parent.transform.position);
        }

    }

    public void SetLine(Vector2 startPoint, Vector2 endPoint)
    {
        if (lineImage == null)
        {
            return;
        }
        // 角度计算
        Vector2 dir = endPoint - startPoint;
        Vector2 dirV2 = new Vector2(dir.x, dir.y);
        float angle = Vector2.SignedAngle(dirV2, Vector2.down);

        // 距离长度，偏转设置
        lineImage.transform.Rotate(0, 0, angle);
        lineImage.transform.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
        float distance = Vector2.Distance(endPoint, startPoint);

        lineImage.rectTransform.sizeDelta = new Vector2(0.5f, distance / 2);

        // 设置位置
        dir = endPoint + startPoint;
        lineImage.GetComponent<RectTransform>().position = new Vector3((float)(dir.x * 0.5f), (float)(dir.y * 0.5f), 0f);
    }

    public Vector3 GetScreenPosition(Transform target)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);
        RectTransform canvasRtm = controller.GetComponent<RectTransform>();
        Vector2 uguiPos = Vector2.zero;
        uguiPos.x = (viewportPos.x - .5f) * canvasRtm.rect.width * 1.2f;
        uguiPos.y = (viewportPos.y - .5f) * canvasRtm.rect.height * 1.2f;
        return uguiPos;
    }

    public void Init(ConstructorAssembleController _controller, ConstructorTreeViewSlot _parent, ConstructorSlot _constructorSlot)
    {
        controller = _controller;
        parent = _parent;
        constructorSlot = _constructorSlot;
        if (constructorSlot.constructorInstance != null)
            constructor = constructorSlot.constructorInstance;

        constructorTreeViewInfo.Init(this);
        constructorTreeViewInfo.onPointerEnterEvent.AddListener(constructorTreeViewInfo.OnPointerEnterEvent);
        constructorTreeViewInfo.onPointerExitEvent.AddListener(constructorTreeViewInfo.OnPointerExitEvent);


        if (constructor != null)
        {
            constructorTreeViewInfo.onPointerDownEvent.AddListener(constructorTreeViewInfo.OnPointerDownEvent);
            constructorTreeViewInfo.onPointerUpEvent.AddListener(constructorTreeViewInfo.OnPointerUpEvent);
            constructorTreeViewInfo.onDragEvent.AddListener(constructorTreeViewInfo.OnDragEvent);

            bgIMG.gameObject.SetActive(true);
            bgIMG.color = GameConfig.Instance.levelColors[constructor.constructorData.level - 1];
            LoadIcon();
            if (constructor.slots.Count > 0)
            {
                ExpandSubSlot();
            }
        }
        else
        {
            bgIMG.gameObject.SetActive(false);
            icon.gameObject.SetActive(false);
        }

        if (_parent != null)
        {
            if (lineImage == null)
            {
                lineImage = Instantiate(linePrefab, controller.lineLayer).GetComponent<Image>();
            }
            else
            {
                lineImage.gameObject.SetActive(true);
            }
        }
    }

    public void ChassisConstructorInit(ConstructorAssembleController _controller, ConstructorBase _constructor)
    {
        controller = _controller;
        constructor = _constructor;

        constructorTreeViewInfo.Init(this);
        constructorTreeViewInfo.onPointerEnterEvent.AddListener(constructorTreeViewInfo.OnPointerEnterEvent);
        constructorTreeViewInfo.onPointerExitEvent.AddListener(constructorTreeViewInfo.OnPointerExitEvent);

        constructorTreeViewInfo.onPointerDownEvent.AddListener(constructorTreeViewInfo.OnPointerDownEvent);
        constructorTreeViewInfo.onPointerUpEvent.AddListener(constructorTreeViewInfo.OnPointerUpEvent);
        constructorTreeViewInfo.onDragEvent.AddListener(constructorTreeViewInfo.OnDragEvent);

        bgIMG.gameObject.SetActive(true);
        bgIMG.color = GameConfig.Instance.levelColors[constructor.constructorData.level - 1];
        LoadIcon();
        if (constructor.slots.Count > 0)
        {
            ExpandSubSlot();
        }
    }
    public void LoadIcon()
    {
        icon.gameObject.SetActive(true);
        Sprite _icon = Resources.Load<Sprite>(GamePlayController.Instance.GetConstructorIconPath(constructor.constructorData));
        icon.sprite = _icon;
    }

    public void ExpandSubSlot()
    {
        foreach (var s in constructor.slots)
        {
            if (s.isAble)
            {
                GameObject obj = controller.NewConstructorSlot();
                obj.transform.SetParent(controller.constructorPanel.transform);
                //obj.transform.SetParent(subTab);
                ConstructorTreeViewSlot treeViewSlot = obj.GetComponent<ConstructorTreeViewSlot>();
                treeViewSlot.Init(controller, this, s);
                children.Add(treeViewSlot);
            }

        }

    }

    public void ClearSubSlot()
    {
        foreach (var c in children)
        {
            c.ClearSubSlot();
            c.Reset();
            c.Recycling();
        }
        children.Clear();
    }
    public void Reset()
    {
        constructorSlot = null;
        constructor = null;
        parent = null;
        icon.gameObject.SetActive(false);
        constructorTreeViewInfo.ClearAllListener();
    }
    public void Recycling()
    {
        transform.SetParent(controller.DisableSlotsParent);
        lineImage.gameObject.SetActive(false);
    }

    public bool AttachConstructor(ConstructorBaseData constructorData)
    {
        ConstructorType type = (ConstructorType)Enum.Parse(typeof(ConstructorType), constructorData.type);
        if (constructorSlot.adaptTypes.Contains(type) && constructorSlot.isAble)
        {
            if (constructor != null)
                RemoveConstructor();
            parent.constructor.AttachConstructor(constructorData, constructorSlot);
            Init(controller, parent, constructorSlot);
            return true;
        }
        return false;
    }

    public void RemoveConstructor()
    {
        List<ConstructorBaseData> removedData = new List<ConstructorBaseData>();
        if (constructor.type == ConstructorType.Chassis)
        {
            ChampionController championController = constructor.championController;
            removedData = constructor.removeAllConstructor();
            championController.DestroySelf();
        }

        else
        {
            removedData = parent.constructor.removeConstructor(constructorSlot);
            ClearSubSlot();
            constructor = null;
            icon.gameObject.SetActive(false);
            constructorTreeViewInfo.ClearAllListener();
            Init(controller, parent, constructorSlot);
        }

        UIController.Instance.inventoryController.AddConstructors(removedData);
    }

    public void FindPickedSlot(ConstructorSlot slot)
    {
        if (constructorSlot == slot)
        {
            controller.pickedSlot = this;
        }
        else
        {
            foreach (var c in children)
            {
                c.FindPickedSlot(slot);
            }
        }
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
