using System;
using UnityEngine;

public class StateBase
{
    public FSMStateMachine StateMachine => _stateMachine;
    private FSMStateMachine _stateMachine;
    private int _stateID;
    public StateBase(int stateID)
    {
        _stateID = stateID;
    }
    
    public void SetStateMachine(FSMStateMachine stateMachine)
    {
        this._stateMachine = stateMachine;
    }

    public int GetId()
    {
        return _stateID;
    }

    public virtual void DoOnEnter()
    {
        
    }

    public virtual void DoOnExit()
    {
        
    }
    
    public virtual void DoOnUpdate()
    {
                
    }

    public virtual void DoOnFixedUpdate()
    {
                
    }
}
