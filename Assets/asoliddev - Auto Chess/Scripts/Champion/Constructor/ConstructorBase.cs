using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using UnityEngine.PlayerLoop;
using System.Linq;
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
    public ValueOperation[] valueOperations = new ValueOperation[0];
    //种类
    public ConstructorType type;
    //附加槽位
    public List<ConstructorSlot> slots;
    //父组件
    public ConstructorBase parentConstructor;
    //是否可以播放新的技能动画
    public bool enablePlayNewSkillAnim = true;

    public ChampionController championController;
    public Animator animator;
    public Transform skillCastPoint;
    public UnityAction onSkillAnimEffect = new UnityAction(() => { });
    public UnityAction onSkillAnimFinish = new UnityAction(() => { });


    private void Awake()
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
        if (constructorData.skillID[0] != 0)
        {
            foreach (var id in constructorData.skillID)
            {
                championController.skillController.AddSkill(id, this);
            }
        }
        if (isAutoPackage)
            AutoPackage();
    }

    public void Init(int id, ChampionController championController, bool isAutoPackage)
    {
        constructorDataID = id;
        Init(championController, isAutoPackage);
    }

    public void OnRemove()
    {
        foreach (ValueOperation operation in valueOperations)
        {
            operation.reset.Invoke();
        }
        championController.skillController.RemoveSkill(this);
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

    //移除子组件
    public virtual List<ConstructorBaseData> removeConstructor(ConstructorSlot slot)
    {
        List<ConstructorBaseData> data = new List<ConstructorBaseData>();
        foreach (var c in slot.constructorInstance.GetAllChildrenConstructors(true))
        {
            data.Add(c.constructorData);
            c.OnRemove();
        }
        Destroy(slot.constructorInstance);
        return data;
    }

    //自动获取槽位中的物体并组装
    public virtual void AutoPackage()
    {
        foreach (ConstructorSlot s in slots)
        {
            if (s.slotTrans.childCount > 0)
            {
                ConstructorBase constructor = s.slotTrans.GetChild(0).GetComponent<ConstructorBase>();
                if (attachConstructor(constructor, s))
                {
                    constructor.Init(championController, false);
                    if (!championController.constructors.Contains(constructor))
                        championController.constructors.Add(constructor);
                    constructor.AutoPackage();
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
                constructors.Union<ConstructorBase>(s.constructorInstance.GetAllChildrenConstructors(true));
            }
        }
        return constructors;
    }
}
