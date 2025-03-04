using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using UnityEngine.Events;

public class ShopGUIController : BaseControllerUI
{
    public Button hideBtn;
    public Toggle constructToggle;
    public Toggle updateToggle;
    public Toggle relicToggle;
    public Toggle composeToggle;
    public Toggle lottoToggle;

    Toggle lastActivedToggle;

    public ShopConstructController shopConstructController;
    public ShopUpdateController shopUpdateController;

    GameObject lastActivedSubPanel;
    public UnityEvent contentSwitchEvent;

    public override void Awake()
    {
        base.Awake();
    }

    #region 自动绑定

    #endregion

    void Start()
    {
        AddAllListener();
        lastActivedToggle = relicToggle;
        lastActivedSubPanel = shopUpdateController.gameObject;
        constructToggle.isOn = true;
    }

    // Update is called once per frame
    public override void UpdateUI()
    {
        UIController.Instance.shopController.SetUIActive(UIController.Instance.shopController.isExpand);
    }

    void AddAllListener()
    {
        hideBtn.onClick.AddListener(() =>
            {
                isExpand = false;
                UpdateUI();
            });
        constructToggle.onValueChanged.AddListener((bool b) =>
            {
                if (b)
                {
                    OnToggleActive(constructToggle);
                    contentSwitchEvent.AddListener(() =>
                    {
                        ActiveConstructPanel();
                    });
                }
            });
        updateToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(updateToggle);
                contentSwitchEvent.AddListener(() =>
                {
                    ActiveUpdatePanel();
                });

            }
        });
        relicToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(relicToggle);
            }
        });
        composeToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(composeToggle);
            }
        });
        lottoToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(lottoToggle);
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
