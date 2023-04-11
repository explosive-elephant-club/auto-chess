using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleBehaviour : BaseBehaviour
{
    float combatTimer = 0;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        combatTimer = 0;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!championController.CheckState("immovable"))
        {
            if (championController.target == null || championController.target.GetComponent<ChampionController>().isDead)
            {
                combatTimer += Time.deltaTime;
                if (combatTimer > 0.5f)
                {
                    combatTimer = 0;
                    championController.target = championController.FindTarget(30, FindTargetMode.AnyInRange);
                }
            }
            else
            {
                float distance = Vector3.Distance(championController.transform.position, championController.target.transform.position);
                if (distance < championController.champion.attackRange && !championController.CheckState("disarm"))
                {
                    animator.SetBool("isAttacking", true);
                }
                else
                {
                    championController.MoveToTarget();
                }
            }
        }
    }


    /// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
