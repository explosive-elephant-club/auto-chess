using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using ExcelConfig;
using UnityEngine.EventSystems;
using Game;

public class SkillDamageInfo : ContainerInfo
{
    #region 自动绑定
    private UICustomText _textIconText;
    private UICustomText _textTypeNameText;
    private UICustomText _textValue;

    ExcelConfig.SkillData.damageDataClass damageInfo;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _textIconText = transform.Find("IconText_Auto").GetComponent<UICustomText>();
        _textTypeNameText = transform.Find("TypeNameText_Auto").GetComponent<UICustomText>();
        _textValue = transform.Find("TypeNameText_Auto/Value_Auto").GetComponent<UICustomText>();
    }
    #endregion

    public void Init(ExcelConfig.SkillData.damageDataClass _damageInfo)
    {
        damageInfo = _damageInfo;
        _textIconText.text = string.Format("<quad name=DamageType/Icon_{0} />", _damageInfo.type);
        _textTypeNameText.text = _damageInfo.type;
        string correctionText = _damageInfo.correction == 0 ? "" : string.Format("({0})", _damageInfo.correction);
        _textValue.text = string.Format("{0}{1}", _damageInfo.dmg, correctionText);

    }
}
