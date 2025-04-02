using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using ExcelConfig;
using UnityEngine.EventSystems;
using Game;
using General;

public class ChampionAttributeInfo : ContainerInfo
{
    ChampionAttribute attribute;

    public float constructorDelta = 0;
    #region 自动绑定
    private UICustomText _textIconText;
    private UICustomText _textValueText;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _textIconText = transform.Find("IconText_Auto").GetComponent<UICustomText>();
        _textValueText = transform.Find("ValueText_Auto").GetComponent<UICustomText>();
    }
    #endregion

    public override void Awake()
    {
        base.Awake();
    }

    public void Init(ChampionAttribute _attribute)
    {
        Clear();
        attribute = _attribute;
        _textIconText.text = string.Format("<quad name=Icon/Attributes/Icon_{0} />", attribute.attributeName);
        constructorDelta = 0;

        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);

    }

    public void Init(ChampionAttributesController attributesController, string attributeName)
    {
        ChampionAttribute attribute = (ChampionAttribute)GeneralMethod.GetValueByName(attributesController, attributeName);
        Init(attribute);
    }

    public void UpdateUI()
    {
        if (attribute != null)
        {
            if (constructorDelta == 0)
                UpdateValueTextOnCambat();
            else
                UpdateValueTextOnAssemble(constructorDelta);
        }

    }

    public void UpdateValueTextOnCambat()
    {
        string textColor = attribute.GetColor();
        switch (attribute.attributeFormat)
        {
            case AttributeFormat.Int:
                _textValueText.text = string.Format("<color={0}>{1:G}</color>", textColor, attribute.GetTrueValue());
                break;
            case AttributeFormat.Float2:
                _textValueText.text = string.Format("<color={0}>{1:G}</color>", textColor, attribute.GetTrueValue());
                break;
            case AttributeFormat.Percentage:
                _textValueText.text = string.Format("<color={0}>{1:P0}</color>", textColor, attribute.GetTrueValue());
                break;
        }
    }

    public void UpdateValueTextOnAssemble(float delta)
    {
        string textColor = attribute.GetColor(delta);
        switch (attribute.attributeFormat)
        {
            case AttributeFormat.Int:
                _textValueText.text = string.Format("{0:G}(<color = {1}>{2:G}</color>)", attribute.GetTrueValue(), textColor, delta);
                break;
            case AttributeFormat.Float2:
                _textValueText.text = string.Format("{0:N}(<color = {1}>{2:G}</color>)", attribute.GetTrueValue(), textColor, delta);
                break;
            case AttributeFormat.Percentage:
                _textValueText.text = string.Format("{0:P0}(<color = {1}>{2:P0}</color>)", attribute.GetTrueValue(), textColor, delta);
                break;
        }
    }

    public void Clear()
    {
        attribute = null;
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.attributePopup.Show
            (attribute, this.gameObject, Vector3.right);
    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        UIController.Instance.popupController.attributePopup.Clear();
    }


}
