using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ExcelConfig;

/// <summary>
/// Updates and controls UI elements
/// </summary>
public class UIController : CreateSingleton<UIController>
{
    public GameObject[] bonusPanels;


    public Text timerText;
    public Text championCountText;
    public Text goldText;
    public Text hpText;

    public ShopGUIController shop;
    public GameObject restartButton;
    public GameObject placementText;
    public GameObject gold;
    public GameObject bonusContainer;
    public GameObject bonusUIPrefab;

    protected override void InitSingleton()
    {

    }

    /// <summary>
    /// Called when restart button clicked on UI
    /// </summary>
    public void Restart_Click()
    {
        GamePlayController.Instance.RestartGame();
    }

    /// <summary>
    /// Updates ui when needed
    /// </summary>
    public void UpdateUI()
    {
        goldText.text = GamePlayController.Instance.currentGold.ToString();
        championCountText.text = GamePlayController.Instance.ownChampionManager.currentChampionCount.ToString()
            + " / " + GamePlayController.Instance.ownChampionManager.currentChampionLimit.ToString();
        hpText.text = "HP " + GamePlayController.Instance.currentHP.ToString();

        shop.UpdateUI();

        //hide bonusus UI
        foreach (GameObject go in bonusPanels)
        {
            go.SetActive(false);
        }


        //if not null
        if (GamePlayController.Instance.ownChampionManager.championTypeCount != null)
        {
            int i = 0;
            //iterate bonuses
            foreach (KeyValuePair<ChampionType, int> m in GamePlayController.Instance.ownChampionManager.championTypeCount)
            {
                //Now you can access the key and value both separately from this attachStat as:
                GameObject bonusUI = bonusPanels[i];
                bonusUI.transform.SetParent(bonusContainer.transform);
                bonusUI.transform.Find("icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(m.Key.icon);
                bonusUI.transform.Find("name").GetComponent<Text>().text = m.Key.name;
                bonusUI.transform.Find("count").GetComponent<Text>().text = m.Value.ToString();

                bonusUI.SetActive(true);

                i++;
            }
        }
    }

    /// <summary>
    /// updates timer
    /// </summary>
    public void UpdateTimerText()
    {
        timerText.text = GamePlayController.Instance.timerDisplay.ToString();
    }

    /// <summary>
    /// sets timer visibility
    /// </summary>
    /// <param name="b"></param>
    public void SetTimerTextActive(bool b)
    {
        timerText.gameObject.SetActive(b);

        placementText.SetActive(b);
    }

    /// <summary>
    /// displays loss screen when game ended
    /// </summary>
    public void ShowLossScreen()
    {
        SetTimerTextActive(false);
        gold.SetActive(false);


        restartButton.SetActive(true);
    }

    /// <summary>
    /// displays game screen when game started
    /// </summary>
    public void ShowGameScreen()
    {
        SetTimerTextActive(true);
        gold.SetActive(true);


        restartButton.SetActive(false);
    }

}
