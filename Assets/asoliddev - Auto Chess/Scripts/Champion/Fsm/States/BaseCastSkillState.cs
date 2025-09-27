using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseCastSkillState : State
{
    public override void OnEnter()
    {
        championController.championMovementController.StopMove();
        /*if (championController.CheckState("disarm"))
        {
            fsm.SwitchState("Idle");
        }*/
    }
    public override void OnUpdate()
    {
        // 先检测状态
        if (championController.CheckState("disarm")) return;
        /*if (championController.CheckState("disarm") || !championController.IsTargetInAttackRange())
        {
            fsm.SwitchState("Idle");
            return;
        }*/
        championController.skillController.Tick(out var needFindTarget);
        if (needFindTarget)
            fsm.SwitchState("Idle");
        /*if (!championController.skillController.isCasting())
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
        }*/
    }
    public override void OnLeave()
    {

    }
}