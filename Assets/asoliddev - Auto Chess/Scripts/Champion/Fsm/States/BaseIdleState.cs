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
                    championController.target = championController.FindNextAvailableSkillTarget();
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
                fsm.SwitchState("CastSkill");
                return;
            }
            else
            {
                fsm.SwitchState("Move");
                return;
            }
        }
    }
    public override void OnLeave()
    {
        combatTimer = 0;
    }
}