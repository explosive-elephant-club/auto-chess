using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class to identify player interaction on the map
/// </summary>
public class TriggerInfo : MonoBehaviour
{
    ///The type of the grid, GRIDTYPE_OWN_INVENTORY = 0 GRIDTYPE_OPONENT_INVENTORY = 1 GRIDTYPE_HEXA_MAP = 2;
    public int gridType = -1;

    ///X position on the grid
    public int gridX = -1;

    ///Z position on the grid
    public int gridZ = -1;

    public bool walkable;
    public Vector3 coor;
    public float g;
    public float h;
    public float f => g + h;
    public List<TriggerInfo> neighbors;
    public TriggerInfo connection;

    public void Init(Vector3 _coor)
    {
        coor = _coor;
        walkable = true;
        connection = null;
        gameObject.name = coor.ToString();
    }

    public int GetDistance(TriggerInfo trigger)
    {
        return (int)Mathf.Max(Mathf.Abs(coor.x - trigger.coor.x),
        Mathf.Abs(coor.y - trigger.coor.y),
        Mathf.Abs(coor.z - trigger.coor.z));
    }

    public void CalculateWeight(TriggerInfo startTrigger, TriggerInfo targetTrigger)
    {
        g = GetDistance(startTrigger);
        h = GetDistance(targetTrigger);
    }
}
