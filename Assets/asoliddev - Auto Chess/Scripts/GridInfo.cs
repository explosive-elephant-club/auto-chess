using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class to identify player interaction on the map
/// </summary>
public class Index
{
    public Index(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public int x = 0;
    public int y = 0;
}
public class GridInfo : MonoBehaviour
{
    public GridType gridType = GridType.HexaMap;
    public bool walkable;
    public Vector3 coor;
    public Index index;
    public float g;
    public float h;
    public float f => g + h;
    public List<GridInfo> neighbors;
    public GridInfo connection;

    public Collider trigger;
    public GameObject indicator;
    Material mat;

    public ChampionController occupyChampion;
    public ChampionController bookChampion;

    public void Init(Index _index, Vector3 _coor, GridType _gridType)
    {
        gridType = _gridType;
        index = _index;
        coor = _coor;
        walkable = true;
        connection = null;
        gameObject.layer = LayerMask.NameToLayer("Triggers");
        mat = indicator.GetComponent<MeshRenderer>().material;
        gameObject.name = coor.ToString();
    }

    public void CacheNeighbors()
    {
        neighbors = new List<GridInfo>();
        foreach (var t in Map.Instance.mapGridArray)
        {
            if (GetDistance(t) == 1)
            {
                neighbors.Add(t);
            }
        }
    }

    public int GetDistance(GridInfo trigger)
    {
        return (int)Mathf.Max(Mathf.Abs(coor.x - trigger.coor.x),
        Mathf.Abs(coor.y - trigger.coor.y),
        Mathf.Abs(coor.z - trigger.coor.z));
    }

    public void CalculateWeight(GridInfo startTrigger, GridInfo targetTrigger)
    {
        g = GetDistance(startTrigger);
        h = GetDistance(targetTrigger);
    }

    public void SetColor(Color _color)
    {
        if (mat.color != _color)
            mat.color = _color;
    }

    public bool CheckInGrid(ChampionController champion)
    {
        if (Vector3.Distance(
            new Vector3(transform.position.x, transform.position.y, 0),
            new Vector3(champion.transform.position.x, champion.transform.position.y, 0)
        ) < 0.2f)
        {
            return true;
        }
        return false;
    }

    public bool IsBookedOrOccupied(ChampionController champion)
    {
        if (occupyChampion != null)
        {
            if (occupyChampion != champion)
            {
                return true;
            }
        }
        if (bookChampion != null)
        {
            if (bookChampion != champion)
            {
                return true;
            }
        }
        return false;
    }

}
