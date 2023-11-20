using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Controlls player input
/// </summary>
public class InputController : CreateSingleton<InputController>
{
    public LayerMask uiLayer;
    public LayerMask triggerLayer;

    //declare rayhit
    RaycastHit hit;

    //convert mouse screen position to ray
    Ray ray;
    public List<RaycastResult> uiRaycastResults;

    public KeyCode viewModeSwitchKey;
    public KeyCode popupNailKey;

    protected override void InitSingleton()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    //to store mouse position
    private Vector3 mousePosition;

    public GridInfo gridInfo = null;
    public GridInfo previousGridInfo = null;
    public GameObject ui = null;

    /// Update is called once per frame
    void Update()
    {
        gridInfo = null;
        ui = null;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        uiRaycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);
        if (uiRaycastResults.Count > 0)
            ui = uiRaycastResults[0].gameObject;


        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if ray hits something
        if (Physics.Raycast(ray, out hit, 100f, triggerLayer, QueryTriggerInteraction.Collide))
        {
            //get trigger info of the  hited object
            gridInfo = hit.collider.gameObject.GetComponent<GridInfo>();

            //this is a trigger
            if (gridInfo != null)
            {
                if (gridInfo != previousGridInfo)
                {
                    Map.Instance.resetIndicators();
                    previousGridInfo = gridInfo;
                    gridInfo.SetColor(Map.Instance.indicatorActiveColor);
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
