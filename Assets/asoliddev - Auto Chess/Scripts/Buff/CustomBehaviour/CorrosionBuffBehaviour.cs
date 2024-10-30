using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorrosionBuffBehaviour : BuffBehaviour
{
    int maxLayer = 10;
    string superposeValueChanges = "applyDamageMultiple + 0.05";

    public override void BuffStart()
    {
        ModifyAttributeBuff modifyAttributeBuff = (ModifyAttributeBuff)buff;
        int count = buff.curLayer <= maxLayer ? buff.curLayer : maxLayer;
        for (int i = 0; i < count; i++)
        {
            modifyAttributeBuff.valueOperations.Add(new ValueOperation(superposeValueChanges, buff.owner.attributesController));
        }
    }

    public override void BuffDestroy()
    {
        buff.owner.attributesController.acidResistance.curValue = 5;
    }
}
