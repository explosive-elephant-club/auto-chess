using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseMoveState : State
{
    public override void OnEnter()
    {
        championController.FindPath();
    }
    public override void OnUpdate()
    {
        if (championController.path == null || championController.target == null || championController.CheckState("immovable"))
        {
            fsm.SwitchState("Idle");
            return;
        }
        if (championController.target.isDead == true) //target champion is alive
        {
            championController.target = null;
            fsm.SwitchState("Idle");
            return;
        }
        else
        {
            var c = championController.FindTarget(championController.champion.attackRange);
            if (c != null)
            {
                championController.target = c;
                if (!championController.CheckState("disarm"))
                {
                    fsm.SwitchState("Attack");
                    return;
                }
            }
        }
        championController.MoveToTarget();
    }
    public override void OnLeave()
    {

    }
}