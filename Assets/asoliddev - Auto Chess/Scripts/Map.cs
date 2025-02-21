using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// 用于管理游戏地图中不同类型的地块
/// </summary>

public class Map : CreateSingleton<Map>
{

    /// <summary>
    /// 用于射线检测的平面
    /// </summary>
    public Plane m_Plane;
    /// <summary>
    /// 玩家的仓库区域
    /// </summary>
    public MapContainer ownInventoryContainer;
    /// <summary>
    /// 玩家的战斗区域
    /// </summary>
    public MapContainer ownBattleContainer;
    /// <summary>
    /// 敌人的仓库区域
    /// </summary>
    public MapContainer oponentInventoryContainer;
    /// <summary>
    /// 敌人的战斗区域
    /// </summary>
    public MapContainer oponentBattleContainer;


    public Color indicatorDefaultColor;
    public Color indicatorActiveColor;
    public Color indicatorDisactiveColor;

    protected override void InitSingleton()
    {

    }


    private void Start()
    {
        m_Plane = new Plane(Vector3.up, new Vector3(0, ownBattleContainer.col.bounds.min.y, 0));
        Invoke("Ready", 1f);
        //GamePlayController.Instance.OnMapReady();

    }

    void Ready()
    {
        GamePlayController.Instance.OnMapReady();
    }
}
