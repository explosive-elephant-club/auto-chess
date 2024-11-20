using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using UnityEngine.PlayerLoop;
using System.Linq;
using System.Diagnostics;
public enum ConstructorType
{
    Arm,
    Backpack,
    Chassis,
    Cockpit,
    WeaponPlatform,
    Cockpit_Weapon,
    Gadget,
    Sidepod,
    Antenna,
    Shield,
    Base,
    Weapon
}


[Serializable]
public class ConstructorSlot
{
    public int slotDataID;
    public Transform slotTrans;
    [HideInInspector]
    public ConstructorSlotType slotType;
    [HideInInspector]
    public List<ConstructorType> adaptTypes = new List<ConstructorType>();



    [HideInInspector]
    public bool isAble = true;
    public ConstructorBase constructorInstance;

    public void Init()
    {
        isAble = true;
        slotType = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == slotDataID);
        foreach (var t in slotType.adaptTypes)
        {
            if (!string.IsNullOrEmpty(t))
            {
                adaptTypes.Add((ConstructorType)Enum.Parse(typeof(ConstructorType), t));
            }

        }
    }

}


public class ConstructorBase : MonoBehaviour
{
    public int constructorDataID;
    [HideInInspector]
    public ConstructorBaseData constructorData;

    //属性修改
    public ValueOperation[] valueOperations = new ValueOperation[0];
    //种类
    [HideInInspector]
    public ConstructorType type;
    //附加槽位
    public List<ConstructorSlot> slots;
    //父组件
    public ConstructorBase parentConstructor;
    //是否可以播放新的技能动画
    public bool enablePlayNewSkillAnim = true;

    public ChampionController championController;
    public Animator animator;
    public Transform[] skillCastPoints;
    public UnityAction onSkillAnimEffect = new UnityAction(() => { });
    public UnityAction onSkillAnimFinish = new UnityAction(() => { });
    public Renderer[] renderers;

    public int cost;

    private void OnEnable()
    {
        if (GetComponent<Animator>())
        {
            animator = GetComponent<Animator>();
            BaseBehaviour[] behaviours = animator.GetBehaviours<BaseBehaviour>();
            foreach (BaseBehaviour b in behaviours)
            {
                b.constructor = this;
            }
        }
    }

    public void _onSkillAnimEffect()
    {
        onSkillAnimEffect.Invoke();
    }

    public void Init(ChampionController _championController, bool isAutoPackage)
    {
        if (constructorData.ID == 0)
            constructorData = GameExcelConfig.Instance.constructorsArray.Find(c => c.ID == constructorDataID);
        championController = _championController;
        if (constructorData.level > 0)
        {
            cost = Mathf.CeilToInt
                         (GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorMechType>(constructorData.type).cost *
                         GameExcelConfig.Instance._eeDataManager.Get<ExcelConfig.ConstructorLevel>(constructorData.level).cost);
        }
        else
        {
            cost = 0;
        }


        type = (ConstructorType)Enum.Parse(typeof(ConstructorType), constructorData.type);

        foreach (var s in slots)
        {
            s.Init();
        }
        if (GamePlayController.Instance.pickedChampion == championController)
        {
            foreach (Transform tran in transform.GetComponentsInChildren<Transform>())
            {
                tran.gameObject.layer = 9;
            }
        }

        InitPainting();

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
        if (constructorData.skillID[0] != 0)
        {
            foreach (var id in constructorData.skillID)
            {
                UnityEngine.Debug.Log("AddSkill " + id);
                championController.skillController.AddSkill(id, this);
            }
        }
        if (isAutoPackage)
            AutoPackage();
        championController.skillController.UpdateSkillCapacity();
    }

    public void Init(ConstructorBaseData _constructorData, ChampionController championController, bool isAutoPackage)
    {
        constructorData = _constructorData;
        Init(championController, isAutoPackage);
    }

    public void OnRemove()
    {
        foreach (ValueOperation operation in valueOperations)
        {
            operation.reset.Invoke();
        }
        championController.skillController.RemoveSkill(this);
        UIController.Instance.championInfoController.UpdateUI();
    }

    //更换涂装
    public void InitPainting()
    {
    }

    public bool CanAttach(ConstructorBase constructor, ConstructorSlot slot)
    {
        ConstructorType t = (ConstructorType)Enum.Parse(typeof(ConstructorType), constructor.constructorData.type);
        return (slot.adaptTypes.Contains(t) && slot.isAble);
    }

