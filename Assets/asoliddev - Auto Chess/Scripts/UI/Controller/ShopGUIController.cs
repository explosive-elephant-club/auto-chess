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
    Toggle lastActivedToggle;

    public ShopConstructController shopConstructController;
    public ShopUpdateController shopUpdateController;

    GameObject lastActivedSubPanel;

    void Awake()
    {
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

    void AddAllListener()
    {
        hideBtn.onClick.AddListener(() =>
            {
                UIController.Instance.levelInfo.shopBtn.interactable = true;
                gameObject.SetActive(false);
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
        Debug.Log(toggle.GetComponentInChildren<TextMeshProUGUI>().text);
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

        gameObject.SetActive(true);
        shopConstructController.RefreshShop(false);
    }

    /*
            public void UpdateUI()
            {
                refreshCostText.text
                    = GamePlayController.Instance.levelUpCostList[GamePlayController.Instance.ownChampionManager.currentChampionLimit - 3].ToString();
                championLimitText.text = GamePlayController.Instance.ownChampionManager.currentChampionCount.ToString()
                        + " / " + GamePlayController.Instance.ownChampionManager.currentChampionLimit.ToString();
                if (Panel.gameObject.activeSelf)
                {
                    showBtn.gameObject.SetActive(false);
                    hideBtn.gameObject.SetActive(true);
                }
                else
                {
                    hideBtn.gameObject.SetActive(false);
                    showBtn.gameObject.SetActive(true);
                }
                if (ChampionShop.Instance.isLocked)
                {
                    unlockPanel.SetActive(true);
                    lockPanel.SetActive(false);
                }
                else
                {
                    lockPanel.SetActive(true);
                    unlockPanel.SetActive(false);
                }
            }

            public void OnRefreshBtnClicked()
            {
                ChampionShop.Instance.RefreshShop(false);
            }

            public void OnLevelUpBtnClicked()
            {
                ChampionShop.Instance.BuyLvl();
            }

            public void OnLockBtnClicked()
            {
                ChampionShop.Instance.SwitchLock();
                for (int i = 0; i < championsBtnArray.Length; i++)
                {
                    if (championsBtnArray[i].gameObject.activeSelf)
                    {
                        championsBtnArray[i].Onlocked(ChampionShop.Instance.isLocked);
                    }
                }
            }

            public void OnShowBtnClicked()
            {
                Panel.gameObject.SetActive(true);
                UpdateUI();
            }

            public void OnHideBtnClicked()
            {
                Panel.gameObject.SetActive(false);
                UpdateUI();
            }

            public void AddSlotSuccess(ChampionBaseData data)
            {
                championsBtnArray[ChampionShop.Instance.curShopChampionLimit - 1].Refresh(data);
                championsBtnArray[ChampionShop.Instance.curShopChampionLimit].gameObject.SetActive(true);
                championsBtnArray[ChampionShop.Instance.curShopChampionLimit].ShowAdd();
                LayoutRebuilder.ForceRebuildLayoutImmediate(Panel);
            }
            */
}
