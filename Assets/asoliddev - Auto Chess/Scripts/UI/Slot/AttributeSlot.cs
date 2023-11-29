using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ExcelConfig;
using UnityEngine.EventSystems;

public class AttributeSlot : ContainerSlot
{
    TextMeshProUGUI valueText;

    ChampionAttribute baseAttribute;
    ChampionAttribute correctionAttribute;

    private void Awake()
    {
        valueText = transform.Find("Text_Value").GetComponent<TextMeshProUGUI>();
    }

    public void Init(ChampionAttribute _baseAttribute, ChampionAttribute _correctionAttribute, bool _isBaseLinear = true, bool _isCorrectionDecr = true)
    {
        Clear();

        baseAttribute = _baseAttribute;
        correctionAttribute = _correctionAttribute;

        if (_correctionAttribute == null)
        {
            if (_isBaseLinear)
                valueText.text = _baseAttribute.GetTrueLinearValue().ToString();
            else
                valueText.text = (_baseAttribute.GetTrueMultipleValue() * 100f).ToString() + "%";
        }
        else
        {
            if (_isBaseLinear)
            {
                if (_isCorrectionDecr)
                    valueText.text = (_baseAttribute.GetTrueLinearValue() * (1 - _correctionAttribute.GetTrueMultipleValue())).ToString();
                else
                    valueText.text = (_baseAttribute.GetTrueLinearValue() * (_correctionAttribute.GetTrueMultipleValue())).ToString();
            }
            else
            {
                if (_isCorrectionDecr)
                    valueText.text = ((_baseAttribute.GetTrueMultipleValue() * 100f) * (1 - _correctionAttribute.GetTrueMultipleValue())).ToString();
                else
                    valueText.text = ((_baseAttribute.GetTrueMultipleValue() * 100f) * (_correctionAttribute.GetTrueMultipleValue())).ToString();
            }
        }

        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);



    }

    public void Clear()
    {
        baseAttribute = null;
        correctionAttribute = null;
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.attributePopup.Show
            (baseAttribute.attributeName + ":", valueText.text, this.gameObject, Vector3.right);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.attributePopup.Clear();
    }


}
