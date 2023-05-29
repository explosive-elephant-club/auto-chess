using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

/// <summary>
/// Creates and stores champions available, XP and LVL purchase
/// </summary>
public class ChampionShop : CreateSingleton<ChampionShop>
{
    ///Array to store available champions to purchase
    private ChampionBaseData[] availableChampionArray;
    protected override void InitSingleton()
    {

    }

    /// Start is called before the first frame update
    void Start()
    {
        RefreshShop(true);
    }

    /// Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Gives a level up the player
    /// </summary>
    public void BuyLvl()
    {
        GamePlayController.Instance.Buylvl();
    }

    /// <summary>
    /// Refreshes shop with new random champions
    /// </summary>
    public void RefreshShop(bool isFree)
    {
        //return if we dont have enough gold
        if (GamePlayController.Instance.currentGold < 2 && isFree == false)
            return;


        //init array
        availableChampionArray = new ChampionBaseData[5];

        //fill up shop
        for (int i = 0; i < availableChampionArray.Length; i++)
        {
            //get a random champion
            ChampionBaseData champion = GetRandomChampionInfo();

            //store champion in array
            availableChampionArray[i] = champion;

            //load champion to ui
            UIController.Instance.LoadShopItem(champion, i);

            //show shop items
            UIController.Instance.ShowShopItems();
        }

        //decrase gold
        if (isFree == false)
            GamePlayController.Instance.currentGold -= 2;

        //update ui
        UIController.Instance.UpdateUI();
    }

    /// <summary>
    /// Called when ui champion frame clicked
    /// </summary>
    /// <param name="index"></param>
    public void OnChampionFrameClicked(int index)
    {
        bool isSucces = GamePlayController.Instance.BuyChampionFromShop(availableChampionArray[index]);

        if (isSucces)
            UIController.Instance.HideChampionFrame(index);
    }

    /// <summary>
    /// Returns a random champion
    /// </summary>
    public ChampionBaseData GetRandomChampionInfo()
    {
        //randomise a number
        int rand = Random.Range(0, GameData.Instance.championsArray.Count);

        //return from array
        return GameData.Instance.championsArray[rand];
    }


}
