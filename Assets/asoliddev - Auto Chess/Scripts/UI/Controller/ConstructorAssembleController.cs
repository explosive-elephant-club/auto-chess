using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 部件组装管理器
/// </summary>
public class ConstructorAssembleController : BaseControllerUI
{
    public bool isEditable = true;
    /// <summary>
    ///组件槽位UI预制体
    /// </summary>
    public GameObject constructorSlotPrefab;
    /// <summary>
    ///根节点（底盘）
    /// </summary>
    public ConstructorTreeViewSlot chassisSlot;
    /// <summary>
    /// 鼠标悬停的组件槽位UI
    /// </summary>
    public ConstructorTreeViewSlot pointEnterTreeViewSlot;
    /// <summary>
    /// 此UI的panel节点
    /// </summary>
    public GameObject constructorPanel;
    /// <summary>
    ///视角滑块
    /// </summary>
    public Slider pitchSlider; public Slider yawSlider;
    /// <summary>
    ///缩放按钮
    /// </summary>
    public Button zoomInBtn; public Button zoomOutBtn;
    /// <summary>
    ///编辑模式开关
    /// </summary>
    public Toggle editToggle;
    /// <summary>
    ///预设缩放等级
    /// </summary>
    float[] zoomValues = { 12f, 15f, 18f, 20f, 24f };
    int zoomIndex = 2;
    public Text zoomValueText;

    /// <summary>
    /// 展开UI Panel
    /// </summary>
    public GameObject expandPanel;
    /// <summary>
    /// 展开按钮
    /// </summary>
    public Button expandBtn;
    /// <summary>
    /// 关闭按钮
    /// </summary>
    public Button closeBtn;

    GOToUICameraController camController;

    //存放未激活的上级节点 子节点是对象池以供复用
    [HideInInspector]
    public Transform DisableSlotsParent;
    [HideInInspector]
    public Transform lineLayer;
    //当前选中的槽位
    public ConstructorTreeViewSlot pickedSlot;

    // Start is called before the first frame update
    void Awake()
    {
        Init();
        DisableSlotsParent = constructorPanel.transform.Find("DisableSlots");
        lineLayer = constructorPanel.transform.Find("LineLayer");
    }

    #region 自动绑定
    
    #endregion
    private void Start()
    {
        camController = GamePlayController.Instance._GOToUICameraController;
        AddAllListener();
        SetUIActive(false);
    }

    /// <summary>
    /// 绑定UI事件
    /// </summary>
    void AddAllListener()
    {
        //控制相机视角
        pitchSlider.onValueChanged.AddListener(UpdatePitchSlider);
        yawSlider.onValueChanged.AddListener(UpdateYawSlider);
        //控制缩放
        zoomInBtn.onClick.AddListener(ZoomInBtnClick);
        zoomOutBtn.onClick.AddListener(ZoomOutBtnClick);
        //切换编辑模式
        editToggle.onValueChanged.AddListener(editToggleClick);
        //打开扩展
        expandBtn.onClick.AddListener(Expand);
        //关闭UI
        closeBtn.onClick.AddListener(Close);
    }

    void UpdatePitchSlider(float value)
    {
        camController.UpdatePitch(value);
    }
    void UpdateYawSlider(float value)
    {
        camController.UpdateYaw(value);
    }
    void ZoomInBtnClick()
    {
        if (zoomIndex < zoomValues.Length - 1)
        {
            zoomIndex++;
            camController.UpdateZoom(zoomValues[zoomIndex]);
            zoomValueText.text = (zoomValues[2] / zoomValues[zoomIndex]).ToString("0.00");
        }
    }
    void ZoomOutBtnClick()
    {
        if (zoomIndex > 0)
        {
            zoomIndex--;
            camController.UpdateZoom(zoomValues[zoomIndex]);
            zoomValueText.text = (zoomValues[2] / zoomValues[zoomIndex]).ToString("0.00");
        }
    }
    void editToggleClick(bool value)
    {
        isEditable = value;
        RefreshConstructorPanel();
    }

    void Expand()
    {
        isExpand = true;
        UpdateUI();
    }

    void Close()
    {
        isExpand = false;
        UpdateUI();
    }

    /// <summary>
    /// UI更新
    /// </summary>
    public override void UpdateUI()
    {
        //清空UI组件槽
        ClearAllSub(chassisSlot);
        chassisSlot.Reset();

        //判断是否有选中的单位
        if (GamePlayController.Instance.pickedChampion != null)
        {
            //判断是否展开UI，并设置默认视角和缩放
            if (isExpand)
            {
                expandPanel.SetActive(true);
                expandBtn.gameObject.SetActive(false);

                pitchSlider.value = 0.5f;
                UpdatePitchSlider(0.5f);
                yawSlider.value = 0.5f;
                UpdateYawSlider(0.5f);
                zoomIndex = 2;
                camController.UpdateZoom(zoomValues[zoomIndex]);
                zoomValueText.text = (zoomValues[2] / zoomValues[zoomIndex]).ToString("0.00");
                editToggle.isOn = isEditable;
                RefreshConstructorPanel();
            }
            else
            {
                expandPanel.SetActive(false);
                expandBtn.gameObject.SetActive(true);
            }
            SetUIActive(true);
        }
        else
        {
            SetUIActive(false);
        }
    }

    /// <summary>
    /// 刷新组件面板
    /// </summary>
    void RefreshConstructorPanel()
    {
        constructorPanel.SetActive(isEditable);
        if (isEditable)
        {
            ConstructorBase chassisConstructor = GamePlayController.Instance.pickedChampion.GetChassisConstructor();
            chassisSlot.ChassisConstructorSlotInit(this, chassisConstructor);
        }
    }

    /// <summary>
    /// 递归清理所有子部件槽
    /// </summary>
    /// <param name="slot">需要清理的父部件</param>
    public void ClearAllSub(ConstructorTreeViewSlot slot)
    {
        foreach (var c in slot.children)
        {
            ClearAllSub(c);
        }
        slot.ClearSubSlot();
    }

    /// <summary>
    /// 从对象池取出新的部件槽UI
    /// </summary>
    /// <returns></returns>
    public GameObject NewConstructorSlot()
    {
        if (DisableSlotsParent.childCount > 0)
        {
            return DisableSlotsParent.GetChild(0).gameObject;
        }
        //如果对象池没有多余的 实例化新槽位
        GameObject instance = Instantiate(constructorSlotPrefab, DisableSlotsParent);
        return instance;
    }

    /// <summary>
    /// 显示被选中槽位UI的选中框
    /// </summary>
    /// <param name="slot"></param>
    public void ShowPickedSlotFrame(ConstructorSlot slot)
    {
        chassisSlot.FindPickedSlot(slot);
        if (pickedSlot != null)
        {
            pickedSlot.pickedFrame.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 关闭被选中槽位UI的选中框
    /// </summary>
    public void ClosePickedSlotFrame()
    {
        if (pickedSlot != null)
        {
            pickedSlot.pickedFrame.gameObject.SetActive(false);
            pickedSlot = null;
        }
    }
}
