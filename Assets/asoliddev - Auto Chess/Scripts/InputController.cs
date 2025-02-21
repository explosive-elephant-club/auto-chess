using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controlls player input
/// </summary>
public class InputController : CreateSingleton<InputController>
{
    public LayerMask uiLayer;
    public LayerMask triggerLayer;
    public LayerMask unitLayer;

    //declare rayhit
    RaycastHit hit;

    //convert mouse screen position to ray
    Ray ray;
    public List<RaycastResult> uiRaycastResults;

    protected override void InitSingleton()
    {
        m_Plane = new Plane(Vector3.up, Vector3.zero);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    //to store mouse position
    public Vector3 mousePosition;
    public Plane m_Plane;
    public MapContainer mapContainer;
    public ChampionController champion;
    public ChampionController previousChampion;
    public GameObject ui = null;

    /// Update is called once per frame
    void Update()
    {
        mapContainer = null;
        champion = null;
        ui = null;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();
        uiRaycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);
        if (uiRaycastResults.Count > 0)
            ui = uiRaycastResults[0].gameObject;


        ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        float enter = 100.0f;
        if (Map.Instance.m_Plane.Raycast(ray, out enter))
        {
            mousePosition = ray.GetPoint(enter);
        }


        if (Physics.Raycast(ray, out hit, 300f, triggerLayer, QueryTriggerInteraction.Collide))
        {
            mapContainer = hit.collider.gameObject.GetComponent<MapContainer>();
        }

        if (Physics.Raycast(ray, out hit, 300f, unitLayer, QueryTriggerInteraction.Collide))
        {
            champion = hit.collider.gameObject.GetComponentInParent<ChampionController>();
            if (champion != null)
                if (champion != previousChampion)
                {
                    previousChampion = champion;
                }
        }

    }

    public bool CheckChampionInRange(ChampionController pickedChampion)
    {
        Vector3 pos = mousePosition;
        float rad = pickedChampion.championVolumeController.rad;
        Debug.Log("CheckChampionInRange pos" + pos);
        Debug.Log("CheckChampionInRange rad" + rad);
        var list = Physics.OverlapSphere(pos, rad, 1 << LayerMask.NameToLayer("UnitVolume"));
        foreach (var c in list)
        {
            Debug.Log(c.gameObject);
            if (pickedChampion.championVolumeController.col != c)
            {
                return true;
            }
        }
        return false;
    }
}
