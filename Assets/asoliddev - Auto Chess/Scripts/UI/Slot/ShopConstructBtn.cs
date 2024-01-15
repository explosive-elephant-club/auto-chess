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
    public GameObject disablePanel;
    public GameObject lockImage;
    public Image iconImage;
    public GameObject[] typeIconArray;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI buyCostText;
    public TextMeshProUGUI addCostText;

    public ConstructorBaseData constructorData;


    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void LoadIcon()
    {
        string iconPath = constructorData.prefab.Substring(0, constructorData.prefab.IndexOf(constructorData.type));
        iconPath = iconPath + constructorData.type + "/Icon/";
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

    public void OnClicked()
    {
        if (ablePanel.activeSelf)
            UIController.Instance.shopController.shopConstructController.BuyConstruct(constructorData, this);
        else if (disablePanel.activeSelf)
            UIController.Instance.shopController.shopConstructController.AddShopSlot();
    }

    public void Onlocked(bool isLocked)
    {
        lockImage.SetActive(isLocked);
    }
    public void Refresh(ConstructorBaseData data)
    {
        ablePanel.SetActive(true);
        disablePanel.SetActive(false);
        constructorData = data;
        LoadIcon();
        nameText.text = constructorData.name;
        typeText.text = constructorData.type.ToString();
        buyCostText.text = constructorData.cost.ToString();
        UpdateType();
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
        disablePanel.SetActive(true);
        addCostText.text = GameConfig.Instance.addSlotCostList
            [GameData.Instance.constructsOnSaleLimit - 3].ToString();
    }
    public void BuySuccessHide()
    {
        ablePanel.SetActive(false);
        disablePanel.SetActive(false);
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
