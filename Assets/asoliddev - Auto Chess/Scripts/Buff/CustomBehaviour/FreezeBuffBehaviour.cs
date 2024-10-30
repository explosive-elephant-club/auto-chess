using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeBuffBehaviour : BuffBehaviour
{
    int maxLayer = 10;
    string superposeValueChanges1 = "moveSpeed * -0.03";
    string superposeValueChanges2 = "castDelay * 0.03";
    public override void BuffStart()
    {
        ModifyAttributeBuff modifyAttributeBuff = (ModifyAttributeBuff)buff;
        int count = buff.curLayer <= maxLayer ? buff.curLayer : maxLayer;
        for (int i = 0; i < count; i++)
        {
            modifyAttributeBuff.valueOperations.Add(new ValueOperation(superposeValueChanges1, buff.owner.attributesController));
            modifyAttributeBuff.valueOperations.Add(new ValueOperation(superposeValueChanges2, buff.owner.attributesController));
        }
    }

    public override void BuffDestroy()
    {
        buff.owner.attributesController.iceResistance.curValue = 5;
    }
}
