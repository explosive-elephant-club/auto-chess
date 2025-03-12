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
        ResetCam();
        StateMachine.eventCenter.AddListener<Transform, float>(CameraStateMachineHelper.ResetGoToUICameraTarget, ResetCam);
        _cameraManager = StateMachine.gameObject.GetComponent<CameraManager>();
        _cameraTransform = _cameraManager.mainCamera.transform;
    }

    public override void DoOnExit()
    {
        base.DoOnExit();
        _inputControls.Disable();
        StateMachine.eventCenter.RemoveListener<Transform, float>(CameraStateMachineHelper.ResetGoToUICameraTarget, ResetCam);
        _cameraManager = null;
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
            _cameraTransform.RotateAround(_cameraManager.cameraTarget.position + _cameraManager.cameraTargetOffset, -_cameraManager.cameraTarget.up, _lastInputDir.x);
            _cameraTransform.RotateAround(_cameraManager.cameraTarget.position + _cameraManager.cameraTargetOffset, _cameraTransform.right, _lastInputDir.y);
        }
    }
    
    public void UpdateDis(InputAction.CallbackContext context)
    {
        if (_cameraManager.cameraTarget == null) return;
        Vector3 dir = (_cameraTransform.position - _realTargetPos).normalized;
        _cameraManager.cameraDist -= context.ReadValue<float>() * _cameraManager.cameraDisMoveSpeed;
        _cameraManager.cameraDist = Mathf.Clamp(_cameraManager.cameraDist, _cameraManager.cameraDisMin, _cameraManager.cameraDisMax);
        _cameraTransform.position = _realTargetPos + dir *  _cameraManager.cameraDist;
        _cameraTransform.LookAt(_cameraManager.cameraTarget);
    }

    public void ResetCam(Transform _cameraTarget = null, float height = 0f)
    {
        _cameraManager.cameraTarget = _cameraTarget;
        if (_cameraManager.cameraTarget != null)
        {
            _realTargetPos = _cameraManager.cameraTarget.position + _cameraManager.cameraTargetOffset + Vector3.up * height;
            _cameraTransform.position = _realTargetPos;
            _cameraTransform.position += _cameraManager.cameraTarget.forward * _cameraManager.cameraDist;
            _cameraTransform.LookAt(_cameraManager.cameraTarget);
        }
    }
}
