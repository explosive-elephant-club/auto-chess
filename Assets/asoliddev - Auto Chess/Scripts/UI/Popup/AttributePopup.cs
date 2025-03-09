using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using General;
using System.Diagnostics;
using System;
using UnityEngine.PlayerLoop;
using Game;

public class AttributePopup : Popup
{
    ChampionAttribute attribute;
    #region 自动绑定
    private UICustomText _textAttributeName;
    private UICustomText _textAttributeValue;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _textAttributeName = transform.Find("AttributeName_Auto").GetComponent<UICustomText>();
        _textAttributeValue = transform.Find("AttributeValue_Auto").GetComponent<UICustomText>();
    }
    #endregion

    public void Show(ChampionAttribute _attribute, GameObject targetUI, Vector3 dir)
    {
        attribute = _attribute;
        _textAttributeName.text = attribute.attributeName;
        switch (attribute.attributeFormat)
        {
            case AttributeFormat.Int:
                _textAttributeValue.text = string.Format(":{0:G}", attribute.GetTrueValue());
                break;
            case AttributeFormat.Float2:
                _textAttributeValue.text = string.Format(":{0:G}", attribute.GetTrueValue());
                break;
            case AttributeFormat.Percentage:
                _textAttributeValue.text = string.Format(":{0:P0}", attribute.GetTrueValue());
                break;
        }
        base.Show(targetUI, dir);
    }
}
