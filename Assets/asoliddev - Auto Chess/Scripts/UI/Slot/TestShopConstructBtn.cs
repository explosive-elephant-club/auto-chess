using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using UnityEngine.EventSystems;

public class TestShopConstructBtn : MonoBehaviour
{
    public Image iconImage;
    public Image levelFrameImage;
    public GameObject[] typeIconArray;
    public Text nameText;
    public Text typeText;
    public Text buyCostText;

    public ConstructorBaseData constructorData;
    int cost;

    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void LoadIcon()
    {
        Sprite _icon = Resources.Load<Sprite>(GamePlayController.Instance.GetConstructorIconPath(constructorData));
        iconImage.sprite = _icon;
    }

    private void Start()
    {
    }

    public void OnClicked()
    {
        UIController.Instance.inventoryController.AddConstructor(constructorData, true);
        UIController.Instance.inventoryController.UpdateUI();
    }


    public void Refresh(ConstructorBaseData data)
    {
        if (data.ID > 200)
            return;
        constructorData = data;
        cost = Mathf.CeilToInt
        (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(constructorData.type).cost *
         GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(constructorData.level).cost);
        LoadIcon();
        nameText.text = "No" + data.ID + ":" + constructorData.name;
        typeText.text = constructorData.type.ToString();
        buyCostText.text = cost.ToString();

        Color tempColor;
        if (ColorUtility.TryParseHtmlString(GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(constructorData.level).color, out tempColor))
            levelFrameImage.color = tempColor;
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
