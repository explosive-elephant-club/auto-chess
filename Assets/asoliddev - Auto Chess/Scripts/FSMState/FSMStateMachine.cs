using System.Collections.Generic;
using UnityEngine;


public class FSMStateMachine
{
    public Dictionary<int, StateBase> _states;
    public int currentStateID;
    public GameObject gameObject;
    public Transform transform => gameObject?.transform;
    public EventCenter eventCenter;

    public FSMStateMachine(GameObject gameObject = null)
    {
        _states = new();
        this.gameObject = gameObject;
        eventCenter = new EventCenter();
    }

    public void AddState(StateBase state)
    {
        _states.Add(state.GetId(), state);
        state.SetStateMachine(this);
    }

    public void RemoveState(StateBase state)
    {
        _states.Remove(state.GetId());
    }


    public void GotoState(int stateID)
    {
        if (stateID == currentStateID) return;


        StateBase beforeState = GetState(currentStateID);
        if (beforeState != null)
        {
            beforeState.DoOnExit();
        }

        currentStateID = stateID;
        StateBase state = GetState(currentStateID);
        if (state != null)
        {
            state.DoOnEnter();
        }
        else
        {
            if (currentStateID >= 0)
            {
                Debug.LogError("StateMachine: Can't find state of " + currentStateID);
            }
        }

    }

    public void Update()
    {
        StateBase state = GetState(currentStateID);
        if (state != null)
        {
            state.DoOnUpdate();
        }
    }

    public void FixedUpdate()
    {
        StateBase state = GetState(currentStateID);
        if (state != null)
        {
            state.DoOnFixedUpdate();
        }
    }

    public StateBase GetState(int stateID)
    {
        if (stateID >= 0)
        {
            StateBase state;
            if (_states.TryGetValue(stateID, out state))
            {
                return state;
            }
        }
        return null;
    }
}
