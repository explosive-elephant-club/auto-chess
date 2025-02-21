using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class ChampionMovementController
{
    [HideInInspector]
    public NavMeshAgent navMeshAgent;
    [HideInInspector]
    public NavMeshObstacle navMeshObstacle;
    public List<Vector3> path;
    ChampionController championController;
    public ChampionMovementController(ChampionController _championController)
    {
        championController = _championController;
        navMeshAgent = championController.GetComponent<NavMeshAgent>();
        navMeshObstacle = championController.GetComponent<NavMeshObstacle>();
        navMeshAgent.enabled = false;
    }

    public void StartMove()
    {
        if (navMeshAgent.enabled)
        {
            if (navMeshAgent.speed != championController.attributesController.moveSpeed.GetTrueValue())
            {
                navMeshAgent.speed = championController.attributesController.moveSpeed.GetTrueValue();
            }
            navMeshAgent.destination = championController.target.transform.position;
            navMeshAgent.isStopped = false;
            navMeshObstacle.enabled = false;
            path = navMeshAgent.path.corners.ToList<Vector3>();
        }
    }

    public void StopMove()
    {
        if (navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshObstacle.enabled = true;
        }
    }

    public void MoveMode()
    {
        navMeshAgent.enabled = true;
        navMeshObstacle.enabled = false;
    }

    public void StaticMode()
    {
        navMeshAgent.enabled = false;
        navMeshObstacle.enabled = true;
    }

    public void UpdateSpeed()
    {
        navMeshAgent.speed = championController.attributesController.moveSpeed.GetTrueValue();
    }

    public void UpdateRad(float rad)
    {
        navMeshAgent.radius = rad;
        navMeshObstacle.radius = rad;
    }
}
