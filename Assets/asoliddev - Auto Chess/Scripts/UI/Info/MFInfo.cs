using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ExcelConfig;
using UnityEngine.EventSystems;
using System;

[Serializable]
public class TypeCountCube
{
    public GameObject[,] cubes;

    public TypeCountCube(Transform lvl)
    {
        cubes = new GameObject[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                cubes[i, j] = lvl.GetChild(i).GetChild(j).gameObject;
            }
        }
    }
}

public class MFInfo : ContainerInfo
{
    public TypeCountCube typeCountCube;

    public ConstructorBonus ConstructorBonus;
    public int curCount;

    bool isShowPopup = false;

    #region 自动绑定
    private Image _imgIcon;
    private HorizontalLayoutGroup _layoutGroupLvl;
    //自动获取组件添加字典管理
    public override void AutoBindingUI()
    {
        _imgIcon = transform.Find("IconFrame/Icon_Auto").GetComponent<Image>();
        _layoutGroupLvl = transform.Find("Lvl_Auto").GetComponent<HorizontalLayoutGroup>();
    }
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        typeCountCube = new TypeCountCube(_layoutGroupLvl.transform);
    }

    public void Init(ConstructorBonus _ConstructorBonus, int _curCount, bool _isShowPopup)
    {
        UpdateUI(_ConstructorBonus, _curCount);
        isShowPopup = _isShowPopup;
        ClearAllListener();
        onPointerEnterEvent.AddListener(OnPointerEnterEvent);
        onPointerExitEvent.AddListener(OnPointerExitEvent);
    }

    public void UpdateUI(ConstructorBonus _ConstructorBonus, int _curCount)
    {
        ConstructorBonus = _ConstructorBonus;
        curCount = _curCount;
        _imgIcon.sprite = ResourceManager.LoadResource<Sprite>(ConstructorBonus.icon);

        int cubeCount = 0;
        for (int i = 0; i < 3; i++)
        {
            typeCountCube.cubes[i, 0].transform.parent.gameObject.SetActive(false);
            if (i < ConstructorBonus.Bonus.Length && ConstructorBonus.Bonus[i].count != 0)
            {
                typeCountCube.cubes[i, 0].transform.parent.gameObject.SetActive(true);
                for (int j = 0; j < 3; j++)
                {
                    typeCountCube.cubes[i, j].SetActive(false);
                    if (j < ConstructorBonus.Bonus[i].count)
                    {
                        typeCountCube.cubes[i, j].SetActive(true);
                        cubeCount++;
                        if (cubeCount > curCount)
                        {
                            typeCountCube.cubes[i, j].transform.GetChild(0).gameObject.SetActive(false);
                        }
                        else
                        {
                            typeCountCube.cubes[i, j].transform.GetChild(0).gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    public void OnPointerEnterEvent(PointerEventData eventData)
    {
        if (isShowPopup)
            UIController.Instance.popupController.manufacturerPopup.Show
                (ConstructorBonus, curCount, this.gameObject, Vector3.right);

    }

    public void OnPointerExitEvent(PointerEventData eventData)
    {
        if (isShowPopup)
            UIController.Instance.popupController.manufacturerPopup.Clear();
    }
}
