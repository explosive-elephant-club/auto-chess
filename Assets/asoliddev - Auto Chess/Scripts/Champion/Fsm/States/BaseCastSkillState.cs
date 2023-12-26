using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseCastSkillState : State
{
    public override void OnEnter()
    {
        championController.StopMove();
        if (championController.CheckState("disarm"))
        {
            fsm.SwitchState("Idle");
        }
        StartCoroutine(championController.TurnToTarget());
    }
    public override void OnUpdate()
    {
        championController.skillController.TryCastSkill();
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