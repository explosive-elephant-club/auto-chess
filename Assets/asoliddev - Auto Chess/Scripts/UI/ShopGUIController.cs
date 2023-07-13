using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;

public class ShopGUIController : MonoBehaviour
{
    public ChampionShopBtn[] championsBtnArray;

    public Button refreshBtn;
    public Button levelUpBtn;
    public Button lockBtn;
    public Button showBtn;
    public Button hideBtn;

    public TextMeshProUGUI refreshCostText;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateUI()
    {
        refreshCostText.text
            = GamePlayController.Instance.levelUpCostList[GamePlayController.Instance.ownChampionManager.currentChampionLimit - 3].ToString();
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

    }

    public void AddSlotSuccess(ChampionBaseData data)
    {
        championsBtnArray[ChampionShop.Instance.curShopChampionLimit].Refresh(data);
        championsBtnArray[ChampionShop.Instance.curShopChampionLimit + 1].gameObject.SetActive(true);
        championsBtnArray[ChampionShop.Instance.curShopChampionLimit + 1].ShowAdd();
    }
}
