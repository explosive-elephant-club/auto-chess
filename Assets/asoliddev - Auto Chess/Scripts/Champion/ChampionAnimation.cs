using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls champion animations
/// </summary>
public class ChampionAnimation : MonoBehaviour
{

    private GameObject characterModel;
    public Animator animator;
    private ChampionController championController;

    private Vector3 lastFramePosition;
    /// Start is called before the first frame update
    void Start()
    {
        //get character model
        characterModel = this.transform.Find("character").gameObject;

        //get animator
        animator = characterModel.GetComponent<Animator>();
        championController = this.transform.GetComponent<ChampionController>();

        BaseBehaviour[] behaviours = animator.GetBehaviours<BaseBehaviour>();
        foreach (BaseBehaviour b in behaviours)
        {
            b.championController = championController;
        }
    }

    public void InitBehavour()
    {
        BaseBehaviour[] behaviours = animator.GetBehaviours<BaseBehaviour>();
        foreach (BaseBehaviour b in behaviours)
        {
            b.championController = championController;
        }
    }

    /// Update is called once per frame
    void Update()
    {
        //calculate speed
        float movementSpeed = (this.transform.position - lastFramePosition).magnitude / Time.deltaTime;

        //set movement speed on animator controller
        animator.SetFloat("movementSpeed", movementSpeed);

        //store last frame position
        lastFramePosition = this.transform.position;
    }

    /// <summary>
    /// sets animation state
    /// </summary>
    /// <returns></returns>
    public void IsAnimated(bool b)
    {
        animator.enabled = b;
    }
}
