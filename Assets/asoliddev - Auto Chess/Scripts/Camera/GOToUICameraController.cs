using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GOToUICameraController : MonoBehaviour
{
    [Header("Camera Target")]
    public Transform cameraTarget;

    [Header("Init Position")]
    [SerializeField]
    private float cameraDist = 6f;

    [SerializeField]
    private Vector3 cameraTargetOffset;


    
    [Header("Camera Distance")]
    [SerializeField]
    private float cameraDisMoveSpeed = .001f;
    [SerializeField]
    private float cameraDisMax = 10f;
    [SerializeField]
    private float cameraDisMin = 3f;
    
    private Vector3 _realTargetPos;
    private InputControls _inputControls;
    private Vector2 _lastInputDir;
    // Start is called before the first frame update
    void Start()
    {
        _inputControls = new InputControls();
        _inputControls.GamePlay.CamZoom.started += UpdateDis;
        ResetCam();
    }

    private void OnEnable()
    {
        _inputControls.Enable();
    }

    private void OnDisable()
    {
        _inputControls.Disable();
    }

    private void Update()
    {
        if (cameraTarget != null)
        {
            _lastInputDir = _inputControls.GamePlay.CamMove.ReadValue<Vector2>();
        }
    }

    private void FixedUpdate()
    {
        if (cameraTarget != null && _lastInputDir.sqrMagnitude > 0.01f)
        {
            transform.RotateAround(cameraTarget.position + cameraTargetOffset, -cameraTarget.up, _lastInputDir.x);
            transform.RotateAround(cameraTarget.position + cameraTargetOffset, transform.right, _lastInputDir.y);
        }
    }
    
    public void UpdateDis(InputAction.CallbackContext context)
    {
        if (cameraTarget == null) return;
        Vector3 dir = (transform.position - _realTargetPos).normalized;
        cameraDist -= context.ReadValue<float>() * cameraDisMoveSpeed;
        cameraDist = Mathf.Clamp(cameraDist, cameraDisMin, cameraDisMax);
        transform.position = _realTargetPos + dir *  cameraDist;
        transform.LookAt(cameraTarget);
    }

    public void ResetCam(Transform _cameraTarget = null, float height = 0f)
    {
        cameraTarget = _cameraTarget;
        if (cameraTarget != null)
        {
            _realTargetPos = cameraTarget.position + cameraTargetOffset + Vector3.up * height;
            transform.position = _realTargetPos;
            transform.position += cameraTarget.forward * cameraDist;
            transform.LookAt(cameraTarget);
        }
    }
}
