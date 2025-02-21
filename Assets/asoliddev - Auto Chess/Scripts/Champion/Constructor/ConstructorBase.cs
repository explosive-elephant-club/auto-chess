using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ExcelConfig;
using UnityEngine.PlayerLoop;
using System.Linq;
using System.Diagnostics;

/// <summary>
/// 部件类型
/// </summary>
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
    Weapon,
    Isolate
}


/// <summary>
/// 安装部件的槽位
/// </summary>
[Serializable]
public class ConstructorSlot
{
    public int slotDataID;
    public Transform slotTrans;
    [HideInInspector]
    public ConstructorSlotType slotType;
    /// <summary>
    /// 该槽位支持的部件类型列表
    /// </summary>
    [HideInInspector]
    public List<ConstructorType> adaptTypes = new List<ConstructorType>();



    [HideInInspector]
    public bool isAble = true;
    /// <summary>
    /// 当前槽位上的部件实例
    /// </summary>
    public ConstructorBase constructorInstance;

    public void Init()
    {
        isAble = true;
        slotType = GameExcelConfig.Instance.constructorSlotTypesArray.Find(s => s.ID == slotDataID);
        //解析配置数据中的slotType.adaptTypes，将其转换为列表
        foreach (var t in slotType.adaptTypes)
        {
            if (!string.IsNullOrEmpty(t))
            {
                adaptTypes.Add((ConstructorType)Enum.Parse(typeof(ConstructorType), t));
            }

        }
    }

    /// <summary>
    /// 子物体能否放入槽位中
    /// </summary>
    /// <param name="constructor">子物体</param>
    /// <returns>尝试结果</returns>
    public bool CanAttach(ConstructorBase constructor)
    {
        ConstructorType t = (ConstructorType)Enum.Parse(typeof(ConstructorType), constructor.constructorData.type);
        return (adaptTypes.Contains(t) && isAble);
    }

}

/// <summary>
/// 部件类
/// </summary>
public class ConstructorBase : MonoBehaviour
{
    public int constructorDataID;
    /// <summary>
    /// 组件的配置数据
    /// </summary>
    [HideInInspector]
    public ConstructorBaseData constructorData;

    /// <summary>
    /// 用于修改属性值的操作
    /// </summary>
    public ValueOperation[] valueOperations = new ValueOperation[0];
    /// <summary>
    /// 组件类型
    /// </summary>
    public ConstructorType type;
    /// <summary>
    /// 槽位
    /// </summary>
    public List<ConstructorSlot> slots;
    /// <summary>
    /// 父部件
    /// </summary>
    public ConstructorBase parentConstructor;
    /// <summary>
    /// 是否可以播放新的技能动画
    /// </summary>
    public bool enablePlayNewSkillAnim = true;
    /// <summary>
    /// 所属的单位
    /// </summary>
    public ChampionController championController;
    public Animator animator;
    /// <summary>
    /// 技能释放点位
    /// </summary>
    public Transform[] skillCastPoints;
    public UnityAction onSkillAnimEffect = new UnityAction(() => { });
    public UnityAction onSkillAnimFinish = new UnityAction(() => { });
    /// <summary>
    /// 渲染器 用于更换材质
    /// </summary>
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

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="_championController">所属单位</param>
    /// <param name="isAutoPackage">是否自动装配子部件</param>
    public void Init(ChampionController _championController, bool isAutoPackage)
    {
        //获取配置数据
        if (constructorData.ID == 0)
            constructorData = GameExcelConfig.Instance.constructorsArray.Find(c => c.ID == constructorDataID);

        championController = _championController;
        //计算价格
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

        //属性加成
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
        //增加技能给单位
        if (constructorData.skillID[0] != 0)
        {
            foreach (var id in constructorData.skillID)
            {
                championController.skillController.AddSkill(id, this);
            }
        }
        //递归组装所有子部件
        if (isAutoPackage)
            AutoPackage();
        //更新技能槽位
        championController.skillController.UpdateSkillCapacity();
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// /// <param name="_constructorData">部件数据</param>
    /// <param name="championController">所属单位</param>
    /// <param name="isAutoPackage">是否自动装配子部件</param>
    public void Init(ConstructorBaseData _constructorData, ChampionController championController, bool isAutoPackage)
    {
        constructorData = _constructorData;
        Init(championController, isAutoPackage);
    }

    /// <summary>
    /// 移除部件
    /// </summary>
    public void OnRemove()
    {
        //恢复属性
        foreach (ValueOperation operation in valueOperations)
        {
            operation.reset.Invoke();
        }
        //移除技能
        championController.skillController.RemoveSkill(this);
        UIController.Instance.championInfoController.UpdateUI();
    }

    /// <summary>
    /// 更换涂装
    /// </summary>
    public void InitPainting()
    {
    }

    /// <summary>
    /// 添加子组件
    /// </summary>
    /// <param name="constructor">子组件</param>
    /// <param name="slot">槽位</param>
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
    /// <summary>
    /// 添加子组件
    /// </summary>
    /// <param name="constructorData">部件配置数据</param>
    /// <param name="slot">槽位</param>
    public virtual void AttachConstructor(ConstructorBaseData constructorData, ConstructorSlot slot)
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Prefab/Constructor/" + constructorData.prefab));
        ConstructorBase _constructorBase = obj.GetComponent<ConstructorBase>();
        _constructorBase.Init(constructorData, championController, false);
        UIController.Instance.championInfoController.UpdateUI();
        AttachConstructor(_constructorBase, slot);
    }

    /// <summary>
    /// 移除子组件
    /// </summary>
    /// <param name="slot">槽位</param>
    /// <returns>被移除的部件和其所有子部件</returns>
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

    /// <summary>
    /// 移除所有组件
    /// </summary>
    /// <returns>被移除的所有部件</returns>
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

    /// <summary>
    /// 自动获取槽位中的物体并组装
    /// </summary>
    public virtual void AutoPackage()
    {
        //遍历所有槽位，检测是否已有子部件
        foreach (ConstructorSlot s in slots)
        {
            if (s.slotTrans.childCount > 0)
            {
                ConstructorBase constructor = s.slotTrans.GetComponentInChildren<ConstructorBase>();
                if (constructor == null)
                    return;
                constructor.Init(championController, false);
                //如果能装配，则调用递归进行组装，否则销毁无效组件
                if (s.CanAttach(constructor))
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

    /// <summary>
    /// 向上获取所有父组件
    /// </summary>
    /// <param name="isContainSelf">是否包括自身</param>
    /// <returns>获取的所有父组件</returns>
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

    /// <summary>
    /// 向下获取所有子组件
    /// </summary>
    /// <param name="isContainSelf">是否包括自身</param>
    /// <returns>获取的所有子组件</returns>
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

    /// <summary>
    /// 计算并返回该部件件的旋转基点
    /// </summary>
    /// <returns>旋转基点</returns>
    public virtual Transform GetRotateTrans()
    {
        ConstructorBase son = this;
        ConstructorBase parent = parentConstructor;
        ConstructorSlot parentSlot = null;

        do
        {
            if (son.type == ConstructorType.Chassis || son.type == ConstructorType.Isolate)
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
            //特性类型返回自身
            if (parentSlot.slotDataID == 1002 || parentSlot.slotDataID == 1003 || parentSlot.slotDataID == 1004)
            {
                return parentSlot.slotTrans;
            }
            son = parent;
            //遍历到父节点就返回null
            if (son.type == ConstructorType.Chassis || son.type == ConstructorType.Isolate)
            {
                return null;
            }
            parent = parent.parentConstructor;
        } while (true);
    }
}
