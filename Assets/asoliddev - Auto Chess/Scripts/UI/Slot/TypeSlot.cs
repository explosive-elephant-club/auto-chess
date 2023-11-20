using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;

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

public class TypeSlot : MonoBehaviour
{
    Image icon;
    public TypeCountCube typeCountCube;

    public ConstructorBonusType constructorBonusType;
    public int curCount;

    // Start is called before the first frame update
    void Start()
    {
        icon = transform.Find("Icon").GetComponent<Image>();
        typeCountCube = new TypeCountCube(transform.Find("Lvl"));
    }

    // Update is called once per frame
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
                            typeCountCube.cubes[i, j].transform.GetChild(0).gameObject.SetActive(true);
                        }
                        else
                        {
                            typeCountCube.cubes[i, j].transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
