using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using General;
using Game;

public class ShopConstructController : BaseControllerUI
{
    public List<ShopConstructBtn> shopConstructBtnList = new List<ShopConstructBtn>();
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


    public override void Awake()
    {
        base.Awake();
        foreach (Transform child in _layoutGroupConstructsContent.transform)
        {
            shopConstructBtnList.Add(child.GetComponent<ShopConstructBtn>());
        }

        _btnLockBtn.onClick.AddListener(OnLockBtnClicked);
        _btnRefreshBtn.onClick.AddListener(() =>
        {
            RefreshShop(false);
        });
    }

    #region 自动绑定
    private Button _btnName;
    private Button _btnType;
    private Button _btnCost;
    private Button _btnBonusTypeBar;
    private Button _btnSlotsBar;
    private Button _btnRefreshBtn;
    private Button _btnLockBtn;
    private Image _imgRefreshBtn;
    private Image _imgLockBtn;
    private Image _imgLockPanel;
    private Image _imgUnlockPanel;
    private UICustomText _textName;
    private UICustomText _textType;
    private UICustomText _textCost;
    private UICustomText _textBonusTypeBar;
    private UICustomText _textSlotsBar;
    private UICustomText _textCostText;
    private VerticalLayoutGroup _layoutGroupConstructsContent;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _btnName = transform.Find("Title/Name_Auto").GetComponent<Button>();
        _btnType = transform.Find("Title/Type_Auto").GetComponent<Button>();
        _btnCost = transform.Find("Title/Cost_Auto").GetComponent<Button>();
        _btnBonusTypeBar = transform.Find("Title/BonusTypeBar_Auto").GetComponent<Button>();
        _btnSlotsBar = transform.Find("Title/SlotsBar_Auto").GetComponent<Button>();
        _btnRefreshBtn = transform.Find("Menu/RefreshBtn_Auto").GetComponent<Button>();
        _btnLockBtn = transform.Find("Menu/LockBtn_Auto").GetComponent<Button>();
        _imgRefreshBtn = transform.Find("Menu/RefreshBtn_Auto").GetComponent<Image>();
        _imgLockBtn = transform.Find("Menu/LockBtn_Auto").GetComponent<Image>();
        _imgLockPanel = transform.Find("Menu/LockBtn_Auto/LockPanel_Auto").GetComponent<Image>();
        _imgUnlockPanel = transform.Find("Menu/LockBtn_Auto/UnlockPanel_Auto").GetComponent<Image>();
        _textName = transform.Find("Title/Name_Auto").GetComponent<UICustomText>();
        _textType = transform.Find("Title/Type_Auto").GetComponent<UICustomText>();
        _textCost = transform.Find("Title/Cost_Auto").GetComponent<UICustomText>();
        _textBonusTypeBar = transform.Find("Title/BonusTypeBar_Auto").GetComponent<UICustomText>();
        _textSlotsBar = transform.Find("Title/SlotsBar_Auto").GetComponent<UICustomText>();
        _textCostText = transform.Find("Menu/RefreshBtn_Auto/HorizontalGroup/CostText_Auto").GetComponent<UICustomText>();
        _layoutGroupConstructsContent = transform.Find("Constructs/ConstructsContent_Auto").GetComponent<VerticalLayoutGroup>();
    }
    #endregion



    void Start()
    {
        UpdateUI();
        _textCostText.text = GameConfig.Instance.refreshCost.ToString();
        normalConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 1 && c.type != "Isolate");
        rareConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 2 && c.type != "Isolate");
        specialConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 3 && c.type != "Isolate");
        epicConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 4 && c.type != "Isolate");
        legenConstructors = GameExcelConfig.Instance.constructorsArray.FindAll(c => c.level == 5 && c.type != "Isolate");
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    public override void UpdateUI()
    {

        if (isLocked)
        {
            _imgUnlockPanel.gameObject.SetActive(true);
            _imgLockPanel.gameObject.SetActive(false);
        }
        else
        {
            _imgLockPanel.gameObject.SetActive(true);
            _imgUnlockPanel.gameObject.SetActive(false);
        }
    }

    public void OnLockBtnClicked()
    {
        isLocked = !isLocked;
        Debug.Log("OnLockBtnClicked " + isLocked);
        UpdateUI();
        foreach (var btn in shopConstructBtnList)
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
        for (int i = 0; i < shopConstructBtnList.Count; i++)
        {
            shopConstructBtnList[i].gameObject.SetActive(false);
            if (i < tradeLevelData.saleCount)
            {
                shopConstructBtnList[i].gameObject.SetActive(true);
                shopConstructBtnList[i].Refresh(GetRandomChampionInfo(), isLocked);
            }
        }

        //decrase gold
        if (isFree == false)
            GameData.Instance.currentGold -= GameConfig.Instance.refreshCost;

        //update ui
        UpdateUI();
        GeneralMethod.ForceRefreshContentSizeFitterUpwards(_layoutGroupConstructsContent.transform);
    }

    public void AddShopSlot()
    {
        AddSlotSuccess(GetRandomChampionInfo());
    }

    public void AddSlotSuccess(ConstructorBaseData data)
    {
        shopConstructBtnList[tradeLevelData.saleCount - 1].gameObject.SetActive(true);
        shopConstructBtnList[tradeLevelData.saleCount - 1].Refresh(data, isLocked);
        GeneralMethod.ForceRefreshContentSizeFitterUpwards(_layoutGroupConstructsContent.transform);
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
