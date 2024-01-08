using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;

public class ShopGUIController : MonoBehaviour
{
    public Button hideBtn;
    public Toggle constructToggle;
    public Toggle updateToggle;
    public Toggle relicToggle;
    public Toggle refiningToggle;
    public Toggle composeToggle;
    public Toggle lottoToggle;

    [HideInInspector]
    public CanvasGroup canvasGroup;
    Toggle lastActivedToggle;

    public ShopConstructController shopConstructController;
    public ShopUpdateController shopUpdateController;

    GameObject lastActivedSubPanel;

    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }
    void Start()
    {
        AddAllListener();
        lastActivedToggle = relicToggle;
        lastActivedSubPanel = shopUpdateController.gameObject;
        constructToggle.isOn = true;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetUIActive(bool isActive)
    {
        if (isActive)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

    }

    void AddAllListener()
    {
        hideBtn.onClick.AddListener(() =>
            {
                SetUIActive(false);
            });
        constructToggle.onValueChanged.AddListener((bool b) =>
            {
                if (b)
                {
                    OnToggleActive(constructToggle);
                    ActiveConstructPanel();
                }
            });
        updateToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                ActiveUpdatePanel();
                OnToggleActive(updateToggle);
            }
        });
        relicToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(relicToggle);
            }
        });
        refiningToggle.onValueChanged.AddListener((bool b) =>
        {
            if (b)
            {
                OnToggleActive(refiningToggle);
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
        SetUIActive(true);
        shopConstructController.RefreshShop(false);
    }
    public void OnLeavePreparation()
    {
        SetUIActive(false);
    }
}
