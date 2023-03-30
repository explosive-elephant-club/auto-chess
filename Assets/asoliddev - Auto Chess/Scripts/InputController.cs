using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls player input
/// </summary>
public class InputController : CreateSingleton<InputController>
{
    public LayerMask triggerLayer;

    //declare ray starting position var
    private Vector3 rayCastStartPosition;
    protected override void InitSingleton()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        //set position of ray starting point to trigger objects
        rayCastStartPosition = new Vector3(0, 20, 0);
    }

    //to store mouse position
    private Vector3 mousePosition;


    [HideInInspector]
    public TriggerInfo triggerInfo = null;

    [HideInInspector]
    public TriggerInfo previousTriggerInfo = null;

    /// Update is called once per frame
    void Update()
    {
        triggerInfo = null;

        //declare rayhit
        RaycastHit hit;

        //convert mouse screen position to ray
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //if ray hits something
        if (Physics.Raycast(ray, out hit, 100f, triggerLayer, QueryTriggerInteraction.Collide))
        {
            //get trigger info of the  hited object
            triggerInfo = hit.collider.gameObject.GetComponent<TriggerInfo>();

            //this is a trigger
            if (triggerInfo != null)
            {
                if (triggerInfo != previousTriggerInfo)
                {
                    Map.Instance.resetIndicators();
                    previousTriggerInfo = triggerInfo;
                    //get indicator
                    GameObject indicator = Map.Instance.GetIndicatorFromTriggerInfo(triggerInfo);

                    //set indicator color to active
                    indicator.GetComponent<MeshRenderer>().material.color = Map.Instance.indicatorActiveColor;
                }

            }
            else
                Map.Instance.resetIndicators(); //reset colors
        }
        else
        {
            Map.Instance.resetIndicators();
        }

        //store mouse position
        mousePosition = Input.mousePosition;
    }
}
