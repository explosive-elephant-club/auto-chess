using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class ShopConstructBtn : ContainerSlot
{
    public GameObject mainPanel;
    public GameObject lockImage;
    public Image iconImage;

    public GameObject[] typeIconArray;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI buyCostText;

    public Transform slotContent;
    public List<GameObject> slotInfo;
    public Image[] bgImages;

    public ConstructorBaseData constructorData;

    int cost;

    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(BuyConstruct);
    }

    public void LoadIcon()
    {
        Sprite _icon = Resources.Load<Sprite>(GamePlayController.Instance.GetConstructorIconPath(constructorData));
        iconImage.sprite = _icon;
    }

    private void Start()
    {
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
        foreach (Transform child in slotContent)
        {
            slotInfo.Add(child.gameObject);
        }
    }

    public void Onlocked(bool isLocked)
    {
        lockImage.SetActive(isLocked);
    }
    public void Refresh(ConstructorBaseData data)
    {
        GetComponent<Button>().interactable = true;
        foreach (var img in bgImages)
        {
            img.color = GameConfig.Instance.levelColors[data.level - 1];
        }

        constructorData = data;
        LoadIcon();
        cost = Mathf.CeilToInt
        (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(constructorData.type).cost *
         GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(constructorData.level).cost);

        nameText.text = constructorData.name;
        Color color = GameConfig.Instance.levelColors[constructorData.level - 1];
        mainPanel.SetActive(true);
        typeText.text = constructorData.type.ToString();
        buyCostText.text = cost.ToString();
        UpdateType();
        UpdateSlotInfo(constructorData);
    }
    public void BuyConstruct()
    {
        if (GameData.Instance.currentGold >= cost)
        {
            GameData.Instance.currentGold -= cost;
            UIController.Instance.levelInfo.UpdateUI();
            UIController.Instance.inventoryController.AddConstructor(constructorData, true);
            UIController.Instance.inventoryController.UpdateUI();
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

    void UpdateSlotInfo(ConstructorBaseData constructorData)
    {
        for (int i = 0; i < slotInfo.Count; i++)
        {
            slotInfo[i].SetActive(false);
            if (i < constructorData.slots.Length && constructorData.slots[0] != 0)
            {
                slotInfo[i].GetComponent<ConstructorSlotSlot>().Init(constructorData.slots[i]);
                slotInfo[i].SetActive(true);
            }
        }
        slotContent.gameObject.SetActive(slotInfo.Count > 0);
    }
    public void BuySuccessHide()
    {
        mainPanel.SetActive(false);
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
