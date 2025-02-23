using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;

public class LevelInfoController : MonoBehaviour
{
    public Text midTitleText;
    public Text midValueText;
    public Text goldText;
    public Text HPText;
    public Text robotLimitText;
    public Button readyBtn;
    public Button shopBtn;
    public Button inventoryBtn;
    public GameObject inventoryPointTip;

    // Start is called before the first frame update
    void Start()
    {
        readyBtn.onClick.AddListener(OnReadyBtnClicked);
        shopBtn.onClick.AddListener(OnShopBtnClicked);
        inventoryBtn.onClick.AddListener(OnInventoryBtnClicked);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void UpdateUI()
    {
        goldText.text = GameData.Instance.currentGold.ToString();
        HPText.text = GameData.Instance.currentHP.ToString();
        robotLimitText.text = GamePlayController.Instance.ownChampionManager.currentChampionCount.ToString() + "/" + GamePlayController.Instance.ownChampionManager.currentChampionLimit.ToString();

    }

    public void UpdateInventoryPointTip(int count)
    {
        if (count > 0)
        {
            inventoryPointTip.SetActive(true);
            inventoryPointTip.GetComponentInChildren<Text>().text = count.ToString();
        }
        else
        {
            inventoryPointTip.SetActive(false);
        }
    }

    public void OnShopBtnClicked()
    {
        UIController.Instance.shopController.isExpand = !UIController.Instance.shopController.isExpand;
        UIController.Instance.shopController.UpdateUI();
    }

    public void OnInventoryBtnClicked()
    {
        UIController.Instance.inventoryController.isExpand = !UIController.Instance.inventoryController.isExpand;
        UIController.Instance.inventoryController.UpdateUI();
    }

    public void OnReadyBtnClicked()
    {
        GamePlayController.Instance.StageChange(GameStage.Combat);
    }

    public void UpdateCombatTimer(int time)
    {
        midValueText.text = time.ToString();
    }

    public void OnEnterPreparation()
    {
        midTitleText.text = string.Format("第{0:G}关", GameData.Instance.mapLevel);
        readyBtn.gameObject.SetActive(true);
    }

    public void OnEnterCombat()
    {
        midTitleText.text = "时间";
        readyBtn.gameObject.SetActive(false);
    }
}
