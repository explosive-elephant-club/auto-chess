using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class MainCameraController : CameraStateBase
{
    public MainCameraController(int stateId) : base(stateId)
    {
        inputControls.GamePlay.CamZoom.started += CameraZoom;
        inputControls.GamePlay.CamZoom.canceled += CameraZoom;
    }

    Vector2 inputDir;
    float inputZoom;
    Vector3 oringinPos;

    Vector3 savePos;
    Vector3 saveOrthographicSize;
    public override void DoOnEnter()
    {
        base.DoOnEnter();

        oringinPos = CameraManager.cameraOriginPos;
        CameraManager.mainCamera.orthographic = true;
        CameraManager.targetPos = oringinPos;
        //cameraManager.mainCamera.transform.position = oringinPos;
        CameraManager.mainCamera.transform.rotation = Quaternion.Euler(CameraManager.cameraOriginRot);
    }

    public override void DoOnUpdate()
    {
        inputDir = inputControls.GamePlay.CamMove.ReadValue<Vector2>();
    }

    public override void DoOnFixedUpdate()
    {
        base.DoOnFixedUpdate();
        if (inputDir.magnitude >= 0.05f)
        {
            CameraManager.targetPos = CameraManager.mainCamera.transform.position;
            CameraManager.targetPos += Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(inputDir.x, 0, inputDir.y);
        }
        else if (Mathf.Abs(inputZoom) > 0)
        {
            CameraManager.targetZoom = CameraManager.mainCamera.orthographicSize + Mathf.Sign(inputZoom) * 4;
            //target += new Vector3(0, Mathf.Sign(inputZoom) * 4, 0);
        }
        Vector3 offset = CameraManager.targetPos - oringinPos;
        if (CameraManager.offsetXMin > offset.x || CameraManager.offsetXMax < offset.x)
        {
            CameraManager.targetPos.x = CameraManager.mainCamera.transform.position.x;
        }
        if (CameraManager.zoomMin > CameraManager.targetZoom || CameraManager.zoomMax < CameraManager.targetZoom)
        {
            CameraManager.targetZoom = CameraManager.mainCamera.orthographicSize;
        }
        if (CameraManager.offsetZMin > offset.z || CameraManager.offsetZMax < offset.z)
        {
            CameraManager.targetPos.z = CameraManager.mainCamera.transform.position.z;
        }
        CameraManager.mainCamera.transform.position = Vector3.Slerp(CameraManager.mainCamera.transform.position, CameraManager.targetPos, CameraManager.speed * Time.deltaTime);
        CameraManager.mainCamera.orthographicSize = Mathf.Lerp(CameraManager.mainCamera.orthographicSize, CameraManager.targetZoom, CameraManager.speed * Time.deltaTime);
        CameraManager.worldCanvasCamera.transform.position = CameraManager.mainCamera.transform.position;
    }
    

    private void CameraZoom(InputAction.CallbackContext context)
    {
        inputZoom = context.ReadValue<float>();
    }
}
