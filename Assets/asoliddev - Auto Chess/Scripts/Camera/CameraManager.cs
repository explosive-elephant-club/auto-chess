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
    public float cameraPitchMax = 89.0f;
    public float cameraPitchMin = -89.0f;
    public Vector3 cameraOriginPos;
    public Vector3 cameraOriginRot;

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
    [SerializeField]
    public float curDis = 0;
    public float curCameraPitch;
    void Awake()
    {
        cameraOriginPos = mainCamera.transform.position;
        cameraOriginRot = mainCamera.transform.rotation.eulerAngles;

        _cameraFSM = new FSMStateMachine(gameObject);
        _cameraFSM.AddState(new GOToUICameraController(CameraStateMachineHelper.CameraGoToUIState));
        _cameraFSM.AddState(new MainCameraController(CameraStateMachineHelper.CameraNormalState));
        SetCameraController(CameraStateMachineHelper.CameraNormalState);

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

    public int GetCurCamState()
    {
        return _cameraFSM.currentStateID;
    }
}