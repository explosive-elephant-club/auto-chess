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
    public GameObject[] championsFrameArray;
    public GameObject[] bonusPanels;


    public Text timerText;
    public Text championCountText;
    public Text goldText;
    public Text hpText;

    public GameObject shop;
    public GameObject restartButton;
    public GameObject placementText;
    public GameObject gold;
    public GameObject bonusContainer;
    public GameObject bonusUIPrefab;

    protected override void InitSingleton()
    {

    }


    /// <summary>
    /// Called when a chamipon panel clicked on shop UI
    /// </summary>
    public void OnChampionClicked()
    {
        //get clicked champion ui name
        string name = EventSystem.current.currentSelectedGameObject.transform.parent.name;

        //calculate index from name
        string defaultName = "champion container ";
        int championFrameIndex = int.Parse(name.Substring(defaultName.Length, 1));

        //message shop from click
        ChampionShop.Instance.OnChampionFrameClicked(championFrameIndex);
    }

    /// <summary>
    /// Called when refresh button clicked on shop UI
    /// </summary>
    public void Refresh_Click()
    {
        ChampionShop.Instance.RefreshShop(false);
    }

    /// <summary>
    /// Called when buyXP button clicked on shop UI
    /// </summary>
    public void BuyXP_Click()
    {
        ChampionShop.Instance.BuyLvl();
    }

    /// <summary>
    /// Called when restart button clicked on UI
    /// </summary>
    public void Restart_Click()
    {
        GamePlayController.Instance.RestartGame();
    }

    /// <summary>
    /// hides chamipon ui frame
    /// </summary>
    public void HideChampionFrame(int index)
    {
        championsFrameArray[index].transform.Find("champion").gameObject.SetActive(false);
    }

    /// <summary>
    /// make shop items visible
    /// </summary>
    public void ShowShopItems()
    {
        //unhide all champion frames
        for (int i = 0; i < championsFrameArray.Length; i++)
        {
            championsFrameArray[i].transform.Find("champion").gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// displays champion info to given index on UI
    /// </summary>
    /// <param name="champion"></param>
    /// <param name="index"></param>
    public void LoadShopItem(ChampionBaseData champion, int index)
    {
        //get unit frames
        Transform championUI = championsFrameArray[index].transform.Find("champion");
        Transform top = championUI.Find("top");
        Transform bottom = championUI.Find("bottom");
        Transform name = bottom.Find("Name");
        Transform cost = bottom.Find("Cost");
        Transform[] types = new Transform[top.Find("BG").childCount];

        name.GetComponent<Text>().text = champion.name;
        cost.GetComponent<Text>().text = champion.cost.ToString();

        List<ChampionType> championTypes = GamePlayController.Instance.GetAllChampionTypes(champion);
        for (int i = 0; i < types.Length; i++)
        {
            types[i] = top.Find("BG/type" + (i + 1).ToString());
            if (i < championTypes.Count)
            {
                types[i].gameObject.SetActive(true);
                types[i].Find("icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(championTypes[i].icon);
                types[i].Find("name").GetComponent<Text>().text = championTypes[i].name;
            }
            else
            {
                types[i].gameObject.SetActive(false);
            }
        }
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
        shop.SetActive(false);
        gold.SetActive(false);


        restartButton.SetActive(true);
    }

    /// <summary>
    /// displays game screen when game started
    /// </summary>
    public void ShowGameScreen()
    {
        SetTimerTextActive(true);
        shop.SetActive(true);
        gold.SetActive(true);


        restartButton.SetActive(false);
    }

}
