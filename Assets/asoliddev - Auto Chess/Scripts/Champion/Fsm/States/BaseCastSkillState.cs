using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseCastSkillState : State
{
    public override void OnEnter()
    {
        championController.championMovementController.StopMove();
        if (championController.CheckState("disarm"))
        {
            fsm.SwitchState("Idle");
        }
    }
    public override void OnUpdate()
    {
        if (!championController.skillController.isCasting())
        {
            if (championController.skillController.GetNextSkillConstructor() != null)
            {
                if (!championController.TurnToTarget())
                    championController.skillController.TryCastSkill();
            }
            else
            {
                championController.skillController.SkipEmptySkill();
            }
        }


        if (championController.CheckState("disarm") || !championController.IsTargetInAttackRange())
        {
            fsm.SwitchState("Idle");
            return;
        }
    }
    public override void OnLeave()
    {

    }
}