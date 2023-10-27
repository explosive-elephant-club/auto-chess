using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;

public class ChampionInfoController : MonoBehaviour
{
    //State1
    public GameObject typesBar;
    public GameObject armorBar;
    public GameObject mechBar;
    public GameObject monoBar;


    //State1
    public GameObject state2;
    public GameObject[] attributeDatas;

    //Skill
    public GameObject[] activatedSkillSlots;
    public GameObject[] deactivatedSkillSlots;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateTypesBar()
    {
        for (int i = 0; i < typesBar.transform.childCount; i++)
        {
            typesBar.transform.GetChild(i);
        }
    }
}
