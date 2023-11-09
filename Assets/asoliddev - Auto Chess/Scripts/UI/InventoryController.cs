using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.Linq;



public class InventoryController : MonoBehaviour
{

    CanvasGroup canvasGroup;

    public List<ConstructorBaseData> allConstructors = new List<ConstructorBaseData>();
    public List<ConstructorBaseData> pickedConstructors = new List<ConstructorBaseData>();

    public InventorySlot pointEnterInventorySlot;
    public List<InventorySlot> inventorySlots;

    public Button pickAllBtn;
    public Button cancelAllBtn;

    [Serializable]
    public class typeToggle
    {
        public ConstructorType[] types;
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

        AddConstructor(1);
        AddConstructor(2);
        AddConstructor(3);
        AddConstructor(4);
        AddConstructor(5);

        UpdateInventory();
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
            UpdateInventory();
        });
        cancelAllBtn.onClick.AddListener(() =>
        {
            foreach (var t in typeToggles)
            {
                t.toggle.isOn = false;
            }
            UpdateInventory();
        });
        foreach (var t in typeToggles)
        {
            t.toggle.onValueChanged.AddListener((bool b) =>
            {
                UpdateInventory();
            });
        }
    }

    public void SetUIActive(bool isActive)
    {
        if (isActive)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

    }

    public void UpdateInventory()
    {
        GetPickedConstructors();
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].gameObject.SetActive(false);
            if (i < pickedConstructors.Count)
            {
                inventorySlots[i].gameObject.SetActive(true);
                inventorySlots[i].Init(pickedConstructors[i], this);
            }
        }
    }

    public void AddConstructor(ConstructorBaseData constructorBaseData)
    {
        allConstructors.Add(constructorBaseData);
    }

    public void AddConstructor(int id)
    {
        AddConstructor(GameData.Instance.constructorsArray.Find(c => c.ID == id));
    }

    void GetPickedConstructors()
    {
        List<ConstructorType> types = new List<ConstructorType>();
        foreach (var t in typeToggles)
        {
            if (t.toggle.isOn)
            {
                types = types.Concat(new List<ConstructorType>(t.types)).ToList<ConstructorType>();
            }
        }
        pickedConstructors = allConstructors.FindAll(c => IsTypeEqual(c, types));
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
