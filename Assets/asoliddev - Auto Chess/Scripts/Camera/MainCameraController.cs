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
    public Vector3 target;
    public float speed;
    public float offsetXMin;
    public float offsetXMax;
    public float offsetYMin;
    public float offsetYMax;
    public float offsetZMin;
    public float offsetZMax;

    private void Start()
    {
        oringinPos = mainCamera.transform.position;
        target += mainCamera.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inputDir.magnitude >= 0.05f)
        {
            target = mainCamera.transform.position;
            target += new Vector3(-inputDir.x, 0, -inputDir.y);
        }
        else if (Mathf.Abs(inputZoom) > 0)
        {
            Debug.Log("Zoom");
            target = mainCamera.transform.position;
            target += new Vector3(0, Mathf.Sign(inputZoom) * 4, 0);
        }
        Vector3 offset = target - oringinPos;
        if (offsetXMin > offset.x || offsetXMax < offset.x)
        {
            target.x = mainCamera.transform.position.x;
        }
        if (offsetYMin > offset.y || offsetYMax < offset.y)
        {
            target.y = mainCamera.transform.position.y;
        }
        if (offsetZMin > offset.z || offsetZMax < offset.z)
        {
            target.z = mainCamera.transform.position.z;
        }
        mainCamera.transform.position = Vector3.Slerp(mainCamera.transform.position, target, speed * Time.deltaTime);
        worldCanvasCamera.transform.position = mainCamera.transform.position;
    }

    public void CameraMove(InputAction.CallbackContext context)
    {
        inputDir = context.ReadValue<Vector2>();
    }

    public void CameraZoom(InputAction.CallbackContext context)
    {
        inputZoom = context.ReadValue<float>();
        Debug.Log(inputZoom);
    }


}