    //添加子组件
    public virtual void AttachConstructor(ConstructorBase constructor, ConstructorSlot slot)
    {
        //UnityEngine.Debug.Log(name + " " + slot.isAble);
        constructor.gameObject.transform.SetParent(slot.slotTrans);
        constructor.gameObject.transform.localPosition = Vector3.zero;
        constructor.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        constructor.gameObject.transform.localScale = Vector3.one;
        slot.constructorInstance = constructor;
        constructor.parentConstructor = this;

        //禁用子物体所有槽位
        if (slot.slotType.isForbiddenAllChildrenSlots)
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
                s.isAble = true;
                foreach (int id in slot.slotType.forbiddenChildrenSlotTypes)
                {
                    if (s.slotDataID == id)
                    {
                        s.isAble = false;
                        break;
                    }
                }
            }
        }

        //禁用父物体所有槽位
        if (slot.slotType.isForbiddenAllParentsSlots)
        {
            //UnityEngine.Debug.Log(name + " isForbiddenAllSubSlots");
            foreach (ConstructorSlot s in slots)
            {
                s.isAble = false;
            }
        }
        else//禁用父物体对应类别的槽位
        {
            foreach (ConstructorSlot s in slots)
            {
                s.isAble = true;
                foreach (int id in slot.slotType.forbiddenParentsSlotTypes)
                {
                    if (s.slotDataID == id)
                    {
                        s.isAble = false;
                        break;
                    }
                }
            }
        }
        championController.skillController.UpdateSkillCapacity();
    }

    public virtual void AttachConstructor(ConstructorBaseData constructorData, ConstructorSlot slot)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Prefab/Constructor/" + constructorData.prefab));
        ConstructorBase _constructorBase = obj.GetComponent<ConstructorBase>();
        _constructorBase.Init(constructorData, championController, false);
        UIController.Instance.championInfoController.UpdateUI();
        AttachConstructor(_constructorBase, slot);
    }

    //移除子组件
    public virtual List<ConstructorBaseData> removeConstructor(ConstructorSlot slot)
    {
        List<ConstructorBaseData> data = new List<ConstructorBaseData>();
        foreach (var c in slot.constructorInstance.GetAllChildrenConstructors(true))
        {
            data.Add(c.constructorData);
            c.OnRemove();
        }
        DestroyImmediate(slot.constructorInstance.gameObject);
        slot.constructorInstance = null;
        championController.skillController.UpdateSkillCapacity();
        return data;
    }

    //移除所有组件
    public virtual List<ConstructorBaseData> removeAllConstructor()
    {
        List<ConstructorBaseData> data = new List<ConstructorBaseData>();
        foreach (var c in GetAllChildrenConstructors(true))
        {
            data.Add(c.constructorData);
            c.OnRemove();
        }
        championController.skillController.UpdateSkillCapacity();
        DestroyImmediate(this.gameObject);
        return data;
    }

    //自动获取槽位中的物体并组装
    public virtual void AutoPackage()
    {
        foreach (ConstructorSlot s in slots)
        {
            if (s.slotTrans.childCount > 0)
            {
                ConstructorBase constructor = s.slotTrans.GetComponentInChildren<ConstructorBase>();
                if (constructor == null)
                    return;
                constructor.Init(championController, false);
                if (CanAttach(constructor, s))
                {
                    AttachConstructor(constructor, s);
                    if (!championController.constructors.Contains(constructor))
                        championController.constructors.Add(constructor);
                    constructor.AutoPackage();
                }
                else
                {
                    UnityEngine.Debug.Log(constructor.name + " Destroy");
                    Destroy(constructor.gameObject);
                }

            }
        }
        return;
    }

    //向上获取所有父组件
    public virtual List<ConstructorBase> GetAllParentConstructors(bool isContainSelf)
    {
        List<ConstructorBase> constructors = new List<ConstructorBase>();
        if (isContainSelf)
            constructors.Add(this);
        ConstructorBase parent = parentConstructor;
        if (parent == null)
            return constructors;
        do
        {
            constructors.Add(parent);
            parent = parent.parentConstructor;
        } while (parent != null);
        return constructors;
    }

    //向下获取所有子组件
    public virtual List<ConstructorBase> GetAllChildrenConstructors(bool isContainSelf)
    {
        List<ConstructorBase> constructors = new List<ConstructorBase>();
        if (isContainSelf)
            constructors.Add(this);
        foreach (var s in slots)
        {
            if (s.constructorInstance != null)
            {
                constructors = constructors.Concat(s.constructorInstance.GetAllChildrenConstructors(true)).ToList<ConstructorBase>();
            }
        }
        return constructors;
    }

    //获取旋转底座
    public virtual Transform GetRotateTrans()
    {
        ConstructorBase son = this;
        ConstructorBase parent = parentConstructor;
        ConstructorSlot parentSlot = null;

        do
        {
            if (son.type == ConstructorType.Chassis)
            {
                return null;
            }
            foreach (var s in parent.slots)
            {
                if (s.constructorInstance == son)
                {
                    parentSlot = s;
                    break;
                }
            }

            if (parentSlot.slotDataID == 1002 || parentSlot.slotDataID == 1003 || parentSlot.slotDataID == 1004)
            {
                return parentSlot.slotTrans;
            }
            son = parent;
            if (son.type == ConstructorType.Chassis)
            {
                return null;
            }
            parent = parent.parentConstructor;
        } while (true);
    }
}
