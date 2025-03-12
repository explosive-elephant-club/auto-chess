using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class MainCameraController : StateBase
{
    public MainCameraController(int stateId) : base(stateId)
    {
        
    }
    
    Vector2 inputDir;
    float inputZoom;
    Vector3 oringinPos;
    private CameraManager _cameraManager;
    private InputControls _inputControls = new();
    
    public override void DoOnEnter()
    {
        base.DoOnEnter();
        _inputControls.GamePlay.CamZoom.started += CameraZoom;
        _inputControls.GamePlay.CamZoom.canceled += CameraZoom;
        _inputControls.GamePlay.CamMove.started += CameraMove;
        _inputControls.GamePlay.CamMove.canceled += CameraMove;
        _inputControls.Enable();
        _cameraManager = StateMachine.gameObject.GetComponent<CameraManager>();
        oringinPos = _cameraManager.cameraOriginPos;
        
        _cameraManager.mainCamera.transform.position = oringinPos;
        _cameraManager.targetPos += _cameraManager.mainCamera.transform.position;
    }

    public override void DoOnFixedUpdate()
    {
        base.DoOnFixedUpdate();
        if (inputDir.magnitude >= 0.05f)
        {
            _cameraManager.targetPos = _cameraManager.mainCamera.transform.position;
            _cameraManager.targetPos += Quaternion.AngleAxis(-45, Vector3.up) * new Vector3(inputDir.x, 0, inputDir.y);
        }
        else if (Mathf.Abs(inputZoom) > 0)
        {
            _cameraManager.targetZoom = _cameraManager.mainCamera.orthographicSize + Mathf.Sign(inputZoom) * 4;
            //target += new Vector3(0, Mathf.Sign(inputZoom) * 4, 0);
        }
        Vector3 offset = _cameraManager.targetPos - oringinPos;
        if (_cameraManager.offsetXMin > offset.x || _cameraManager.offsetXMax < offset.x)
        {
            _cameraManager.targetPos.x = _cameraManager.mainCamera.transform.position.x;
        }
        if (_cameraManager.zoomMin > _cameraManager.targetZoom || _cameraManager.zoomMax < _cameraManager.targetZoom)
        {
            _cameraManager.targetZoom = _cameraManager.mainCamera.orthographicSize;
        }
        if (_cameraManager.offsetZMin > offset.z || _cameraManager.offsetZMax < offset.z)
        {
            _cameraManager.targetPos.z = _cameraManager.mainCamera.transform.position.z;
        }
        _cameraManager.mainCamera.transform.position = Vector3.Slerp(_cameraManager.mainCamera.transform.position, _cameraManager.targetPos, _cameraManager.speed * Time.deltaTime);
        _cameraManager.mainCamera.orthographicSize = Mathf.Lerp(_cameraManager.mainCamera.orthographicSize, _cameraManager.targetZoom, _cameraManager.speed * Time.deltaTime);
        _cameraManager.worldCanvasCamera.transform.position = _cameraManager.mainCamera.transform.position;
    }

    private void CameraMove(InputAction.CallbackContext context)
    {
        inputDir = context.ReadValue<Vector2>();
    }

    private void CameraZoom(InputAction.CallbackContext context)
    {
        inputZoom = context.ReadValue<float>();
    }
}
