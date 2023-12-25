using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;

public class LevelInfoController : MonoBehaviour
{
    public TextMeshProUGUI mapNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI gameStateText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI HPText;
    public Button readyBtn;
    public Button shopBtn;
    public RectTransform Panel1;
    public RectTransform Panel2;

    // Start is called before the first frame update
    void Start()
    {
        readyBtn.onClick.AddListener(OnReadyBtnClicked);
        shopBtn.onClick.AddListener(OnShopBtnClicked);
        ResetReadyBtn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateUI()
    {
        goldText.text = GameData.Instance.currentGold.ToString();
        HPText.text = GameData.Instance.currentHP.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Panel1);
        LayoutRebuilder.ForceRebuildLayoutImmediate(Panel2);
    }

    public void OnShopBtnClicked()
    {
        UIController.Instance.shopController.gameObject.SetActive
            (!UIController.Instance.shopController.gameObject.activeSelf);
    }
    public void OnReadyBtnClicked()
    {
        GamePlayController.Instance.StageChange(GameStage.Combat);
        readyBtn.interactable = false;
    }

    public void UpdateCombatTimer(int time)
    {
        readyBtn.GetComponentInChildren<TextMeshProUGUI>().text = time.ToString();
    }

    public void ResetReadyBtn()
    {
        readyBtn.interactable = true;
        readyBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Ready";
    }
}
