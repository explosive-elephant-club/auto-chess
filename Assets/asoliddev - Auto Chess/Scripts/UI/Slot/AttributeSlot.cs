using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using ExcelConfig;
using UnityEngine.EventSystems;

public class AttributeSlot : ContainerSlot
{
    Text valueText;
    string attributeName;

    public override void Awake()
    {
        valueText = transform.Find("Text_Value").GetComponent<Text>();
    }

    public void Init(string _attributeName, float value, bool _isBaseLinear = true)
    {
        Clear();
        attributeName = _attributeName;

        if (_isBaseLinear)
            valueText.text = value.ToString();
        else
            valueText.text = ((int)(value * 100f)).ToString() + "%";

        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);

    }


    public void Clear()
    {
        attributeName = null;
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.attributePopup.Show
            (attributeName + ":", valueText.text, this.gameObject, Vector3.right);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.attributePopup.Clear();
    }


}
