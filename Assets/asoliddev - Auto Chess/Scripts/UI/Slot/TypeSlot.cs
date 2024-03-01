using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;
using System;

[Serializable]
public class TypeCountCube
{
    public GameObject[,] cubes;

    public TypeCountCube(Transform lvl)
    {
        cubes = new GameObject[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                cubes[i, j] = lvl.GetChild(i).GetChild(j).gameObject;
            }
        }
    }
}

public class TypeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TypeCountCube typeCountCube;

    public ConstructorBonusType constructorBonusType;
    public int curCount;

    bool isShowPopup = false;

    // Start is called before the first frame update
    void Start()
    {
        typeCountCube = new TypeCountCube(transform.Find("Lvl"));
    }

    public void Init(ConstructorBonusType _constructorBonusType, int _curCount, bool _isShowPopup)
    {
        UpdateUI(_constructorBonusType, _curCount);
        isShowPopup = _isShowPopup;
    }

    public void UpdateUI(ConstructorBonusType _constructorBonusType, int _curCount)
    {
        constructorBonusType = _constructorBonusType;
        curCount = _curCount;
        icon.sprite = Resources.Load<Sprite>(constructorBonusType.icon);

        int cubeCount = 0;
        for (int i = 0; i < 3; i++)
        {
            typeCountCube.cubes[i, 0].transform.parent.gameObject.SetActive(false);
            if (i < constructorBonusType.Bonus.Length && constructorBonusType.Bonus[i].count != 0)
            {
                typeCountCube.cubes[i, 0].transform.parent.gameObject.SetActive(true);
                for (int j = 0; j < 3; j++)
                {
                    typeCountCube.cubes[i, j].SetActive(false);
                    if (j < constructorBonusType.Bonus[i].count)
                    {
                        typeCountCube.cubes[i, j].SetActive(true);
                        cubeCount++;
                        if (cubeCount > curCount)
                        {
                            typeCountCube.cubes[i, j].transform.GetChild(0).gameObject.SetActive(false);
                        }
                        else
                        {
                            typeCountCube.cubes[i, j].transform.GetChild(0).gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isShowPopup)
            UIController.Instance.popupController.typePopup.Show
                (constructorBonusType, curCount, this.gameObject, Vector3.right);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isShowPopup)
            UIController.Instance.popupController.typePopup.Clear();
    }
}
