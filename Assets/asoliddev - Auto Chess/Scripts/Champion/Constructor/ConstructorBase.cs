using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using UnityEngine.PlayerLoop;
public enum ConstructorType
{
    Arm,
    Backpack,
    Base,
    Cockpit,
    Cockpit_Gun,
    CockpitWeapon,
    Gadget_Attach,
    Gadget_Replace,
    HalfShoulder,
    Prop,
    Shield,
    SubBase,
    TopPlatform,
    Weapon
}


[Serializable]
public class ConstructorSlot
{
    [HideInInspector]
    public bool isAble;
    public Transform slotTrans;
    public List<ConstructorType> adaptTypes;
    public ConstructorBase constructorInstance;

    //禁用子物体槽位类别
    public List<ConstructorType> forbiddenSubSlotTypes;
    //是否禁用子物体所有槽位
    public bool isForbiddenAllSubSlots;

}


public class ConstructorBase : MonoBehaviour
{
    public int constructorDataID;
    [HideInInspector]
    public ConstructorBaseData constructorData;
    //属性修改
    public ValueOperation[] valueOperations;
    //种类
    public ConstructorType type;
    //附加槽位
    public List<ConstructorSlot> slots;
    //父组件
    public ConstructorBase parentConstructor;

    public ChampionController championController;
    public Animator animator;
    public Transform skillCastPoint;
    public UnityAction onSkillAnimEffect;
    public UnityAction onSkillAnimFinish;


    private void Awake()
    {
        if (GetComponent<Animator>())
        {
            animator = GetComponent<Animator>();
        }
        onSkillAnimFinish = new UnityAction(() =>
        {
            Debug.Log("Fire");
        });
    }
    public void Init(ChampionController _championController, bool isAutoPackage)
    {
        constructorData = GameData.Instance.constructorsArray.Find(c => c.ID == constructorDataID);
        championController = _championController;

        if (!string.IsNullOrEmpty(constructorData.valueChanges[0]))
        {
            valueOperations = new ValueOperation[constructorData.valueChanges.Length];
            for (int i = 0; i < valueOperations.Length; i++)
            {
                valueOperations[i] = new ValueOperation(constructorData.valueChanges[i],
                championController.attributesController);
            }
        }
        foreach (ValueOperation operation in valueOperations)
        {

            operation.operate.Invoke();
        }
        if (isAutoPackage)
            AutoPackage();
    }

    public void Init(int id, ChampionController championController, bool isAutoPackage)
    {
        constructorDataID = id;
        Init(championController, isAutoPackage);
    }

    //添加子组件
    public virtual bool attachConstructor(ConstructorBase constructor, ConstructorSlot slot)
    {
        if (slot.adaptTypes.Contains(constructor.type))
        {
            constructor.gameObject.transform.SetParent(slot.slotTrans);
            constructor.gameObject.transform.localPosition = Vector3.zero;
            constructor.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            slot.constructorInstance = constructor;
            constructor.parentConstructor = this;

            //禁用子物体所有槽位
            if (slot.isForbiddenAllSubSlots)
            {
                foreach (ConstructorSlot s in constructor.slots)
                {
                    s.isAble = false;
                }
            }
            else//禁用子物体对应类别的槽位
            {
                foreach (ConstructorSlot s in constructor.slots)
                {
                    foreach (ConstructorType t in slot.forbiddenSubSlotTypes)
                    {
                        if (s.adaptTypes.Contains(t))
                        {
                            s.isAble = false;
                        }
                        else
                        {
                            s.isAble = true;
                        }
                    }
                }
            }

            return true;
        }
        return false;
    }

    //自动获取槽位中的物体并组装
    public virtual void AutoPackage()
    {
        Debug.Log(gameObject);
        foreach (ConstructorSlot s in slots)
        {
            if (s.slotTrans.childCount > 0)
            {
                ConstructorBase constructor = s.slotTrans.GetChild(0).GetComponent<ConstructorBase>();
                if (attachConstructor(constructor, s))
                {
                    constructor.Init(championController, false);
                    if (championController.constructors.Contains(constructor))
                        championController.constructors.Add(constructor);
                }
                else
                    Destroy(constructor.gameObject);
            }
        }
    }

    //向上获取所有父组件
    public virtual List<ConstructorBase> GetAllParentConstructors(bool isContainSelf)
    {
        List<ConstructorBase> constructors = new List<ConstructorBase>();
        if (isContainSelf)
            constructors.Add(this);
        ConstructorBase parent = parentConstructor;
        do
        {
            constructors.Add(parent);
            parent = parent.parentConstructor;
        } while (parent != null);
        return constructors;
    }
}
