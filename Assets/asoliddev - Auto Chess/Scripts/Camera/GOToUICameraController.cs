using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GOToUICameraController : StateBase
{
    public GOToUICameraController(int stateId) : base(stateId)
    {
        _inputControls = new();
        _inputControls.GamePlay.CamZoom.started += UpdateDis;
    }

    private CameraManager _cameraManager;
    private InputControls _inputControls;
    private Vector3 _realTargetPos;
    private Vector2 _lastInputDir;
    private Transform _cameraTransform;

    public override void DoOnEnter()
    {
        base.DoOnEnter();
        _inputControls.Enable();
        if (_cameraManager == null)
            _cameraManager = StateMachine.gameObject.GetComponent<CameraManager>();
        _cameraTransform = _cameraManager.mainCamera.transform;

        ResetCam(GamePlayController.Instance.pickedChampion.transform, 0);
        _cameraManager.mainCamera.Reset();
        _cameraManager.mainCamera.orthographic = false;
        _cameraManager.mainCamera.fieldOfView = 90;

        _cameraManager.mainCamera.ResetWorldToCameraMatrix();
    }

    public override void DoOnExit()
    {
        base.DoOnExit();
        _inputControls.Disable();
    }

    public override void DoOnUpdate()
    {
        base.DoOnUpdate();
        if (_cameraManager.cameraTarget != null)
        {
            _lastInputDir = _inputControls.GamePlay.CamMove.ReadValue<Vector2>();
        }
    }

    public override void DoOnFixedUpdate()
    {
        base.DoOnFixedUpdate();
        if (_cameraManager.cameraTarget != null && _lastInputDir.sqrMagnitude > 0.01f)
        {
            _cameraTransform.RotateAround(_cameraManager.cameraTarget.position + _cameraManager.cameraTargetOffset, Vector3.down, _lastInputDir.x);
            if (_cameraManager.curCameraPitch + _lastInputDir.y > _cameraManager.cameraPitchMin && _cameraManager.curCameraPitch + _lastInputDir.y < _cameraManager.cameraPitchMax)
                _cameraTransform.RotateAround(_cameraManager.cameraTarget.position + _cameraManager.cameraTargetOffset, _cameraTransform.right, _lastInputDir.y);

            _cameraManager.curCameraPitch = _cameraTransform.localEulerAngles.x > 180 ? _cameraTransform.localEulerAngles.x - 360 : _cameraTransform.localEulerAngles.x;
        }
        _cameraManager.curDis = (_cameraTransform.position - _realTargetPos).magnitude;
    }

    public void UpdateDis(InputAction.CallbackContext context)
    {
        if (_cameraManager.cameraTarget == null) return;
        Vector3 dir = (_cameraTransform.position - _realTargetPos).normalized;
        _cameraManager.cameraDist -= context.ReadValue<float>() * _cameraManager.cameraDisMoveSpeed;
        _cameraManager.cameraDist = Mathf.Clamp(_cameraManager.cameraDist, _cameraManager.cameraDisMin, _cameraManager.cameraDisMax);
        _cameraTransform.position = _realTargetPos + dir * _cameraManager.cameraDist;
        //_cameraTransform.LookAt(_cameraManager.cameraTarget);
    }

    public void ResetCam(Transform _cameraTarget = null, float height = 0f)
    {
        _cameraManager.cameraTarget = _cameraTarget;
        if (_cameraManager.cameraTarget != null)
        {
            _realTargetPos = _cameraManager.cameraTarget.position + _cameraManager.cameraTargetOffset + Vector3.up * height;
            _cameraTransform.position = _realTargetPos;
            _cameraTransform.position += _cameraManager.cameraTarget.forward * _cameraManager.cameraDist;
            _cameraTransform.LookAt(_cameraManager.cameraTarget.position + _cameraManager.cameraTargetOffset);
        }
    }
}
