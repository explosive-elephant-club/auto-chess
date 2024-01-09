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

public class ConstructorSlotSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI slotName;
    public GameObject forbidden;
    public GameObject picked;

    ConstructorSlotType slotTypeData;
    void Start()
    {

    }

    public void Init(int id, ConstructorSlot slot)
    {
        slotTypeData = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == id);
        slotName.text = slotTypeData.name;
        forbidden.SetActive(false);
        picked.SetActive(false);
        if (slot != null)
        {
            if (!slot.isAble)
            {
                forbidden.SetActive(true);
            }
            if (id == slot.slotType.ID)
            {
                picked.SetActive(true);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIController.Instance.popupController.constructorSlotPopup.Show
            (slotTypeData, this.gameObject, Vector3.right);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIController.Instance.popupController.constructorSlotPopup.Clear();
    }
}
