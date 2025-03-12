using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using General;
using UnityEngine.EventSystems;
using Game;

public class TypeInfo : ContainerInfo
{
	ConstructorMechType constructorMechType;
	#region 自动绑定
	private Image _imgIcon;
	private UICustomText _textNameText;
	//自动获取组件添加字典管理
	public override void AutoBindingUI()
	{
		_imgIcon = transform.Find("Icon_Auto").GetComponent<Image>();
		_textNameText = transform.Find("NameText_Auto").GetComponent<UICustomText>();
	}
	#endregion


	public void Init(ConstructorMechType _constructorMechType)
	{
		constructorMechType = _constructorMechType;
		_imgIcon.sprite = ResourceManager.LoadResource<Sprite>(constructorMechType.icon);
		_textNameText.text = constructorMechType.name;
	}

	public void Init(string _constructorMechTypeName)
	{
		ConstructorMechType constructorMechType = GameExcelConfig.Instance._eeDataManager.Get<ConstructorMechType>(_constructorMechTypeName);
		Init(constructorMechType);
	}

}
