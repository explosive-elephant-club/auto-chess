using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseIdleState : State
{
    float combatTimer = 0;
    public override void OnEnter()
    {

    }
    public override void OnUpdate()
    {
        if (championController.isDead)
        {
            return;
        }
        if (championController.target == null)
        {
            if (!championController.CheckState("immovable"))
            {
                combatTimer += Time.deltaTime;
                if (combatTimer > 0.5f)
                {
                    combatTimer = 0;
                    championController.target = championController.FindTarget(30, FindTargetMode.Nearest);
                }
            }
        }
        else if (championController.target.isDead == true)
        {
            championController.target = null;
        }
        else
        {
            if (championController.IsTargetInAttackRange())
            {
                if (championController.IsLegalAttackIntervel())
                {
                    fsm.SwitchState("Attack");
                }

                return;
            }
            else
            {
                if (championController.FindPath())
                {
                    fsm.SwitchState("Move");
                    return;
                }
                else
                {
                    championController.target = null;
                }
            }
        }
    }
    public override void OnLeave()
    {
        combatTimer = 0;
    }
}