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

    TradeLevelData tradeLevelData
    {
        get { return GameConfig.Instance.GetCurTradeLevelData(); }
    }

    List<ConstructorBaseData> normalConstructors;
    List<ConstructorBaseData> rareConstructors;
    List<ConstructorBaseData> specialConstructors;
    List<ConstructorBaseData> epicConstructors;
    List<ConstructorBaseData> legenConstructors;


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
        normalConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 1);
        rareConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 2);
        specialConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 3);
        epicConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 4);
        legenConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 5);
    }

    private void OnEnable()
    {
        UpdateUI();
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
        int np = tradeLevelData.NP;
        int rp = np + tradeLevelData.RP;
        int sp = rp + tradeLevelData.SP;
        int ep = sp + tradeLevelData.EP;
        int lp = ep + tradeLevelData.LP;

        //randomise a number
        int rand = Random.Range(0, 100);
        if (rand < np)
        {
            rand = Random.Range(0, normalConstructors.Count);
            return normalConstructors[rand];
        }
        else if (rand < rp)
        {
            rand = Random.Range(0, rareConstructors.Count);
            return rareConstructors[rand];
        }
        else if (rand < sp)
        {
            rand = Random.Range(0, specialConstructors.Count);
            return specialConstructors[rand];
        }
        else if (rand < ep)
        {
            rand = Random.Range(0, epicConstructors.Count);
            return epicConstructors[rand];
        }
        else
        {
            rand = Random.Range(0, legenConstructors.Count);
            return legenConstructors[rand];
        }
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
            if (i < tradeLevelData.saleCount)
            {
                shopConstructBtns[i].gameObject.SetActive(true);
                shopConstructBtns[i].Refresh(GetRandomChampionInfo());
            }
        }

        //decrase gold
        if (isFree == false)
            GameData.Instance.currentGold -= GameConfig.Instance.refreshCost;

        //update ui
        UpdateUI();
    }

    public void AddShopSlot()
    {
        AddSlotSuccess(GetRandomChampionInfo());
    }

    public void AddSlotSuccess(ConstructorBaseData data)
    {
        shopConstructBtns[tradeLevelData.saleCount - 1].gameObject.SetActive(true);
        shopConstructBtns[tradeLevelData.saleCount - 1].Refresh(data);
        LayoutRebuilder.ForceRebuildLayoutImmediate(constructsContent.GetComponent<RectTransform>());
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
