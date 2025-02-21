using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOToUICameraController : MonoBehaviour
{
    [Header("Camera Target")]
    public Transform cameraTarget;

    [Header("Init Position")]
    [SerializeField]
    private float cameraDist = 6f;
    [SerializeField]
    private float cameraPitch = 6f;
    [SerializeField]
    private float cameraYaw = 180.0f;
    [SerializeField]
    private Vector3 cameraTargetOffset;

    [Header("Parameter")]
    [SerializeField]
    private float cameraPitchMax = 89.0f;
    [SerializeField]
    private float cameraPitchMin = -89.0f;


    [Header("State")]
    public float curCameraDist;
    public float curCameraPitch;
    public float curCameraYaw;
    public float setCameraDist;
    public float setCameraPitch;
    public float setCameraYaw;




    // Start is called before the first frame update
    void Start()
    {
        ResetCam();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        updateCamera();
    }


    private void updateCamera()
    {
        if (cameraTarget == null)
        {
            return;
        }
        transform.LookAt(cameraTarget.position + cameraTargetOffset);

        curCameraDist = Vector3.Distance((cameraTarget.position + cameraTargetOffset), transform.position);
        Vector3 targetDir = transform.position - (cameraTarget.position + cameraTargetOffset);
        Vector3 forward = cameraTarget.forward + cameraTargetOffset;


        targetDir.y = 0; forward.y = 0;
        curCameraYaw = Vector3.SignedAngle(targetDir, forward, Vector3.up);
        curCameraPitch = transform.localEulerAngles.x > 180 ? transform.localEulerAngles.x - 360 : transform.localEulerAngles.x;
    }

    public void UpdatePitch(float value)
    {
        setCameraPitch = Mathf.Lerp(cameraPitchMin, cameraPitchMax, value);
        float pitchAngle = (setCameraPitch - curCameraPitch);

        transform.RotateAround(cameraTarget.position + cameraTargetOffset, transform.right, pitchAngle);

    }
    public void UpdateYaw(float value)
    {
        setCameraYaw = Mathf.Lerp(180, -180, value);
        float yawAngle = (setCameraYaw - curCameraYaw);
        transform.RotateAround(cameraTarget.position + cameraTargetOffset, Vector3.down, yawAngle);
    }

    public void UpdateOffset(Vector2 speed)
    {

    }

    public void UpdateZoom(float value)
    {
        setCameraDist = value;
        Vector3 dir = (transform.localPosition - cameraTargetOffset).normalized;
        transform.localPosition = dir * value + cameraTargetOffset;
    }

    public void ResetCam(Transform _cameraTarget = null)
    {
        cameraTarget = _cameraTarget;
        transform.SetParent(cameraTarget);
        if (cameraTarget != null)
        {
            transform.localPosition = new Vector3(0, 0, cameraDist) + cameraTargetOffset;
            transform.localEulerAngles = Vector3.zero;

            transform.RotateAround(cameraTarget.position + cameraTargetOffset, Vector3.up, cameraYaw);
            transform.RotateAround(cameraTarget.position + cameraTargetOffset, transform.right, cameraPitch);


            curCameraDist = cameraDist;
            curCameraPitch = cameraPitch;
            curCameraYaw = cameraYaw;
        }
    }

}
