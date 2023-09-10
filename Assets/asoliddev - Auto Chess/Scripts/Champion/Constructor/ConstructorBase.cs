using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    //种类
    public ConstructorType type;
    //附加槽位
    public List<ConstructorSlot> slots;



    public virtual bool attachConstructor(ConstructorBase constructor, ConstructorSlot slot)
    {
        if (slot.adaptTypes.Contains(constructor.type))
        {
            constructor.gameObject.transform.SetParent(slot.slotTrans);
            constructor.gameObject.transform.localPosition = Vector3.zero;
            constructor.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            slot.constructorInstance = constructor;

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


}
