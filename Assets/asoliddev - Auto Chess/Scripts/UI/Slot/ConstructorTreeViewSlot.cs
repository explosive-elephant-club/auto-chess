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

/// <summary>
/// 部件槽位UI
/// </summary>
public class ConstructorTreeViewSlot : MonoBehaviour
{
    /// <summary>
    /// 部件图标
    /// </summary>
    public Image constructorIcon;
    /// <summary>
    /// 槽位图标
    /// </summary>
    public Image slotIcon;
    /// <summary>
    /// 空背景
    /// </summary>
    public Image bgIMG;
    /// <summary>
    /// 选中框
    /// </summary>
    public Image pickedFrame;
    Camera cam;
    RectTransform screenCanvasRectTransform;

    /// <summary>
    /// 槽位连接线图片实体
    /// </summary>
    [HideInInspector]
    public Image lineImage;
    /// <summary>
    /// 槽位连接线预制体
    /// </summary>
    public GameObject linePrefab;

    /// <summary>
    /// 组装管理器
    /// </summary>
    [HideInInspector]
    public ConstructorAssembleController controller;

    /// <summary>
    /// 部件的槽位
    /// </summary>
    public ConstructorSlot constructorSlot;
    /// <summary>
    /// 当前槽位中的部件
    /// </summary>
    public ConstructorBase constructor;
    /// <summary>
    /// 子节点数组
    /// </summary>
    public List<ConstructorTreeViewSlot> children = new List<ConstructorTreeViewSlot>();
    /// <summary>
    /// 父节点
    /// </summary>
    public ConstructorTreeViewSlot parent;
    /// <summary>
    /// 信息显示UI
    /// </summary>
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
        //如果constructorSlot槽位存在，则此UI位置跟随槽位的slotTrans位置
        if (constructorSlot != null)
        {
            if (constructorSlot.slotTrans != null)
                GetComponent<RectTransform>().anchoredPosition = GetScreenPosition(constructorSlot.slotTrans);
        }
        else
        {
            //如果没有槽位,但是有部件，则此UI位置跟随部件的位置
            if (constructor != null)
            {
                GetComponent<RectTransform>().anchoredPosition = GetScreenPosition(constructor.transform);
            }
        }
        //如果存在父节点UI 用线连接父节点UI
        if (parent != null)
        {
            SetLine(transform.position, parent.transform.position);
        }

    }

    /// <summary>
    /// 设置一条线连接父子节点UI
    /// </summary>
    /// <param name="startPoint">子节点起点</param>
    /// <param name="endPoint">父节点终点</param>
    public void SetLine(Vector2 startPoint, Vector2 endPoint)
    {
        if (lineImage == null)
        {
            return;
        }
        //计算两点之间的角度
        Vector2 dir = endPoint - startPoint;
        Vector2 dirV2 = new Vector2(dir.x, dir.y);
        float angle = Vector2.SignedAngle(dirV2, Vector2.down);

        // 距离长度，偏转设置
        lineImage.transform.Rotate(0, 0, angle);
        lineImage.transform.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
        float distance = Vector2.Distance(endPoint, startPoint);

        //设置sizeDelta使其适应距离长度
        lineImage.rectTransform.sizeDelta = new Vector2(0.5f, distance / 2);

        //设置位置
        dir = endPoint + startPoint;
        lineImage.GetComponent<RectTransform>().position = new Vector3((float)(dir.x * 0.5f), (float)(dir.y * 0.5f), 0f);
    }

    /// <summary>
    /// 通过获取目标在摄像机的视口坐标 世界坐标 → UI 坐标转换
    /// </summary>
    /// <param name="target">需要显示的目标</param>
    /// <returns>计算出UI的anchoredPosition</returns>
    public Vector3 GetScreenPosition(Transform target)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);
        RectTransform canvasRtm = controller.GetComponent<RectTransform>();
        Vector2 uguiPos = Vector2.zero;
        uguiPos.x = (viewportPos.x - .5f) * canvasRtm.rect.width * 1.2f;
        uguiPos.y = (viewportPos.y - .5f) * canvasRtm.rect.height * 1.2f;
        return uguiPos;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="_controller">组装管理器</param>
    /// <param name="_parent">父节点</param>
    /// <param name="_constructorSlot">对应的槽位</param>
    public void Init(ConstructorAssembleController _controller, ConstructorTreeViewSlot _parent, ConstructorSlot _constructorSlot)
    {
        controller = _controller;
        parent = _parent;
        constructorSlot = _constructorSlot;
        if (constructorSlot.constructorInstance != null)
            constructor = constructorSlot.constructorInstance;

        //绑定鼠标事件
        constructorTreeViewInfo.Init(this);
        constructorTreeViewInfo.onPointerEnterEvent.AddListener(constructorTreeViewInfo.OnPointerEnterEvent);
        constructorTreeViewInfo.onPointerExitEvent.AddListener(constructorTreeViewInfo.OnPointerExitEvent);

        //如果有部件 就显示部件相关UI
        if (constructor != null)
        {
            constructorTreeViewInfo.onPointerDownEvent.AddListener(constructorTreeViewInfo.OnPointerDownEvent);
            constructorTreeViewInfo.onPointerUpEvent.AddListener(constructorTreeViewInfo.OnPointerUpEvent);
            constructorTreeViewInfo.onDragEvent.AddListener(constructorTreeViewInfo.OnDragEvent);

            bgIMG.gameObject.SetActive(true);
            bgIMG.color = GameConfig.Instance.levelColors[constructor.constructorData.level - 1];
            LoadIcon();
            slotIcon.gameObject.SetActive(false);
            if (constructor.slots.Count > 0)
            {
                ExpandSubSlot();
            }
        }
        //如果无部件 就不显示
        else
        {
            bgIMG.gameObject.SetActive(false);
            constructorIcon.gameObject.SetActive(false);
            slotIcon.gameObject.SetActive(true);
        }

        //如果有父节点就实例化线预制体
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

    /// <summary>
    /// 底盘部件槽位初始化 遍历并递归部件树形结构 直到所有部件槽位初始化完毕
    /// </summary>
    /// <param name="_controller">组装管理器</param>
    /// <param name="_constructor">底盘部件</param>
    public void ChassisConstructorSlotInit(ConstructorAssembleController _controller, ConstructorBase _constructor)
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
        //遍历并递归部件树形结构
        if (constructor.slots.Count > 0)
        {
            ExpandSubSlot();
        }
    }

    /// <summary>
    /// 通过加载部件的UI图标
    /// </summary>
    public void LoadIcon()
    {
        constructorIcon.gameObject.SetActive(true);
        Sprite _icon = Resources.Load<Sprite>(GamePlayController.Instance.GetConstructorIconPath(constructor.constructorData));
        constructorIcon.sprite = _icon;
    }

    /// <summary>
    /// 递归构造UI的树状结构
    /// </summary>
    public void ExpandSubSlot()
    {
        //遍历 constructor.slots 并创建子节点 ConstructorTreeViewSlot
        foreach (var s in constructor.slots)
        {
            if (s.isAble)
            {
                //从对象池取出空白子节点
                GameObject obj = controller.NewConstructorSlot();
                obj.transform.SetParent(controller.constructorPanel.transform);
                //obj.transform.SetParent(subTab);
                ConstructorTreeViewSlot treeViewSlot = obj.GetComponent<ConstructorTreeViewSlot>();
                //初始化子节点 开启下一轮的遍历
                treeViewSlot.Init(controller, this, s);
                children.Add(treeViewSlot);
            }

        }

    }

    /// <summary>
    /// 递归清除子节点
    /// </summary>
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

    /// <summary>
    /// 重置清空UI的显示和数据
    /// </summary>
    public void Reset()
    {
        constructorSlot = null;
        constructor = null;
        parent = null;
        constructorIcon.gameObject.SetActive(false);
        slotIcon.gameObject.SetActive(false);
        constructorTreeViewInfo.ClearAllListener();
    }

    /// <summary>
    /// 将UI放回对象池
    /// </summary>
    public void Recycling()
    {
        transform.SetParent(controller.DisableSlotsParent);
        lineImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 新的部件被放入槽位
    /// </summary>
    /// <param name="constructorData"></param>
    /// <returns>放入的结果</returns>
    public bool AttachConstructor(ConstructorBaseData constructorData)
    {
        //检查槽位和部件是否适配
        ConstructorType type = (ConstructorType)Enum.Parse(typeof(ConstructorType), constructorData.type);
        if (constructorSlot.adaptTypes.Contains(type) && constructorSlot.isAble)
        {
            //如果已有部件，先移除
            if (constructor != null)
                RemoveConstructor();
            //绑定新的部件
            parent.constructor.AttachConstructor(constructorData, constructorSlot);
            //重新初始化
            Init(controller, parent, constructorSlot);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 移除槽位中的部件
    /// </summary>
    public void RemoveConstructor()
    {
        List<ConstructorBaseData> removedData = new List<ConstructorBaseData>();
        //如果是底盘或者个体类型的部件 销毁单位
        if (constructor.type == ConstructorType.Chassis || constructor.type == ConstructorType.Isolate)
        {
            ChampionController championController = constructor.championController;
            removedData = constructor.removeAllConstructor();
            championController.DestroySelf();
        }
        //递归清理子节点
        else
        {
            removedData = parent.constructor.removeConstructor(constructorSlot);
            ClearSubSlot();
            constructor = null;
            constructorIcon.gameObject.SetActive(false);
            slotIcon.gameObject.SetActive(false);
            constructorTreeViewInfo.ClearAllListener();
            Init(controller, parent, constructorSlot);
        }

        UIController.Instance.inventoryController.AddConstructors(removedData);
    }

    /// <summary>
    /// 查找槽位
    /// </summary>
    /// <param name="slot"></param>
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
