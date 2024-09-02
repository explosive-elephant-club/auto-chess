using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.EventSystems;

public class ConstructorSlotPopup : Popup
{
    public TextMeshProUGUI constructorName;
    public Transform adaptTypesContent;
    public List<TextMeshProUGUI> adaptTypes;
    public Transform forbiddenChildrenTypesContent;
    public List<TextMeshProUGUI> forbiddenChildrenTypes;
    public Transform forbiddenParentsTypesContent;
    public List<TextMeshProUGUI> forbiddenParentsTypes;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in adaptTypesContent)
        {
            adaptTypes.Add(child.GetComponentInChildren<TextMeshProUGUI>());
        }
        foreach (Transform child in forbiddenChildrenTypesContent)
        {
            forbiddenChildrenTypes.Add(child.GetComponentInChildren<TextMeshProUGUI>());
        }
        foreach (Transform child in forbiddenParentsTypesContent)
        {
            forbiddenParentsTypes.Add(child.GetComponentInChildren<TextMeshProUGUI>());
        }
    }

    public void Show(ConstructorSlotType slotType, GameObject targetUI, Vector3 dir)
    {
        constructorName.text = slotType.name.ToString();
        UpdateAdaptTypes(slotType);
        UpdateForbiddenChildrenTypes(slotType);
        UpdateForbiddenParentsTypes(slotType);
        base.Show(targetUI, dir);
    }
    void UpdateAdaptTypes(ConstructorSlotType slotType)
    {
        if (!string.IsNullOrEmpty(slotType.adaptTypes[0]))
        {
            adaptTypesContent.parent.gameObject.SetActive(true);
            for (int i = 0; i < adaptTypes.Count; i++)
            {
                adaptTypes[i].transform.parent.gameObject.SetActive(false);
                if (i < slotType.adaptTypes.Length && !string.IsNullOrEmpty(slotType.adaptTypes[0]))
                {
                    adaptTypes[i].text = slotType.adaptTypes[i];
                    adaptTypes[i].transform.parent.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            adaptTypesContent.parent.gameObject.SetActive(false);
        }
    }

    void UpdateForbiddenChildrenTypes(ConstructorSlotType slotType)
    {
        if (slotType.forbiddenChildrenSlotTypes[0] != 0)
        {
            forbiddenChildrenTypesContent.parent.gameObject.SetActive(true);
            for (int i = 0; i < forbiddenChildrenTypes.Count; i++)
            {
                forbiddenChildrenTypes[i].transform.parent.gameObject.SetActive(false);
                if (slotType.isForbiddenAllChildrenSlots)
                {
                    if (i == 0)
                    {
                        forbiddenChildrenTypes[i].text = "All";
                        forbiddenChildrenTypes[i].transform.parent.gameObject.SetActive(true);
                    }
                    return;
                }
                else
                {
                    if (i < slotType.forbiddenChildrenSlotTypes.Length && slotType.forbiddenChildrenSlotTypes[0] != 0)
                    {
                        ConstructorSlotType forbiddenSlotType = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == slotType.forbiddenChildrenSlotTypes[i]);
                        forbiddenChildrenTypes[i].text = forbiddenSlotType.name.ToString();
                        forbiddenChildrenTypes[i].transform.parent.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            forbiddenChildrenTypesContent.parent.gameObject.SetActive(false);
        }

    }
    void UpdateForbiddenParentsTypes(ConstructorSlotType slotType)
    {
        if (slotType.forbiddenParentsSlotTypes[0] != 0)
        {
            forbiddenParentsTypesContent.parent.gameObject.SetActive(true);
            for (int i = 0; i < forbiddenParentsTypes.Count; i++)
            {
                forbiddenParentsTypes[i].transform.parent.gameObject.SetActive(false);
                if (slotType.isForbiddenAllParentsSlots)
                {
                    if (i == 0)
                    {
                        forbiddenParentsTypes[i].text = "All";
                        forbiddenParentsTypes[i].transform.parent.gameObject.SetActive(true);
                    }
                    return;
                }
                else
                {
                    if (i < slotType.forbiddenParentsSlotTypes.Length && slotType.forbiddenParentsSlotTypes[0] != 0)
                    {
                        ConstructorSlotType forbiddenSlotType = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == slotType.forbiddenParentsSlotTypes[i]);
                        forbiddenParentsTypes[i].text = forbiddenSlotType.name.ToString();
                        forbiddenParentsTypes[i].transform.parent.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            forbiddenParentsTypesContent.parent.gameObject.SetActive(false);
        }
    }
}
