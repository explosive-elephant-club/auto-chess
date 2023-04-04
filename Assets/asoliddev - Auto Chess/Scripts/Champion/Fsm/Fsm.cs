using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
public class Fsm
{
    public State curState;
    public State lastState;
    public Dictionary<string, State> states = new Dictionary<string, State>();

    public void SwitchState(string nextStateKey)
    {
        if (states.ContainsKey(nextStateKey))
        {
            curState.OnLeave();
            lastState = curState;
            curState = states[nextStateKey];
            curState.OnEnter();
        }
    }

    public void Init(string firstStateKey)
    {
        curState = states[firstStateKey];
        lastState = curState;
        curState.OnEnter();
    }
}
