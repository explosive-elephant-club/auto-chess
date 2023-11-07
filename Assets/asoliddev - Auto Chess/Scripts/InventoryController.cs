using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using UnityEditor;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public List<ConstructorBaseData> allConstructors = new List<ConstructorBaseData>();
    public List<ConstructorBaseData> pickedConstructors = new List<ConstructorBaseData>();

    public InventorySlot pointEnterInventorySlot;
    public List<InventorySlot> inventorySlots;


    // Start is called before the first frame update
    void Awake()
    {
        foreach (Transform child in transform.Find("Panel/InventorySlots/Viewport/Content"))
        {
            inventorySlots.Add(child.gameObject.GetComponent<InventorySlot>());
        }
    }

    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init()
    {
        AddConstructor(1);
        AddConstructor(2);
        AddConstructor(3);
        AddConstructor(4);
        AddConstructor(5);
        pickedConstructors = allConstructors;
        UpdateInventory();
    }

    public void UpdateInventory()
    {
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
}
