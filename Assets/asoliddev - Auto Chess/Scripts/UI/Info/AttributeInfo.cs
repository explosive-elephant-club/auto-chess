using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
public class AttributeInfo : ContainerInfo
{
    public override void Awake()
    {
        base.Awake();

    }

    public void Init(string name, string value)
    {
        _textName.text = name;
        _textValue.text = value;
    }

    #region 自动绑定
    private UICustomText _textName;
    private UICustomText _textValue;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _textName = transform.Find("Name_Auto").GetComponent<UICustomText>();
        _textValue = transform.Find("Value_Auto").GetComponent<UICustomText>();
    }
    #endregion

}
