using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;

public class LevelInfoController : MonoBehaviour
{
    public TextMeshProUGUI midTitleText;
    public TextMeshProUGUI midValueText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI HPText;
    public TextMeshProUGUI robotLimitText;
    public Button readyBtn;
    public Button shopBtn;

    // Start is called before the first frame update
    void Start()
    {
        readyBtn.onClick.AddListener(OnReadyBtnClicked);
        shopBtn.onClick.AddListener(OnShopBtnClicked);
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

    public void OnShopBtnClicked()
    {
        if (UIController.Instance.shopController.canvasGroup.interactable)
        {
            UIController.Instance.shopController.SetUIActive(false);

        }
        else
        {
            UIController.Instance.shopController.SetUIActive(true);
        }
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
