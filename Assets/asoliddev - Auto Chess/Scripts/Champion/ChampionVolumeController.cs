using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ChampionVolumeController : MonoBehaviour
{
    [SerializeField]
    private GameObject chassisConstructor;

    [HideInInspector]
    public float rad;

    [HideInInspector]
    public ChampionController championController;

    [HideInInspector]
    public GameObject volume;
    [HideInInspector]
    public Bounds bounds;
    [HideInInspector]
    public Collider col;

    float centerOffset = 0;
    private void Awake()
    {
        volume = transform.Find("VolumeIndicator").gameObject;
        col = volume.GetComponent<Collider>();
        championController = gameObject.GetComponent<ChampionController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        chassisConstructor = championController.GetChassisConstructor().gameObject;
        bounds = new Bounds(transform.position, Vector3.zero);
        UpdateVolume();
    }
    private void FixedUpdate()
    {
        if (bounds == null) return;
        bounds.center = new Vector3(volume.transform.transform.position.x, transform.position.y + centerOffset, volume.transform.transform.position.z);

        Vector3 targetScale = new Vector3(rad * 2, volume.transform.localScale.y, rad * 2);
        volume.transform.localScale = Vector3.Lerp(volume.transform.localScale, targetScale, 0.05f);
    }

    // Update is called once per frame
    public float UpdateVolume()
    {
        var mrs = chassisConstructor.GetComponentsInChildren<Renderer>(true);

        Vector3 center = Vector3.zero;
        foreach (var item in mrs)
        {
            center += item.bounds.center;
        }

        center /= mrs.Length;
        centerOffset = center.y - transform.position.y;


        foreach (var item in mrs)
        {
            bounds.Encapsulate(item.bounds);
        }

        float _rad = Vector2.Distance(new Vector2(bounds.min.x, bounds.min.z), new Vector2(bounds.max.x, bounds.max.z));
        rad = _rad * 0.4f;

        volume.transform.transform.position = new Vector3(center.x, transform.position.y + volume.transform.localScale.y, center.z);

        championController.championMovementController.UpdateRad(rad);
        return rad;
    }
    private void OnDrawGizmos()
    {
        if (bounds == null) return;
        Vector3 point1 = bounds.min;
        Vector3 point2 = bounds.max;


        Handles.Label(point1, "min");
        Handles.Label(point2, "max");

        Vector3 point3 = new Vector3(point1.x, point1.y, point2.z);
        Vector3 point4 = new Vector3(point1.x, point2.y, point1.z);
        Vector3 point5 = new Vector3(point2.x, point1.y, point1.z);
        Vector3 point6 = new Vector3(point1.x, point2.y, point2.z);
        Vector3 point7 = new Vector3(point2.x, point1.y, point2.z);
        Vector3 point8 = new Vector3(point2.x, point2.y, point1.z);

        Handles.DrawLine(point6, point2);
        Handles.DrawLine(point2, point8);
        Handles.DrawLine(point8, point4);
        Handles.DrawLine(point4, point6);

        Handles.DrawLine(point3, point7);
        Handles.DrawLine(point7, point5);
        Handles.DrawLine(point5, point1);
        Handles.DrawLine(point1, point3);

        Handles.DrawLine(point6, point3);
        Handles.DrawLine(point2, point7);
        Handles.DrawLine(point8, point5);
        Handles.DrawLine(point4, point1);


        Handles.Label(point3, "顶点3");
        Handles.Label(point4, "顶点4");
        Handles.Label(point5, "顶点5");
        Handles.Label(point6, "顶点6");
        Handles.Label(point7, "顶点7");
        Handles.Label(point8, "顶点8");

    }

    // 检查新圆是否与已有圆相交
    public bool IsIntersect(Vector3 pos, float _rad)
    {
        pos.y = 0;
        Vector3 center = volume.transform.position;
        center.y = 0;
        return Vector3.Distance(center, pos) < (rad + _rad);
    }
}
