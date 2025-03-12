using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;


public class CameraManager : MonoBehaviour
{
    private FSMStateMachine _cameraFSM;
    
    [Header("Camera Parameters")]
    public Camera mainCamera;
    public Camera worldCanvasCamera;
    public Vector3 targetPos;
    public float targetZoom;
    public float speed;
    public float offsetXMin;
    public float offsetXMax;
    public float zoomMin;
    public float zoomMax;
    public float offsetZMin;
    public float offsetZMax;
    public Vector3 cameraOriginPos;
    
    [Header("Camera GoToUI Parameters")]
    [Header("Camera Target")]
    public Transform cameraTarget;
    [Header("Init Position")]
    [SerializeField]
    public float cameraDist = 6f;
    [SerializeField]
    public Vector3 cameraTargetOffset;
    [Header("Camera Distance")]
    [SerializeField]
    public float cameraDisMoveSpeed = .001f;
    [SerializeField]
    public float cameraDisMax = 10f;
    [SerializeField]
    public float cameraDisMin = 3f;
    
    void Awake()
    {
        _cameraFSM = new FSMStateMachine(gameObject);
        _cameraFSM.AddState(new GOToUICameraController(CameraStateMachineHelper.CameraGoToUIState));
        _cameraFSM.AddState(new MainCameraController(CameraStateMachineHelper.CameraNormalState));
        SetCameraController(CameraStateMachineHelper.CameraNormalState);
        cameraOriginPos = mainCamera.transform.position;
    }

    void Update()
    {
        _cameraFSM.Update();
    }

    private void FixedUpdate()
    {
        _cameraFSM.FixedUpdate();
    }

    public void SetCameraController(int cameraStateId)
    {
        _cameraFSM.GotoState(cameraStateId);
    }

    public void SetGoToUICameraControllerTarget(Transform target = null, float height = 0f)
    {
        if (target == null)
        {
            SetCameraController(CameraStateMachineHelper.CameraNormalState);
            return;
        }
        SetCameraController(CameraStateMachineHelper.CameraGoToUIState);
        _cameraFSM.eventCenter.Broadcast(CameraStateMachineHelper.ResetGoToUICameraTarget, target, height);
    }
}