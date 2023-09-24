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
    }
    public override void OnUpdate()
    {
        championController.skillController.TryCastAttackSkill();
        championController.skillController.TryCastOtherSkill();
        if (championController.CheckState("disarm") || championController.skillController.EnableCastNewSkill())
        {
            fsm.SwitchState("Idle");
            return;
        }
    }
    public override void OnLeave()
    {

    }
}