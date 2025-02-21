using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表示地图容器类型 仓库区域/战斗区域
/// </summary>
public enum ContainerType { Inventory, Battle }

public class MapContainer : MonoBehaviour
{
    [HideInInspector]
    public Collider col;
    public ContainerType containerType;
    public ChampionTeam team;

    // Start is called before the first frame update
    void Awake()
    {
        col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 检查指定的pos位置是否在边界内
    /// </summary>
    /// <param name="pos">指定的位置</param>
    /// <returns></returns>
    public bool IsContainsPos(Vector3 pos)
    {
        return col.bounds.Contains(pos);
    }

    /// <summary>
    /// 从指定的起始位置开始寻找一个空位，在边界内内生成一个矩形搜索区域。
    /// </summary>
    /// <param name="rad">空位的半径</param>
    /// <param name="targetPos">搜索到的位置</param>
    /// <param name="step">精度步长</param>
    /// <returns></returns>
    public bool GetEmptyPos(float rad, out Vector3 targetPos, float step = 0.5f)
    {
        rad += 0.5f;
        for (float x = col.bounds.min.x + rad; x <= col.bounds.max.x - rad; x += step)  // 步长可以调整
        {
            for (float z = col.bounds.min.z + rad; z <= col.bounds.max.z - rad; z += step)  // 步长可以调整
            {
                if (!Physics.CheckSphere(new Vector3(x, col.bounds.min.y, z), rad, 1 << LayerMask.NameToLayer("UnitVolume")))
                {
                    targetPos = new Vector3(x, col.bounds.min.y, z);
                    return true;
                }

            }
        }
        targetPos = Vector3.zero;
        return false;
    }

    /// <summary>
    /// 从指定的起始位置开始寻找一个空位，在边界内内生成一个环形搜索区域。
    /// </summary>
    /// <param name="startPos">指定的起始位置</param>
    /// <param name="targetPos">搜索到的位置</param>
    /// <param name="rad">空位的半径</param>
    /// <param name="numPoints">搜索精度</param>
    /// <returns></returns>
    public bool GetEmptyPos(Vector3 startPos, out Vector3 targetPos, float rad, int numPoints = 10)
    {
        float increment = 2 * Mathf.PI / numPoints;

        for (float theta = 0; theta < 2 * Mathf.PI; theta += increment)
        {
            float x = startPos.x + rad * Mathf.Cos(theta);
            float z = startPos.z + rad * Mathf.Sin(theta);
            if (x > col.bounds.min.x + rad && z > col.bounds.min.z + rad &&
                    x <= col.bounds.max.x - rad && z <= col.bounds.max.z - rad)
                if (!Physics.CheckSphere(new Vector3(x, 0, z), rad, LayerMask.NameToLayer("UnitVolume")))
                {
                    targetPos = new Vector3(x, col.bounds.min.y, z);
                    return true;
                }

        }
        targetPos = Vector3.zero;
        return false;
    }

    /// <summary>
    /// 检查给定的单位是否位于容器边界内，并根据单位的volumeController判断位置是否合规。
    /// </summary>
    /// <param name="championController">单位的volumeController</param>
    /// <returns></returns>
    public bool CheckChampionInBounds(ChampionController championController)
    {
        ChampionVolumeController volumeController = championController.championVolumeController;
        Debug.Log("rad " + volumeController.rad);
        Debug.Log(col.bounds.min.x + "/" + volumeController.transform.position.x + "/" + col.bounds.max.x);
        Debug.Log(col.bounds.min.z + "/" + volumeController.transform.position.z + "/" + col.bounds.max.z);
        if (volumeController.volume.transform.position.x < (col.bounds.min.x + volumeController.rad) || volumeController.volume.transform.position.z < (col.bounds.min.z + volumeController.rad))
        {
            return false;
        }
        if (volumeController.volume.transform.position.x > (col.bounds.max.x - volumeController.rad) || volumeController.volume.transform.position.z > (col.bounds.max.z - volumeController.rad))
        {
            return false;
        }
        return true;
    }
}
