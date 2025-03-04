using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.Linq;
using Game;


public class InventoryController : BaseControllerUI
{
    public List<InventoryConstructor> pickedConstructors = new List<InventoryConstructor>();

    public InventorySlot pointEnterInventorySlot;
    public List<InventorySlot> inventorySlots;

    public int newCount;

    [Serializable]
    public class typeToggle
    {
        public ConstructorType type;
        public Toggle toggle;
    }

    public List<typeToggle> typeToggles;

    string inventorySlotPath = "UI/Slot/InventorySlot";


    #region 自动绑定
    private Button _btnButtonClose;
    private Button _btnPickAllBtn;
    private Button _btnCancelAllBtn;
    private Button _btnName;
    private Button _btnType;
    private Button _btnCost;
    private Button _btnBonusTypeBar;
    private Button _btnSlotsBar;
    private Image _imgButtonClose;
    private Image _imgPickAllBtn;
    private Image _imgCancelAllBtn;
    public Image _imgViewport;
    private UICustomText _textName;
    private UICustomText _textType;
    private UICustomText _textCost;
    private UICustomText _textBonusTypeBar;
    private UICustomText _textSlotsBar;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _btnButtonClose = transform.Find("ButtonClose_Auto").GetComponent<Button>();
        _btnPickAllBtn = transform.Find("BG/TypePick_Auto/PickAllBtn_Auto").GetComponent<Button>();
        _btnCancelAllBtn = transform.Find("BG/TypePick_Auto/CancelAllBtn_Auto").GetComponent<Button>();
        _btnName = transform.Find("BG/Title/Name_Auto").GetComponent<Button>();
        _btnType = transform.Find("BG/Title/Type_Auto").GetComponent<Button>();
        _btnCost = transform.Find("BG/Title/Cost_Auto").GetComponent<Button>();
        _btnBonusTypeBar = transform.Find("BG/Title/BonusTypeBar_Auto").GetComponent<Button>();
        _btnSlotsBar = transform.Find("BG/Title/SlotsBar_Auto").GetComponent<Button>();
        _imgButtonClose = transform.Find("ButtonClose_Auto").GetComponent<Image>();
        _imgPickAllBtn = transform.Find("BG/TypePick_Auto/PickAllBtn_Auto").GetComponent<Image>();
        _imgCancelAllBtn = transform.Find("BG/TypePick_Auto/CancelAllBtn_Auto").GetComponent<Image>();
        _imgViewport = transform.Find("BG/InventorySlots/Viewport_Auto").GetComponent<Image>();
        _textName = transform.Find("BG/Title/Name_Auto").GetComponent<UICustomText>();
        _textType = transform.Find("BG/Title/Type_Auto").GetComponent<UICustomText>();
        _textCost = transform.Find("BG/Title/Cost_Auto").GetComponent<UICustomText>();
        _textBonusTypeBar = transform.Find("BG/Title/BonusTypeBar_Auto").GetComponent<UICustomText>();
        _textSlotsBar = transform.Find("BG/Title/SlotsBar_Auto").GetComponent<UICustomText>();
    }
    #endregion

    public override void Awake()
    {
        base.Awake();
        Init();
        foreach (Transform child in _imgViewport.transform.Find("Content"))
        {
            inventorySlots.Add(child.gameObject.GetComponent<InventorySlot>());
        }
    }

    void Start()
    {
        AddAllListener();
        UpdateUI();
    }

    public void AddAllListener()
    {
        _btnPickAllBtn.onClick.AddListener(() =>
        {
            foreach (var t in typeToggles)
            {
                t.toggle.isOn = true;
            }
            UpdateUI();
        });
        _btnCancelAllBtn.onClick.AddListener(() =>
        {
            foreach (var t in typeToggles)
            {
                t.toggle.isOn = false;
            }
            UpdateUI();
        });
        foreach (var t in typeToggles)
        {
            t.toggle.onValueChanged.AddListener((bool b) =>
            {
                UpdateUI();
            });
        }
        _btnButtonClose.onClick.AddListener(() =>
        {
            isExpand = false;
            UpdateUI();
        });
    }

    public void OnPointEnterSlot(InventorySlot inventorySlot)
    {
        pointEnterInventorySlot = inventorySlot;
        if (pointEnterInventorySlot.inventoryConstructor != null)
            UIController.Instance.popupController.constructorPopup.Show
                (pointEnterInventorySlot.inventoryConstructor.constructorBaseData, pointEnterInventorySlot.gameObject, Vector3.left);
    }

    public void OnPointLeaveSlot()
    {
        pointEnterInventorySlot = null;
        UIController.Instance.popupController.constructorPopup.Clear();
    }

    public override void UpdateUI()
    {
        if (isExpand)
        {
            SetUIActive(true);
        }
        else
        {
            SetUIActive(false);
        }
        GetPickedConstructors();
        UpdateNewCount();
        UpdateInventorySlots();
    }

    /// <summary>
    /// 标记新添加的部件
    /// </summary>
    public void UpdateNewCount()
    {
        newCount = GameData.Instance.allInventoryConstructors.FindAll(c => c.isNew).Count;
        UIController.Instance.levelInfo.UpdateInventoryPointTip(newCount);
    }

    /// <summary>
    /// 刷新仓库部件列表
    /// </summary>
    public void UpdateInventorySlots()
    {
        //检查slot对象池中的数量，如果数量不足，则实例化新的slot
        int n = pickedConstructors.Count - inventorySlots.Count;
        if (n > 0)
        {
            for (var i = 0; i < n; i++)
            {
                InventorySlot slot = ResourceManager.LoadGameObjectResource(inventorySlotPath, _imgViewport.transform.Find("Content")).GetComponent<InventorySlot>();
                inventorySlots.Add(slot);
            }

        }
        //对每个slot重新初始化，隐藏多余的slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].SetUIActive(false);
            if (i < pickedConstructors.Count)
            {
                inventorySlots[i].SetUIActive(true);
                inventorySlots[i].Init(pickedConstructors[i]);
            }
        }
    }

    public void AddConstructors(List<ConstructorBaseData> constructorBaseDatas)
    {
        foreach (var data in constructorBaseDatas)
        {
            AddConstructor(data, false);
            UpdateUI();
        }
    }

    public void AddConstructor(InventoryConstructor inventoryConstructor)
    {
        GameData.Instance.allInventoryConstructors.Add(inventoryConstructor);
    }

    public void AddConstructor(ConstructorBaseData constructorBaseData, bool isNew)
    {
        GameData.Instance.allInventoryConstructors.Add(new InventoryConstructor(constructorBaseData, isNew));
        UpdateNewCount();
    }

    public void AddConstructor(int id)
    {
        AddConstructor(GameExcelConfig.Instance.constructorsArray.Find(c => c.ID == id), true);
    }

    public void RemoveConstructor(InventoryConstructor inventoryConstructor)
    {
        GameData.Instance.allInventoryConstructors.Remove(inventoryConstructor);
        UpdateUI();
    }

    /// <summary>
    /// 获取仓库中所有被选中的部件
    /// </summary>
    void GetPickedConstructors()
    {
        List<ConstructorType> types = new List<ConstructorType>();
        foreach (var t in typeToggles)
        {
            if (t.toggle.isOn && !types.Contains(t.type))
            {
                types.Add(t.type);
            }
        }
        pickedConstructors = GameData.Instance.allInventoryConstructors.FindAll(c => IsTypeEqual(c.constructorBaseData, types));
    }

    bool IsTypeEqual(ConstructorBaseData constructorBaseData, List<ConstructorType> types)
    {
        foreach (var t in types)
        {
            if (t.ToString() == constructorBaseData.type)
                return true;
        }
        return false;
    }
}

