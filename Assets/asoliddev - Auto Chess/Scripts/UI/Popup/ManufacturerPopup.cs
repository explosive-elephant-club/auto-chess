using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExcelConfig;
using Game;

public class ManufacturerPopup : Popup
{
    MFInfo selfMFInfo;

    public Transform constructorContent;
    public List<GameObject> constructorInfo;

    #region 自动绑定
	private Image _imgIcon;
	private UICustomText _textNameText;
	private UICustomText _textDescriptionText;
	private HorizontalLayoutGroup _layoutGroupLvl;
	//自动获取组件添加字典管理
	public override void AutoBindingUI()
	{
		_imgIcon = transform.Find("Panel/MFSlot/IconFrame/Icon_Auto").GetComponent<Image>();
		_textNameText = transform.Find("Panel/NameText_Auto").GetComponent<UICustomText>();
		_textDescriptionText = transform.Find("Description/DescriptionText_Auto").GetComponent<UICustomText>();
		_layoutGroupLvl = transform.Find("Panel/MFSlot/Lvl_Auto").GetComponent<HorizontalLayoutGroup>();
	}
	#endregion



    void Start()
    {
        selfMFInfo = GetComponentInChildren<MFInfo>();
        foreach (Transform child in constructorContent)
        {
            constructorInfo.Add(child.gameObject);
        }
    }

    public void Show(ConstructorBonus _ConstructorBonus, int _curCount, GameObject targetUI, Vector3 dir)
    {
        _textNameText.text = _ConstructorBonus.name;
        _textDescriptionText.text = _ConstructorBonus.description;
        selfMFInfo.Init(_ConstructorBonus, _curCount, false);
        base.Show(targetUI, dir);

        UpdateConstructorInfo(_ConstructorBonus);
    }

    void UpdateConstructorInfo(ConstructorBonus _ConstructorBonus)
    {
        if (GamePlayController.Instance.pickedChampion != null)
        {
            constructorContent.gameObject.SetActive(true);
            List<ConstructorBase> constructors = GamePlayController.Instance.pickedChampion.constructors.FindAll
                (c => c.constructorData.property1 == _ConstructorBonus.name ||
                    c.constructorData.property2 == _ConstructorBonus.name ||
                        c.constructorData.property3 == _ConstructorBonus.name);
            for (int i = 0; i < constructorInfo.Count; i++)
            {
                constructorInfo[i].SetActive(false);
                if (i < constructors.Count)
                {
                    string iconPath = constructors[i].constructorData.prefab.Substring(0, constructors[i].constructorData.prefab.IndexOf(constructors[i].constructorData.type));
                    iconPath = "Prefab/Constructor/" + iconPath + constructors[i].constructorData.type + "/Icon/";
                    string namePath = constructors[i].constructorData.prefab.Substring(constructors[i].constructorData.prefab.IndexOf(constructors[i].constructorData.type) + constructors[i].constructorData.type.Length + 1);

                    Sprite _icon = Resources.Load<Sprite>(iconPath + namePath);
                    //Texture2D tex = AssetPreview.GetAssetPreview(constructors[i].gameObject);
                    constructorInfo[i].GetComponent<Image>().sprite = _icon;
                    constructorInfo[i].SetActive(true);
                }
            }
        }
        else
        {
            constructorContent.gameObject.SetActive(false);
        }
    }
}
