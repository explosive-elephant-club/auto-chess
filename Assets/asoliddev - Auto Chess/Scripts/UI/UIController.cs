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

    public GameObject mask;
    public ShopGUIController shop;
    public LevelInfoController levelInfo;
    public GameObject restartButton;
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
        levelInfo.UpdateUI();
        //shop.UpdateUI();

        //hide bonusus UI
        foreach (GameObject go in bonusPanels)
        {
            go.SetActive(false);
        }


        if (GamePlayController.Instance.ownChampionManager.constructorTypeCount != null)
        {
            int i = 0;
            //iterate bonuses
            foreach (KeyValuePair<ConstructorBonusType, int> m in GamePlayController.Instance.ownChampionManager.constructorTypeCount)
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
    /// displays loss screen when game ended
    /// </summary>
    public void ShowLossScreen()
    {
        mask.SetActive(true);
        restartButton.SetActive(true);
    }

    /// <summary>
    /// displays game screen when game started
    /// </summary>
    public void ShowGameScreen()
    {
        mask.SetActive(false);
        restartButton.SetActive(false);
    }

}
