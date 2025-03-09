using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using UnityEngine.Events;
using Game;

public class ShopGUIController : BaseControllerUI
{

    Toggle lastActivedToggle;

    public ShopConstructController shopConstructController;
    public ShopUpdateController shopUpdateController;

    GameObject lastActivedSubPanel;
    public UnityEvent contentSwitchEvent;
    string shopConstructPath = "UI/Controller/ShopConstructController";
    string shopUpdatePath = "UI/Controller/ShopUpdateController";
    public override void Awake()
    {
        base.Awake();
        _layoutGroupPanel.gameObject.SetActive(false);
        shopConstructController = ResourceManager.LoadGameObjectResource(shopConstructPath, _layoutGroupPanel.transform)
            .GetComponent<ShopConstructController>();
        shopUpdateController = ResourceManager.LoadGameObjectResource(shopUpdatePath, _layoutGroupPanel.transform)
            .GetComponent<ShopUpdateController>();

    }

    #region 自动绑定
    private Button _btnButtonClose;
    private Image _imgButtonClose;
    private HorizontalLayoutGroup _layoutGroupPanel;
    private Toggle _toggleConstructToggle;
    private Toggle _toggleUpdateToggle;
    private Toggle _toggleRelicToggle;
    private Toggle _toggleForgeToggle;
    private Toggle _toggleCompositeToggle;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _btnButtonClose = transform.Find("ButtonClose_Auto").GetComponent<Button>();
        _imgButtonClose = transform.Find("ButtonClose_Auto").GetComponent<Image>();
        _layoutGroupPanel = transform.Find("Panel/Content/Panel_Auto").GetComponent<HorizontalLayoutGroup>();
        _toggleConstructToggle = transform.Find("Panel/Tabs/ConstructToggle_Auto").GetComponent<Toggle>();
        _toggleUpdateToggle = transform.Find("Panel/Tabs/UpdateToggle_Auto").GetComponent<Toggle>();
        _toggleRelicToggle = transform.Find("Panel/Tabs/RelicToggle_Auto").GetComponent<Toggle>();
        _toggleForgeToggle = transform.Find("Panel/Tabs/ForgeToggle_Auto").GetComponent<Toggle>();
        _toggleCompositeToggle = transform.Find("Panel/Tabs/CompositeToggle_Auto").GetComponent<Toggle>();
    }
    #endregion





    void Start()
    {
        AddAllListener();
        lastActivedToggle = _toggleForgeToggle;
        lastActivedSubPanel = shopUpdateController.gameObject;
        _toggleConstructToggle.isOn = true;
        lastActivedSubPanel.SetActive(false);
        _layoutGroupPanel.gameObject.SetActive(true);
    }

    // Update is called once per frame
    public override void UpdateUI()
    {
        UIController.Instance.shopController.SetUIActive(UIController.Instance.shopController.isExpand);
    }

    void AddAllListener()
    {
        _btnButtonClose.onClick.AddListener(() =>
            {
                isExpand = false;
                UpdateUI();
            });
        _toggleConstructToggle.onValueChanged.AddListener((bool b) =>
            {
                if (b)
                {
                    OnToggleActive(_toggleConstructToggle);
                    contentSwitchEvent.AddListener(() =>
                    {
                        ActiveConstructPanel();
                    });
                }
            });
        _toggleUpdateToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(_toggleUpdateToggle);
                contentSwitchEvent.AddListener(() =>
                {
                    ActiveUpdatePanel();
                });

            }
        });
        _toggleRelicToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(_toggleRelicToggle);
            }
        });
        _toggleForgeToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(_toggleForgeToggle);
            }
        });
        _toggleCompositeToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(_toggleCompositeToggle);
            }
        });
    }

    public void OnContentSwitchEvent()
    {
        contentSwitchEvent.Invoke();
        contentSwitchEvent.RemoveAllListeners();
    }

    void OnToggleActive(Toggle toggle)
    {
        lastActivedToggle.isOn = false;
        lastActivedToggle = toggle;
    }

    void ActiveConstructPanel()
    {
        lastActivedSubPanel.SetActive(false);
        shopConstructController.gameObject.SetActive(true);
        lastActivedSubPanel = shopConstructController.gameObject;
    }
    void ActiveUpdatePanel()
    {
        lastActivedSubPanel.SetActive(false);
        shopUpdateController.gameObject.SetActive(true);
        lastActivedSubPanel = shopUpdateController.gameObject;
    }

    public void OnEnterPreparation()
    {
        isExpand = true;
        UpdateUI();
        shopConstructController.RefreshShop(false);
    }
    public void OnLeavePreparation()
    {
        isExpand = false;
        UpdateUI();
    }
}
