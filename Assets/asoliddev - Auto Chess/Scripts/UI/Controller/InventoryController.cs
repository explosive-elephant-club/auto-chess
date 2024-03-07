using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.Linq;



public class InventoryController : BaseControllerUI
{
    public List<InventoryConstructor> pickedConstructors = new List<InventoryConstructor>();

    public InventorySlot pointEnterInventorySlot;
    public List<InventorySlot> inventorySlots;

    public Button pickAllBtn;
    public Button cancelAllBtn;

    public GameObject viewport;

    public Button closeBtn;

    public int newCount;

    [Serializable]
    public class typeToggle
    {
        public ConstructorType type;
        public Toggle toggle;
    }


    public List<typeToggle> typeToggles;


    // Start is called before the first frame update
    void Awake()
    {
        foreach (Transform child in transform.Find("Panel/InventorySlots/Viewport/Content"))
        {
            inventorySlots.Add(child.gameObject.GetComponent<InventorySlot>());
        }
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }

    void Start()
    {
        AddAllListener();
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddAllListener()
    {
        pickAllBtn.onClick.AddListener(() =>
        {
            foreach (var t in typeToggles)
            {
                t.toggle.isOn = true;
            }
            UpdateUI();
        });
        cancelAllBtn.onClick.AddListener(() =>
        {
            foreach (var t in typeToggles)
            {
                t.toggle.isOn = false;
            }
            UpdateUI();
        });
        foreach (var t in typeToggles)
        {
            t.toggle.onValueChanged.AddListener((bool b) =>
            {
                UpdateUI();
            });
        }
        closeBtn.onClick.AddListener(() =>
        {
            isExpand = false;
            UpdateUI();
        });
    }

    public void OnPointEnterSlot(InventorySlot inventorySlot)
    {
        pointEnterInventorySlot = inventorySlot;
        if (pointEnterInventorySlot.inventoryConstructor != null)
            UIController.Instance.popupController.constructorPopup.Show
                (pointEnterInventorySlot.inventoryConstructor.constructorBaseData, pointEnterInventorySlot.gameObject, Vector3.up);
    }

    public void OnPointLeaveSlot()
    {
        pointEnterInventorySlot = null;
        UIController.Instance.popupController.constructorPopup.Clear();
    }

    public override void UpdateUI()
    {
        if (isExpand)
        {
            SetUIActive(true);
        }
        else
        {
            SetUIActive(false);
        }
        GetPickedConstructors();
        UpdateNewCount();
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].gameObject.SetActive(false);
            if (i < pickedConstructors.Count)
            {
                inventorySlots[i].gameObject.SetActive(true);
                inventorySlots[i].Init(pickedConstructors[i]);
            }
        }
    }

    public void UpdateNewCount()
    {
        newCount = GameData.Instance.allInventoryConstructors.FindAll(c => c.isNew).Count;
        UIController.Instance.levelInfo.UpdateInventoryPointTip(newCount);
    }

    public void AddConstructors(List<ConstructorBaseData> constructorBaseDatas)
    {
        foreach (var data in constructorBaseDatas)
        {
            AddConstructor(data, false);
            UpdateUI();
        }
    }

    public void AddConstructor(InventoryConstructor inventoryConstructor)
    {
        GameData.Instance.allInventoryConstructors.Add(inventoryConstructor);
    }

    public void AddConstructor(ConstructorBaseData constructorBaseData, bool isNew)
    {
        GameData.Instance.allInventoryConstructors.Add(new InventoryConstructor(constructorBaseData, isNew));
        UpdateNewCount();
    }

    public void AddConstructor(int id)
    {
        AddConstructor(GameExcelConfig.Instance.constructorsArray.Find(c => c.ID == id), true);
    }

    public void RemoveConstructor(InventoryConstructor inventoryConstructor)
    {
        GameData.Instance.allInventoryConstructors.Remove(inventoryConstructor);
        UpdateUI();
    }

    void GetPickedConstructors()
    {
        List<ConstructorType> types = new List<ConstructorType>();
        foreach (var t in typeToggles)
        {
            if (t.toggle.isOn && !types.Contains(t.type))
            {
                types.Add(t.type);
            }
        }
        pickedConstructors = GameData.Instance.allInventoryConstructors.FindAll(c => IsTypeEqual(c.constructorBaseData, types));
    }

    bool IsTypeEqual(ConstructorBaseData constructorBaseData, List<ConstructorType> types)
    {
        foreach (var t in types)
        {
            if (t.ToString() == constructorBaseData.type)
                return true;
        }
        return false;
    }
}

