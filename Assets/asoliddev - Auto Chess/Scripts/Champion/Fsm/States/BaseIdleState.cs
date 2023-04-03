using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseIdleState : State
{
    float combatTimer = 0;
    public override void OnEnter()
    {
        championController.StopMove();
    }
    public override void OnUpdate()
    {
        if (!championController.CheckState("immovable"))
        {
            if (championController.target == null)
            {
                combatTimer += Time.deltaTime;
                if (combatTimer > 0.5f)
                {
                    combatTimer = 0;
                    championController.target = championController.FindTarget();
                }
            }
            else
            {
                fsm.SwitchState("Move");
            }
        }
    }
    public override void OnLeave()
    {
        combatTimer = 0;
    }
}