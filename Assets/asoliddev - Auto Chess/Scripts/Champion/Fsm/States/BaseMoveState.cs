using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseMoveState : State
{
    public override void OnEnter()
    {
    }
    public override void OnUpdate()
    {
        if (championController.target == null || championController.CheckState("immovable"))
        {
            fsm.SwitchState("Idle");
        }
        if (championController.target.GetComponent<ChampionController>().isDead == true) //target champion is alive
        {
            championController.target = null;
            fsm.SwitchState("Idle");
        }
        else
        {
            float distance = Vector3.Distance(championController.transform.position, championController.target.transform.position);
            if (distance < championController.champion.attackRange)
            {
                if (!championController.CheckState("disarm"))
                    fsm.SwitchState("Attack");
            }
            else
            {
                championController.MoveToTarget();
            }
        }

    }
    public override void OnLeave()
    {

    }
}