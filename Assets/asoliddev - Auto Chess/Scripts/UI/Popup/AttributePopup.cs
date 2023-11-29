using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;

public class AttributePopup : Popup
{
    public TextMeshProUGUI attributeName;
    public TextMeshProUGUI attributeValue;
    public void Show(String _attributeName, string _attributeValue, GameObject targetUI, Vector3 dir)
    {
        attributeName.text = _attributeName;
        attributeValue.text = _attributeValue;
        base.Show(targetUI, dir);
    }
}
