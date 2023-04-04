using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class State : MonoBehaviour
{
    public string name;
    public ChampionController championController;
    public Fsm fsm;
    public virtual void Init()
    {
        championController = gameObject.transform.parent.GetComponent<ChampionController>();
        fsm = championController.AIActionFsm;
    }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnLeave() { }
}
