using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseIdleState : State
{
    public override void OnEnter()
    {

    }
    public override void OnUpdate()
    {
        if (!championController.buffController.buffStateContainer.GetState("immovable"))
        {
            fsm.SwitchState("Move");
        }
    }
    public override void OnLeave()
    {
        
    }
}