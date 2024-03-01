using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using UnityEditor;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;

public class TypePopup : Popup
{
    public TextMeshProUGUI typeName;
    public TextMeshProUGUI description;
    public TypeSlot selfSlot;

    public Transform constructorContent;
    public List<GameObject> constructorInfo;

    void Start()
    {
        foreach (Transform child in constructorContent)
        {
            constructorInfo.Add(child.gameObject);
        }
    }

    public void Show(ConstructorBonusType _constructorBonusType, int _curCount, GameObject targetUI, Vector3 dir)
    {
        typeName.text = _constructorBonusType.name;
        description.text = _constructorBonusType.description;
        selfSlot.Init(_constructorBonusType, _curCount, false);
        base.Show(targetUI, dir);

        UpdateConstructorInfo(_constructorBonusType);
    }

    void UpdateConstructorInfo(ConstructorBonusType _constructorBonusType)
    {
        if (GamePlayController.Instance.pickedChampion != null)
        {
            constructorContent.gameObject.SetActive(true);
            List<ConstructorBase> constructors = GamePlayController.Instance.pickedChampion.constructors.FindAll
                (c => c.constructorData.property1 == _constructorBonusType.name ||
                    c.constructorData.property2 == _constructorBonusType.name ||
                        c.constructorData.property3 == _constructorBonusType.name);
            for (int i = 0; i < constructorInfo.Count; i++)
            {
                constructorInfo[i].SetActive(false);
                if (i < constructors.Count)
                {
                    string iconPath = constructors[i].constructorData.prefab.Substring(0, constructors[i].constructorData.prefab.IndexOf(constructors[i].constructorData.type));
                    iconPath = "Prefab/Constructor/" + iconPath + constructors[i].constructorData.type + "/Icon/";
                    string namePath = constructors[i].constructorData.prefab.Substring(constructors[i].constructorData.prefab.IndexOf(constructors[i].constructorData.type) + constructors[i].constructorData.type.Length + 1);

                    Sprite _icon = Resources.Load<Sprite>(iconPath + namePath);
                    //Texture2D tex = AssetPreview.GetAssetPreview(constructors[i].gameObject);
                    constructorInfo[i].GetComponent<Image>().sprite = _icon;
                    constructorInfo[i].SetActive(true);
                }
            }
        }
        else
        {
            constructorContent.gameObject.SetActive(false);
        }
    }
}
