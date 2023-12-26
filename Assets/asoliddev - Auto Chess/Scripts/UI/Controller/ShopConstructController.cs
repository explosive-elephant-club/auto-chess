using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
public class ShopConstructController : MonoBehaviour
{
    public TextMeshProUGUI refreshCostText;
    public TextMeshProUGUI championLimitText;
    public Button refreshBtn;
    public Button lockBtn;

    public GameObject unlockPanel;
    public GameObject lockPanel;

    GameObject constructsContent;

    public List<ShopConstructBtn> shopConstructBtns = new List<ShopConstructBtn>();
    public ShopConstructBtn pointEnterBtn;
    bool isLocked = false;
    private void Awake()
    {
        constructsContent = transform.Find("Constructs").gameObject;
        foreach (Transform child in constructsContent.transform)
        {
            shopConstructBtns.Add(child.GetComponent<ShopConstructBtn>());
        }

        lockBtn.onClick.AddListener(OnLockBtnClicked);
        refreshBtn.onClick.AddListener(() =>
        {
            RefreshShop(false);
        });
    }
    void Start()
    {
        UpdateUI();
        refreshCostText.text = GameConfig.Instance.refreshCost.ToString();
    }

    public void UpdateUI()
    {

        if (isLocked)
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

    public void OnLockBtnClicked()
    {
        isLocked = !isLocked;
        UpdateUI();
        foreach (var btn in shopConstructBtns)
        {
            if (btn.gameObject.activeSelf)
            {
                btn.Onlocked(isLocked);
            }
        }
    }

    public ConstructorBaseData GetRandomChampionInfo()
    {
        //randomise a number
        int rand = Random.Range(0, GameExcelConfig.Instance.constructorsArray.Count);

        //return from array
        return GameExcelConfig.Instance.constructorsArray[rand];
    }

    public void RefreshShop(bool isFree)
    {
        //return if we dont have enough gold
        if (GameData.Instance.currentGold < GameConfig.Instance.refreshCost && isFree == false)
            return;

        if (isLocked && isFree)
            return;


        //fill up shop
        for (int i = 0; i < shopConstructBtns.Count; i++)
        {
            shopConstructBtns[i].gameObject.SetActive(false);
            if (i < GameData.Instance.constructsOnSaleLimit)
            {
                shopConstructBtns[i].gameObject.SetActive(true);
                shopConstructBtns[i].Refresh(GetRandomChampionInfo());
            }
        }
        if (GameData.Instance.constructsOnSaleLimit < 7)
        {
            shopConstructBtns[GameData.Instance.constructsOnSaleLimit].gameObject.SetActive(true);
            shopConstructBtns[GameData.Instance.constructsOnSaleLimit].ShowAdd();
        }



        //decrase gold
        if (isFree == false)
            GameData.Instance.currentGold -= GameConfig.Instance.refreshCost;

        //update ui
        UpdateUI();
    }

    public void AddShopSlot()
    {
        //return if we dont have enough gold
        if (GameData.Instance.currentGold < GameConfig.Instance.addSlotCostList[GameData.Instance.constructsOnSaleLimit - 3])
            return;

        if (GameData.Instance.constructsOnSaleLimit < 7)
        {
            GameData.Instance.currentGold -= GameConfig.Instance.addSlotCostList[GameData.Instance.constructsOnSaleLimit - 3];
            GameData.Instance.constructsOnSaleLimit++;

            UIController.Instance.UpdateUI();
            AddSlotSuccess(GetRandomChampionInfo());
        }
    }

    public void AddSlotSuccess(ConstructorBaseData data)
    {
        shopConstructBtns[GameData.Instance.constructsOnSaleLimit - 1].Refresh(data);
        shopConstructBtns[GameData.Instance.constructsOnSaleLimit].gameObject.SetActive(true);
        shopConstructBtns[GameData.Instance.constructsOnSaleLimit].ShowAdd();
        LayoutRebuilder.ForceRebuildLayoutImmediate(constructsContent.GetComponent<RectTransform>());
    }

    public void BuyConstruct(ConstructorBaseData data, ShopConstructBtn btn)
    {
        if (GameData.Instance.currentGold >= data.cost)
        {
            UIController.Instance.inventoryController.AddConstructor(data);
            UIController.Instance.inventoryController.UpdateInventory();
            btn.BuySuccessHide();

        }
    }

    public void OnPointEnterSlot(ShopConstructBtn btn)
    {
        pointEnterBtn = btn;
        if (pointEnterBtn.constructorData != null)
            UIController.Instance.popupController.constructorPopup.Show
                (pointEnterBtn.constructorData, pointEnterBtn.gameObject, Vector3.right);
    }

    public void OnPointLeaveSlot()
    {
        pointEnterBtn = null;
        UIController.Instance.popupController.constructorPopup.Clear();
    }
}
