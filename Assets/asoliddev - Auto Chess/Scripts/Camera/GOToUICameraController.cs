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
    private float cameraDist = 1.5f;
    [SerializeField]
    private float cameraPitch = 6f;
    [SerializeField]
    private float cameraYaw = 180.0f;
    [SerializeField]
    private Vector3 cameraTargetOffset;

    [Header("Parameter")]
    [SerializeField]
    private float cameraDistHokanTime = 0.1f;
    [SerializeField]
    private float cameraAngleHokanTime = 0.1f;

    [SerializeField]
    private float cameraDistSpeed = 0.02f;
    [SerializeField]
    private float cameraDistMax = 8.0f;
    [SerializeField]
    private float cameraDistMin = 0.1f;

    [SerializeField]
    private float cameraYawSpeed = 0.3f;
    [SerializeField]
    private float cameraPitchSpeed = 0.3f;
    [SerializeField]
    private float cameraMaxAngleSpeed = 100.0f;
    [SerializeField]
    private float cameraPitchMax = 89.0f;
    [SerializeField]
    private float cameraPitchMin = -89.0f;


    [SerializeField]
    private float moveSpeed = 0.002f;

    [Header("State")]
    public float curCameraDist;
    public float curCameraPitch;
    public float curCameraYaw;
    public float setCameraDist;
    public float setCameraPitch;
    public float setCameraYaw;
    private float cameraDistVelocity;
    private float cameraPitchVelocity;
    private float cameraYawVelocity;




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

        Vector3 targetDir = transform.position - (cameraTarget.position + cameraTargetOffset);
        Vector3 forward = cameraTarget.forward + cameraTargetOffset;
        targetDir.y = 0; forward.y = 0;
        curCameraYaw = Vector3.SignedAngle(targetDir, forward, Vector3.up);
        if (Math.Abs(setCameraYaw - curCameraYaw) > 1f)
        {
            float yawAngle = (setCameraYaw - curCameraYaw) * cameraAngleHokanTime * Time.deltaTime;
            transform.RotateAround(cameraTarget.position + cameraTargetOffset, Vector3.down, yawAngle);
        }

        targetDir = transform.position - (cameraTarget.position + cameraTargetOffset);
        forward = cameraTarget.forward + cameraTargetOffset;
        targetDir.x = 0; forward.x = 0;
        curCameraPitch = Vector3.SignedAngle(targetDir, forward, Vector3.right);
        if (Math.Abs(setCameraPitch - curCameraPitch) > 1f)
        {
            float pitchAngle = (setCameraPitch - curCameraPitch) * cameraAngleHokanTime * Time.deltaTime;
            transform.RotateAround(cameraTarget.position + cameraTargetOffset, GetNormalVector(), pitchAngle);
        }
    }

    public void UpdatePitch(float value)
    {
        setCameraPitch = Mathf.Lerp(cameraPitchMin, cameraPitchMax, value);
    }
    public void UpdateYaw(float value)
    {
        setCameraYaw = Mathf.Lerp(180, -180, value);
    }

    public void UpdateOffset(Vector2 speed)
    {

    }

    public void UpdateZoom(float value)
    {

    }

    public void ResetCam(Transform _cameraTarget = null)
    {
        cameraTarget = _cameraTarget;
        if (cameraTarget != null)
        {
            transform.SetParent(cameraTarget);
            transform.localPosition = new Vector3(0, 0, cameraDist) + cameraTargetOffset;
            transform.localEulerAngles = Vector3.zero;

            transform.RotateAround(cameraTarget.position + cameraTargetOffset, Vector3.up, cameraYaw);
            transform.RotateAround(cameraTarget.position + cameraTargetOffset, GetNormalVector(), cameraPitch);
        }
    }

    Vector3 GetNormalVector()
    {
        Vector3 a = transform.position;
        Vector3 b = cameraTarget.position + cameraTargetOffset;
        Vector3 c = cameraTarget.position + cameraTargetOffset + Vector3.up;

        return Vector3.Cross(b - a, c - a);
    }
}
