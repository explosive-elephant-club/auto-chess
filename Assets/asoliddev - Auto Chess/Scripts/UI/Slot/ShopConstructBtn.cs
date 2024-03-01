using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class ShopConstructBtn : ContainerSlot
{
    public GameObject ablePanel;
    public GameObject lockImage;
    public Image iconImage;
    public Image levelFrameImage;
    public GameObject[] typeIconArray;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI buyCostText;
    public TextMeshProUGUI addCostText;

    public Sprite[] levelFrames;
    public ConstructorBaseData constructorData;

    int cost;

    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(BuyConstruct);
    }

    public void LoadIcon()
    {
        string iconPath = constructorData.prefab.Substring(0, constructorData.prefab.IndexOf(constructorData.type));
        iconPath = "Prefab/Constructor/" + iconPath + constructorData.type + "/Icon/";
        string namePath = constructorData.prefab.Substring(constructorData.prefab.IndexOf(constructorData.type) + constructorData.type.Length + 1);
        Sprite _icon = Resources.Load<Sprite>(iconPath + namePath);
        iconImage.sprite = _icon;
    }

    private void Start()
    {
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
    }

    public void Onlocked(bool isLocked)
    {
        lockImage.SetActive(isLocked);
    }
    public void Refresh(ConstructorBaseData data)
    {
        ablePanel.SetActive(true);
        constructorData = data;
        LoadIcon();
        cost = Mathf.CeilToInt
        (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(constructorData.type).cost *
         GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(constructorData.level).cost);

        nameText.text = constructorData.name;
        typeText.text = constructorData.type.ToString();
        buyCostText.text = cost.ToString();

        levelFrameImage.sprite = levelFrames[constructorData.level - 1];
        UpdateType();
    }
    public void BuyConstruct()
    {
        if (GameData.Instance.currentGold >= cost)
        {
            GameData.Instance.currentGold -= cost;
            UIController.Instance.levelInfo.UpdateUI();
            UIController.Instance.inventoryController.AddConstructor(constructorData);
            UIController.Instance.inventoryController.UpdateInventory();
            BuySuccessHide();
        }
    }

    public void UpdateType()
    {
        List<ConstructorBonusType> types = GamePlayController.Instance.GetAllChampionTypes(constructorData);

        for (int i = 0; i < typeIconArray.Length; i++)
        {
            typeIconArray[i].SetActive(false);
            if (i < types.Count && types[i] != null)
            {
                typeIconArray[i].SetActive(true);
                typeIconArray[i].GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>(types[i].icon);
            }
        }
    }
    public void ShowAdd()
    {
        ablePanel.SetActive(false);
        addCostText.text = GameConfig.Instance.addSlotCostList
            [GameData.Instance.constructsOnSaleLimit - 3].ToString();
    }
    public void BuySuccessHide()
    {
        ablePanel.SetActive(false);
    }
    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        if (ablePanel.activeSelf)
            UIController.Instance.shopController.shopConstructController.OnPointEnterSlot(this);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.shopController.shopConstructController.OnPointLeaveSlot();
    }

}
