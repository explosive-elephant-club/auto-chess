using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseMoveState : State
{
    public override void OnEnter()
    {
        championController.MoveToTarget();
    }
    public override void OnUpdate()
    {
        if (championController.target == null || championController.CheckState("immovable"))
        {
            fsm.SwitchState("Idle");
        }
    }
    public override void OnLeave()
    {

    }
}