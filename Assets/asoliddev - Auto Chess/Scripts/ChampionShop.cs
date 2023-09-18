using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

/// <summary>
/// Creates and stores champions available, XP and LVL purchase
/// </summary>
public class ChampionShop : CreateSingleton<ChampionShop>
{
    [HideInInspector]
    public int curShopChampionLimit = 3;
    [HideInInspector]
    public bool isLocked = false;
    protected override void InitSingleton()
    {

    }

    /// Start is called before the first frame update
    void Start()
    {
        //RefreshShop(true);
    }

    /// Update is called once per frame
    void Update()
    {

    }

    /*
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

            if (isLocked && isFree)
                return;


            //fill up shop
            for (int i = 0; i < UIController.Instance.shop.championsBtnArray.Length; i++)
            {
                UIController.Instance.shop.championsBtnArray[i].gameObject.SetActive(false);
                if (i < curShopChampionLimit)
                {
                    UIController.Instance.shop.championsBtnArray[i].gameObject.SetActive(true);
                    UIController.Instance.shop.championsBtnArray[i].Refresh(GetRandomChampionInfo());
                }
            }
            if (curShopChampionLimit < 7)
            {
                UIController.Instance.shop.championsBtnArray[curShopChampionLimit].gameObject.SetActive(true);
                UIController.Instance.shop.championsBtnArray[curShopChampionLimit].ShowAdd();
            }


            //decrase gold
            if (isFree == false)
                GamePlayController.Instance.currentGold -= 2;

            //update ui
            UIController.Instance.UpdateUI();
        }

        public void AddShopSlot()
        {
            //return if we dont have enough gold
            if (GamePlayController.Instance.currentGold < GamePlayController.Instance.addSlotCostList[curShopChampionLimit - 3])
                return;

            if (curShopChampionLimit < 7)
            {
                curShopChampionLimit++;
                GamePlayController.Instance.currentGold -= GamePlayController.Instance.addSlotCostList[curShopChampionLimit - 3];

                UIController.Instance.UpdateUI();
                UIController.Instance.shop.AddSlotSuccess(GetRandomChampionInfo());
            }
        }

        public void SwitchLock()
        {
            isLocked = !isLocked;
            UIController.Instance.UpdateUI();
        }

        /// <summary>
        /// Called when ui champion frame clicked
        /// </summary>
        /// <param name="index"></param>
        public void OnChampionClicked(ChampionBaseData data, ChampionShopBtn championBtn)
        {
            bool isSucces = GamePlayController.Instance.BuyChampionFromShop(data);

            if (isSucces)
                championBtn.BuySuccessHide();
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

    */
}
