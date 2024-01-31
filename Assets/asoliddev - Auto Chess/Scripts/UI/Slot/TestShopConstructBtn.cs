using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class TestShopConstructBtn : MonoBehaviour
{
    public Image iconImage;
    public GameObject[] typeIconArray;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI buyCostText;

    public ConstructorBaseData constructorData;


    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
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
    }

    public void OnClicked()
    {
        UIController.Instance.inventoryController.AddConstructor(constructorData);
        UIController.Instance.inventoryController.UpdateInventory();
    }


    public void Refresh(ConstructorBaseData data)
    {
        constructorData = data;
        LoadIcon();
        nameText.text = "No" + data.ID + ":" + constructorData.name;
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

}
