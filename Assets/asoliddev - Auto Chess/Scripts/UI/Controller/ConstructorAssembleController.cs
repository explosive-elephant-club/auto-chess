using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using Game;

/// <summary>
/// 部件组装管理器
/// </summary>
public class ConstructorAssembleController : BaseControllerUI
{
    public bool isEditable = true;

    /// <summary>
    ///根节点（底盘）
    /// </summary>
    public ConstructorTreeViewSlot chassisSlot;
    /// <summary>
    /// 鼠标悬停的组件槽位UI
    /// </summary>
    public ConstructorTreeViewSlot pointEnterTreeViewSlot;

    CameraManager camController;

    //当前选中的槽位
    public ConstructorTreeViewSlot pickedSlot;

    #region 自动绑定
    private Button _btnCloseButton;
    private Button _btnSwitchUIButton;
    private Button _btnExpandButton;
    private Image _imgExpandPanel;
    private Image _imgConstructorPanel;
    private Image _imgDisableSlots;
    private Image _imgLineLayer;
    private Image _imgCloseButton;
    private Image _imgSwitchUIButton;
    private Image _imgExpandButton;
    private UICustomText _textOpenText;
    private UICustomText _textCloseText;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _btnCloseButton = transform.Find("ExpandPanel_Auto/CloseButton_Auto").GetComponent<Button>();
        _btnSwitchUIButton = transform.Find("ExpandPanel_Auto/SwitchUIButton_Auto").GetComponent<Button>();
        _btnExpandButton = transform.Find("ExpandButton_Auto").GetComponent<Button>();
        _imgExpandPanel = transform.Find("ExpandPanel_Auto").GetComponent<Image>();
        _imgConstructorPanel = transform.Find("ExpandPanel_Auto/ConstructorPanel_Auto").GetComponent<Image>();
        _imgDisableSlots = transform.Find("ExpandPanel_Auto/ConstructorPanel_Auto/DisableSlots_Auto").GetComponent<Image>();
        _imgLineLayer = transform.Find("ExpandPanel_Auto/ConstructorPanel_Auto/LineLayer_Auto").GetComponent<Image>();
        _imgCloseButton = transform.Find("ExpandPanel_Auto/CloseButton_Auto").GetComponent<Image>();
        _imgSwitchUIButton = transform.Find("ExpandPanel_Auto/SwitchUIButton_Auto").GetComponent<Image>();
        _imgExpandButton = transform.Find("ExpandButton_Auto").GetComponent<Image>();
        _textOpenText = transform.Find("ExpandPanel_Auto/SwitchUIButton_Auto/OpenText_Auto").GetComponent<UICustomText>();
        _textCloseText = transform.Find("ExpandPanel_Auto/SwitchUIButton_Auto/CloseText_Auto").GetComponent<UICustomText>();
    }
    #endregion





    private void Start()
    {
        camController = GamePlayController.Instance.cameraManager;
        AddAllListener();
        SetUIActive(false);
    }

    /// <summary>
    /// 绑定UI事件
    /// </summary>
    void AddAllListener()
    {
        //切换编辑模式
        _btnSwitchUIButton.onClick.AddListener(switchUI);
        //打开扩展
        _btnExpandButton.onClick.AddListener(Expand);
        //关闭UI
        _btnCloseButton.onClick.AddListener(Close);
    }

    void switchUI()
    {
        isEditable = !isEditable;
        RefreshConstructorPanel();
    }

    void Expand()
    {
        isExpand = true;
        camController.SetCameraController(CameraStateMachineHelper.CameraGoToUIState);
        UpdateUI();
    }

    void Close()
    {
        isExpand = false;
        camController.SetCameraController(CameraStateMachineHelper.CameraNormalState);
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
                _imgExpandPanel.gameObject.SetActive(true);
                _btnExpandButton.gameObject.SetActive(false);
                RefreshConstructorPanel();
            }
            else
            {
                _imgExpandPanel.gameObject.SetActive(false);
                _btnExpandButton.gameObject.SetActive(true);
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
        _imgConstructorPanel.gameObject.SetActive(isEditable);
        _textOpenText.gameObject.SetActive(!isEditable);
        _textCloseText.gameObject.SetActive(isEditable);
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
        GameObject instance;
        if (_imgDisableSlots.transform.childCount > 0)
        {
            instance = _imgDisableSlots.transform.GetChild(0).gameObject;
            instance.transform.parent = _imgConstructorPanel.transform;
            return instance;
        }
        //如果对象池没有多余的 实例化新槽位
        instance = ResourceManager.LoadGameObjectResource("UI/Slot/ConstructorAssembleSlot", _imgConstructorPanel.transform);

        return instance;
    }

    /// <summary>
    /// 回收部件槽UI
    /// </summary>
    /// <returns></returns>
    public void RecyclingConstructorSlot(ConstructorTreeViewSlot slot)
    {
        slot.transform.parent = _imgDisableSlots.transform;
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
            pickedSlot.SetPickedFrame(true);
        }
    }

    /// <summary>
    /// 关闭被选中槽位UI的选中框
    /// </summary>
    public void ClosePickedSlotFrame()
    {
        if (pickedSlot != null)
        {
            pickedSlot.SetPickedFrame(false);
            pickedSlot = null;
        }
    }

    /// <summary>
    /// 实例化line
    /// </summary>
    /// <returns></returns>
    public Image InstantiateLine()
    {
        return ResourceManager.LoadGameObjectResource("UI/Others/Line", _imgLineLayer.transform).GetComponent<Image>();
    }
}
