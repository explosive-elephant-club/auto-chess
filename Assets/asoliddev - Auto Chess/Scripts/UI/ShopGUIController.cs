using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;

public class ShopGUIController : MonoBehaviour
{
    public ChampionShopBtn[] championsBtnArray;
    public RectTransform Panel;

    public Button refreshBtn;
    public Button levelUpBtn;
    public Button lockBtn;
    public Button showBtn;
    public Button hideBtn;

    public GameObject lockPanel;
    public GameObject unlockPanel;

    public TextMeshProUGUI refreshCostText;
    public TextMeshProUGUI championLimitText;


    // Start is called before the first frame update
    void Start()
    {
        refreshBtn.onClick.AddListener(OnRefreshBtnClicked);
        levelUpBtn.onClick.AddListener(OnLevelUpBtnClicked);
        lockBtn.onClick.AddListener(OnLockBtnClicked);
        showBtn.onClick.AddListener(OnShowBtnClicked);
        hideBtn.onClick.AddListener(OnHideBtnClicked);
    }

    // Update is called once per frame
    void Update()
    {

    }

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
}
