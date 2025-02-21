using UnityEngine;
using UnityEditor;

public class BoundTest : MonoBehaviour
{
    [SerializeField] private GameObject obj;
    private void OnDrawGizmos()
    {
        if (obj == null) return;




        //获取所有Collider 包括子物体
        var mrs = obj.GetComponentsInChildren<Renderer>(true);
        Vector3 center = Vector3.zero;

        foreach (var item in mrs)
        {
            center += item.bounds.center;
        }
        center /= mrs.Length;
        Bounds bounds = new Bounds(center, Vector3.zero);
        foreach (var item in mrs)
        {
            bounds.Encapsulate(item.bounds);
        }



        Vector3 point1 = bounds.min;
        Vector3 point2 = bounds.max;


        Handles.Label(point1, "min");
        Handles.Label(point2, "max");

        float rad = Vector2.Distance(new Vector2(point1.x, point1.z), new Vector2(point2.x, point2.z));
        Debug.Log("rad " + rad);



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
}