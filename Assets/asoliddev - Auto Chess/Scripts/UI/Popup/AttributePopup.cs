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
        string textColor = attribute.GetColor();

        float originValue = attribute.GetConstructorModifyValue();
        float trueValue = attribute.GetTrueValue();
        float modifyValue = trueValue - originValue;


        string modifyText = string.Format("(<color={0}>{1:G}</color>)", textColor, modifyValue);
        if (modifyValue == 0)
        {
            modifyText = "";
        }
        else
        {
            string positiveSign = "";
            if (modifyValue > 0)
                positiveSign = "+";
            switch (attribute.attributeFormat)
            {
                case AttributeFormat.Int:
                    modifyText = string.Format("(<color={0}>{1}{2:G}</color>)", textColor, positiveSign, modifyValue);
                    break;
                case AttributeFormat.Float2:
                    modifyText = string.Format("(<color={0}>{1}{2:G}</color>)", textColor, positiveSign, modifyValue);
                    break;
                case AttributeFormat.Percentage:
                    modifyText = string.Format("(<color={0}>{1}{2:P0}</color>)", textColor, positiveSign, modifyValue);
                    break;
            }
        }
        switch (attribute.attributeFormat)
        {
            case AttributeFormat.Int:
                _textAttributeValue.text = string.Format(":{0:G}{1}", originValue, modifyText);
                break;
            case AttributeFormat.Float2:
                _textAttributeValue.text = string.Format(":{0:G}{1}", originValue, modifyText);
                break;
            case AttributeFormat.Percentage:
                _textAttributeValue.text = string.Format(":{0:P0}{1}", originValue, modifyText);
                break;
        }
        base.Show(targetUI, dir);
    }
}
