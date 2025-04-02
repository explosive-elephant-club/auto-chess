using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GOToUICameraController : CameraStateBase
{
    public GOToUICameraController(int stateId) : base(stateId)
    {
        inputControls.GamePlay.CamZoom.started += UpdateDis;
    }

    private Vector3 _realTargetPos;
    private Vector2 _lastInputDir;
    private Transform _cameraTransform;

    public override void DoOnEnter()
    {
        base.DoOnEnter();
        _cameraTransform = CameraManager.mainCamera.transform;
        
        ResetCam(GamePlayController.Instance.pickedChampion.transform, 0);
        CameraManager.mainCamera.Reset();
        CameraManager.mainCamera.orthographic = false;
        CameraManager.mainCamera.fieldOfView = 90;

        CameraManager.mainCamera.ResetWorldToCameraMatrix();
    }

    public override void DoOnUpdate()
    {
        base.DoOnUpdate();
        if (CameraManager.cameraTarget != null)
        {
            _lastInputDir = inputControls.GamePlay.CamMove.ReadValue<Vector2>();
        }
    }

    public override void DoOnFixedUpdate()
    {
        base.DoOnFixedUpdate();
        if (CameraManager.cameraTarget != null && _lastInputDir.sqrMagnitude > 0.01f)
        {
            _cameraTransform.RotateAround(CameraManager.cameraTarget.position + CameraManager.cameraTargetOffset, Vector3.down, _lastInputDir.x);
            if (CameraManager.curCameraPitch + _lastInputDir.y > CameraManager.cameraPitchMin && CameraManager.curCameraPitch + _lastInputDir.y < CameraManager.cameraPitchMax)
                _cameraTransform.RotateAround(CameraManager.cameraTarget.position + CameraManager.cameraTargetOffset, _cameraTransform.right, _lastInputDir.y);

            CameraManager.curCameraPitch = _cameraTransform.localEulerAngles.x > 180 ? _cameraTransform.localEulerAngles.x - 360 : _cameraTransform.localEulerAngles.x;
        }
        CameraManager.curDis = (_cameraTransform.position - _realTargetPos).magnitude;
    }

    public void UpdateDis(InputAction.CallbackContext context)
    {
        if (CameraManager.cameraTarget == null) return;
        Vector3 dir = (_cameraTransform.position - _realTargetPos).normalized;
        CameraManager.cameraDist -= context.ReadValue<float>() * CameraManager.cameraDisMoveSpeed;
        CameraManager.cameraDist = Mathf.Clamp(CameraManager.cameraDist, CameraManager.cameraDisMin, CameraManager.cameraDisMax);
        _cameraTransform.position = _realTargetPos + dir * CameraManager.cameraDist;
        //_cameraTransform.LookAt(CameraManager.cameraTarget);
    }

    public void ResetCam(Transform _cameraTarget = null, float height = 0f)
    {
        CameraManager.cameraTarget = _cameraTarget;
        if (CameraManager.cameraTarget != null)
        {
            _realTargetPos = CameraManager.cameraTarget.position + CameraManager.cameraTargetOffset + Vector3.up * height;
            _cameraTransform.position = _realTargetPos;
            _cameraTransform.position += CameraManager.cameraTarget.forward * CameraManager.cameraDist;
            _cameraTransform.LookAt(CameraManager.cameraTarget.position + CameraManager.cameraTargetOffset);
        }
    }
}
