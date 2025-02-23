using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;

public class AttributePopup : Popup
{
    public Text attributeName;
    public Text attributeValue;
    public void Show(String _attributeName, string _attributeValue, GameObject targetUI, Vector3 dir)
    {
        attributeName.text = _attributeName;
        attributeValue.text = _attributeValue;
        base.Show(targetUI, dir);
    }
}
