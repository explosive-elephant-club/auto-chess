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
        //championController.FindPath();
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
            if (championController.path == null)
            {
                fsm.SwitchState("Idle");
                return;
            }
            else
            {
                MoveToTarget();
            }

        }
    }
    public override void OnLeave()
    {

    }

    public void MoveToTarget()
    {
        if (championController.bookGridInfo == null)
        {
            championController.FindPath();
        }
        if (championController.bookGridInfo.CheckInGrid(championController))
        {
            //Debug.Log("InGrid:" + bookGridInfo.name);
            championController.EnterGrid(championController.bookGridInfo);
            OnEnterGrid(championController.bookGridInfo);
            if (championController.path == null)
                return;
            if (championController.bookGridInfo == championController.target.occupyGridInfo)
            {
                championController.StopMove();
                championController.SetWorldPosition();
                OnGetTarget(championController.bookGridInfo);
            }
        }
        else
        {
            championController.NavMeshAgentMove();
        }
    }

    void OnEnterGrid(GridInfo grid)
    {
        var c = championController.FindTarget(championController.GetInAttackRange(), FindTargetMode.AnyInRange);
        if (c != null)
        {
            championController.target = c;
            Debug.Log("Move CastSkill");
            fsm.SwitchState("CastSkill");
            return;
        }
        if (!championController.FindPath())
        {
            fsm.SwitchState("Idle");
            return;
        }
    }

    void OnMoveFailed(GridInfo grid)
    {
        fsm.SwitchState("Idle");
        return;
    }

    void OnGetTarget(GridInfo grid)
    {
        fsm.SwitchState("Idle");
        return;
    }
}