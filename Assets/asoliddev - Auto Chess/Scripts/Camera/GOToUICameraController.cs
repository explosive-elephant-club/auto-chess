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

    [SerializeField]
    private float cameraDisMoveSpeed = .1f;

    // Start is called before the first frame update
    void Start()
    {
        ResetCam();
    }

    public void UpdateRotation(InputAction.CallbackContext context)
    {
        if (cameraTarget == null) return;
        var inputDir = context.ReadValue<Vector2>();
        transform.RotateAround(cameraTarget.position + cameraTargetOffset, -cameraTarget.up, inputDir.x);
        transform.RotateAround(cameraTarget.position + cameraTargetOffset, transform.right, inputDir.y);
    }
    
    public void UpdateDis(InputAction.CallbackContext context)
    {
        if (cameraTarget == null) return;
        Vector3 dir = (cameraTarget.position + cameraTargetOffset - transform.position).normalized;
        transform.position += dir * context.ReadValue<float>() * cameraDisMoveSpeed;
    }

    public void ResetCam(Transform _cameraTarget = null, float height = 0f)
    {
        //cameraTarget = _cameraTarget;
        if (cameraTarget != null)
        {
            transform.position = cameraTarget.position + cameraTargetOffset;
            transform.position += Vector3.up * height;
            transform.position += cameraTarget.forward * cameraDist;
            transform.LookAt(cameraTarget);
        }
    }
}
