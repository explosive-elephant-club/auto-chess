using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class MainCameraController : MonoBehaviour
{
    public Camera mainCamera;
    public Camera worldCanvasCamera;
    Vector2 inputDir;
    float inputZoom;
    Vector3 oringinPos;
    public Vector3 targetPos;
    public float targetZoom;
    public float speed;
    public float offsetXMin;
    public float offsetXMax;
    public float zoomMin;
    public float zoomMax;
    public float offsetZMin;
    public float offsetZMax;

    private void Start()
    {
        oringinPos = mainCamera.transform.position;
        targetPos += mainCamera.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inputDir.magnitude >= 0.05f)
        {
            targetPos = mainCamera.transform.position;
            targetPos += Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(inputDir.x, 0, inputDir.y);
        }
        else if (Mathf.Abs(inputZoom) > 0)
        {
            targetZoom = mainCamera.orthographicSize + Mathf.Sign(inputZoom) * 4;
            //target += new Vector3(0, Mathf.Sign(inputZoom) * 4, 0);
        }
        Vector3 offset = targetPos - oringinPos;
        if (offsetXMin > offset.x || offsetXMax < offset.x)
        {
            targetPos.x = mainCamera.transform.position.x;
        }
        if (zoomMin > targetZoom || zoomMax < targetZoom)
        {
            targetZoom = mainCamera.orthographicSize;
        }
        if (offsetZMin > offset.z || offsetZMax < offset.z)
        {
            targetPos.z = mainCamera.transform.position.z;
        }
        mainCamera.transform.position = Vector3.Slerp(mainCamera.transform.position, targetPos, speed * Time.deltaTime);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, speed * Time.deltaTime);
        worldCanvasCamera.transform.position = mainCamera.transform.position;
    }

    public void CameraMove(InputAction.CallbackContext context)
    {
        inputDir = context.ReadValue<Vector2>();
    }

    public void CameraZoom(InputAction.CallbackContext context)
    {
        inputZoom = context.ReadValue<float>();
    }


}
