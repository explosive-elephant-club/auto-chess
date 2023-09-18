using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;

public class ChampionShopBtn : MonoBehaviour
{


    public GameObject ablePanel;
    public GameObject disablePanel;
    public GameObject lockImage;
    public Image characterImage;
    public GameObject[] typeIconArray;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI buyCostText;
    public TextMeshProUGUI addCostText;

    public ConstructorBaseData constructorData;


    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    // Update is called once per frame
    void Update()
    {

    }
    /*
    public void OnClicked()
    {
        if (ablePanel.activeSelf)
            ChampionShop.Instance.OnChampionClicked(constructorData, this);
        else if (disablePanel.activeSelf)
            ChampionShop.Instance.AddShopSlot();
    }

    public void Onlocked(bool isLocked)
    {
        lockImage.SetActive(isLocked);
    }

    public void BuySuccessHide()
    {
        ablePanel.SetActive(false);
        disablePanel.SetActive(false);
    }

    public void Refresh(ChampionBaseData data)
    {
        ablePanel.SetActive(true);
        disablePanel.SetActive(false);
        championData = data;
        nameText.text = championData.name;
        buyCostText.text = championData.cost.ToString();
        UpdateType();
    }

    public void ShowAdd()
    {
        ablePanel.SetActive(false);
        disablePanel.SetActive(true);
        addCostText.text = GamePlayController.Instance.addSlotCostList[ChampionShop.Instance.curShopChampionLimit - 3].ToString();
    }

    public void UpdateType()
    {
        List<ChampionType> types = GamePlayController.Instance.GetAllChampionTypes(championData);
        for (int i = 0; i < typeIconArray.Length; i++)
        {
            typeIconArray[i].SetActive(false);
            if (i < types.Count)
            {
                typeIconArray[i].SetActive(true);
                typeIconArray[i].GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>(types[i].icon);
            }
        }
    }*/
}
