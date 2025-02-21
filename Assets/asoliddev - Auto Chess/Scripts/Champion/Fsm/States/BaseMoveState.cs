using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseMoveState : State
{
    public override void Init()
    {
        base.Init();
    }
    public override void OnEnter()
    {
        championController.championMovementController.StartMove();
    }
    public override void OnUpdate()
    {
        if (championController.target == null || championController.CheckState("immovable") || championController.isDead)
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
            CheckSkillTarget();
        }
    }
    public override void OnLeave()
    {

    }


    void CheckSkillTarget()
    {
        Debug.Log("CheckSkillTarget");
        if (championController.skillController.GetNextAvailableSkill() != null)
        {
            var c = championController.FindTarget(championController.GetNextAvailableSkillDistance(), FindTargetMode.AnyInRange);
            if (c != null)
            {
                championController.target = c;
                fsm.SwitchState("CastSkill");
                return;
            }
            Debug.Log("FindTarget null");
            return;
        }
        Debug.Log("GetNextAvailableSkill null");
    }

}