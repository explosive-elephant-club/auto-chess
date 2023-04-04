using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseAttackState : State
{
    public override void OnEnter()
    {
        championController.StopMove();
        championController.championAnimation.animator.SetBool("isAttacking", true);
    }
    public override void OnUpdate()
    {
        if (!championController.championAnimation.animator.GetBool("isAttacking"))
        {
            fsm.SwitchState("Idle");
        }
    }
    public override void OnLeave()
    {

    }
}