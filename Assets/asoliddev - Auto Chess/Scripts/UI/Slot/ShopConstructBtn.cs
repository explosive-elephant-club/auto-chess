using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using UnityEngine.EventSystems;
using Game;

public class ShopConstructBtn : ContainerSlot
{
    public List<SingleMFInfo> singleMFInfoList;
    public List<SlotInfo> slotInfoList;
    public Image[] BGImages;

    public ConstructorBaseData constructorData;

    int cost;

    #region 自动绑定
    private Image _imgPanel;
    private Image _imgIconAndName;
    private Image _imgIcon;
    private Image _imgLock;
    private Image _imgType;
    private Image _imgCost;
    private Image _imgBonusTypeBar;
    private Image _imgSlotsBar;
    private UICustomText _textNameText;
    private UICustomText _textTypeText;
    private UICustomText _textCostText;
    private HorizontalLayoutGroup _layoutGroupPanel;
    private HorizontalLayoutGroup _layoutGroupBonusTypeBar;
    private HorizontalLayoutGroup _layoutGroupSlotsBar;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _imgPanel = transform.Find("Panel_Auto").GetComponent<Image>();
        _imgIconAndName = transform.Find("Panel_Auto/IconAndName_Auto").GetComponent<Image>();
        _imgIcon = transform.Find("Panel_Auto/IconAndName_Auto/Icon_Auto").GetComponent<Image>();
        _imgLock = transform.Find("Panel_Auto/IconAndName_Auto/Lock_Auto").GetComponent<Image>();
        _imgType = transform.Find("Panel_Auto/Type_Auto").GetComponent<Image>();
        _imgCost = transform.Find("Panel_Auto/Cost_Auto").GetComponent<Image>();
        _imgBonusTypeBar = transform.Find("Panel_Auto/BonusTypeBar_Auto").GetComponent<Image>();
        _imgSlotsBar = transform.Find("Panel_Auto/SlotsBar_Auto").GetComponent<Image>();
        _textNameText = transform.Find("Panel_Auto/IconAndName_Auto/NameText_Auto").GetComponent<UICustomText>();
        _textTypeText = transform.Find("Panel_Auto/Type_Auto/TypeText_Auto").GetComponent<UICustomText>();
        _textCostText = transform.Find("Panel_Auto/Cost_Auto/CostText_Auto").GetComponent<UICustomText>();
        _layoutGroupPanel = transform.Find("Panel_Auto").GetComponent<HorizontalLayoutGroup>();
        _layoutGroupBonusTypeBar = transform.Find("Panel_Auto/BonusTypeBar_Auto").GetComponent<HorizontalLayoutGroup>();
        _layoutGroupSlotsBar = transform.Find("Panel_Auto/SlotsBar_Auto").GetComponent<HorizontalLayoutGroup>();
    }
    #endregion


    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        foreach (Transform child in _imgBonusTypeBar.transform)
        {
            singleMFInfoList.Add(child.GetComponent<SingleMFInfo>());
        }
        foreach (Transform child in _imgSlotsBar.transform)
        {
            slotInfoList.Add(child.GetComponent<SlotInfo>());
        }
        BGImages = new Image[] { _imgPanel, _imgIconAndName, _imgType, _imgCost, _imgBonusTypeBar, _imgSlotsBar };
    }

    public void LoadIcon()
    {
        Sprite _icon = Resources.Load<Sprite>(GamePlayController.Instance.GetConstructorIconPath(constructorData));
        _imgIcon.sprite = _icon;
    }

    private void Start()
    {
        ClearAllListener();
        GetComponent<Button>().onClick.AddListener(BuyConstruct);
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);

    }

    public void Onlocked(bool isLocked)
    {
        _imgLock.gameObject.SetActive(isLocked);
    }

    public void Refresh(ConstructorBaseData data, bool isLocked)
    {
        GetComponent<Button>().interactable = true;
        foreach (var img in BGImages)
        {
            img.color = GameConfig.Instance.levelColors[data.level - 1];
        }

        constructorData = data;
        LoadIcon();
        cost = Mathf.CeilToInt
        (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(constructorData.type).cost *
         GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(constructorData.level).cost);

        _textNameText.text = constructorData.name;
        Color color = GameConfig.Instance.levelColors[constructorData.level - 1];
        _imgPanel.gameObject.SetActive(true);
        Onlocked(isLocked);
        _textTypeText.text = constructorData.type.ToString();
        _textCostText.text = cost.ToString();
        UpdateMFInfo();
        UpdateSlotInfo(constructorData);
    }
    public void BuyConstruct()
    {
        if (GameData.Instance.currentGold >= cost)
        {
            GameData.Instance.currentGold -= cost;
            UIController.Instance.levelInfoController.UpdateUI();
            UIController.Instance.inventoryController.AddConstructor(constructorData, true);
            UIController.Instance.inventoryController.UpdateUI();
            BuySuccessHide();
        }
    }

    public void UpdateMFInfo()
    {
        List<ConstructorBonus> bonus = GamePlayController.Instance.GetChampionFeatureBonus(constructorData);
        bonus.Add(GamePlayController.Instance.GetChampionManufacturerBonus(constructorData));
        for (int i = 0; i < singleMFInfoList.Count; i++)
        {
            singleMFInfoList[i].SetUIActive(false);
            if (i < bonus.Count && bonus[i] != null)
            {
                singleMFInfoList[i].Init(bonus[i]);
                singleMFInfoList[i].SetUIActive(true);

            }
        }
    }

    void UpdateSlotInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < slotInfoList.Count; i++)
        {
            slotInfoList[i].SetUIActive(false);
            if (i < constructorData.slots.Length && constructorData.slots[0] != 0)
            {
                slotInfoList[i].GetComponent<SlotInfo>().Init(constructorData.slots[i]);
                slotInfoList[i].SetUIActive(true);
            }
        }
        //slotContent.gameObject.SetActive(slotInfoList.Count > 0);
    }
    public void BuySuccessHide()
    {
        _imgPanel.gameObject.SetActive(false);
        GetComponent<Button>().interactable = false;
    }
    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.shopController.shopConstructController.OnPointEnterSlot(this);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.shopController.shopConstructController.OnPointLeaveSlot();
    }

}
