using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;

public class DataStatisticSlot : MonoBehaviour
{
    public Image characterMiniImage;
    public Text nameText;
    public GameObject[] levelStarts;
    public Image slide1;
    public Image slide2;
    public Text valueText;

    float slide1TotalValue;
    float slide2TotalValue;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitData(ChampionController ChampionController)
    {
        //nameText.text = ChampionController.champion.name;
        for (int i = 0; i < levelStarts.Length; i++)
        {
            levelStarts[i].SetActive(false);
            
        }
        slide1.fillAmount = 0;
        slide2.fillAmount = 0;

        slide1TotalValue = 0;
        slide2TotalValue = 0;

        valueText.text = "0";
    }

    public void UpdateData(float value1, float value2, float totalTeamValue)
    {

        slide1TotalValue += value1;
        slide2TotalValue += value2;

        float slide2FillAmount = (slide1TotalValue + slide2TotalValue) / totalTeamValue;
        float slide1FillAmount = slide1TotalValue / (slide1TotalValue + slide2TotalValue) * slide2FillAmount;

        slide1.fillAmount = slide1FillAmount;
        slide2.fillAmount = slide2FillAmount;
        valueText.text = ((int)(slide1TotalValue + slide2TotalValue)).ToString();
    }
}
