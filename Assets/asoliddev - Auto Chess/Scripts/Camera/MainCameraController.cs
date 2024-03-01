using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    public Camera mainCamera;
    public Camera worldCanvasCamera;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CameraHorizontalMove(Vector2 dir)
    {
        mainCamera.transform.position = mainCamera.transform.position + new Vector3(dir.x, 0, dir.y);
        worldCanvasCamera.transform.position = mainCamera.transform.position;
    }

    public void OnEnterBattleViewMode()
    {
        CameraHorizontalMove(new Vector2(0, -11f));
    }

    public void OnEnterLogisticsViewMode()
    {
        CameraHorizontalMove(new Vector2(0, 11f));
    }

}
