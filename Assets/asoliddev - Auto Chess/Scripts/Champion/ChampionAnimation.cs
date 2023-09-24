using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Animations;

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
            
        }

    }

    public void InitBehavour()
    {
        BaseBehaviour[] behaviours = animator.GetBehaviours<BaseBehaviour>();
        foreach (BaseBehaviour b in behaviours)
        {
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

    public AnimationClip GetAnimationState(string stateName)
    {

        //AnimationState state = animator;
        return null;
    }
}
