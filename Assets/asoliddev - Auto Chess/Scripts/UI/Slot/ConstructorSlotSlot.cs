using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;
using UnityEngine.EventSystems;

public class ConstructorSlotSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI slotName;
    public GameObject forbidden;
    public GameObject picked;

    ConstructorSlot slot;
    ConstructorSlotType slotTypeData;
    void Start()
    {

    }

    public void Init(int id)
    {
        slot = null;
        slotTypeData = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == id);
        slotName.text = slotTypeData.name;
        forbidden.SetActive(false);
        picked.SetActive(false);
    }
    public void Init(ConstructorSlot _slot)
    {
        slot = _slot;
        slotTypeData = slot.slotType;
        slotName.text = slotTypeData.name;
        forbidden.SetActive(false);
        picked.SetActive(false);
        if (slot != null)
        {
            if (!slot.isAble)
            {
                forbidden.SetActive(true);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIController.Instance.popupController.constructorSlotPopup.Show
            (slotTypeData, this.gameObject, Vector3.right);
        if (slot != null)
            UIController.Instance.constructorAssembleController.ShowPickedSlot(slot);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIController.Instance.popupController.constructorSlotPopup.Clear();
        if (slot != null)
            UIController.Instance.constructorAssembleController.ClearPickedSlot();
    }
}
