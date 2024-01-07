using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOToUICameraController : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    private Transform targetUnit;
    [SerializeField]
    private Transform targetSlot;


    [Header("Camera Target")]
    public Transform cameraTarget;
    public Vector3 cameraTargetPos;

    [Header("Init Position")]
    [SerializeField]
    private float cameraDist = 1.5f;
    [SerializeField]
    private float cameraPitch = 6f;
    [SerializeField]
    private float cameraYaw = 180.0f;
    [SerializeField]
    private Vector3 cameraStaticOffset;

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
    public float setCameraDist;
    public float setCameraPitch;
    public float setCameraYaw;
    private float cameraDistVelocity;
    private float cameraPitchVelocity;
    private float cameraYawVelocity;
    [SerializeField]
    private Vector3 cameraMoveOffset = Vector3.zero;
    [SerializeField]
    private Vector3 cameraTargetOffset;


    // Start is called before the first frame update
    void Start()
    {
        if (cameraTransform == null)
        {
            var cam = GetComponent<Camera>();
            if (cam)
                cameraTransform = cam.transform;
        }
        if (cameraTransform == null)
            enabled = false;

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
        if (cameraTransform == null)
            return;

        if (cameraTarget != null)
        {
            cameraTargetPos = cameraTarget.position;
        }
        else
        {
            return;
        }

        Vector3 staticOffset = Vector3.Lerp(Vector3.zero, cameraStaticOffset,
         (cameraDist - cameraDistMin) / (cameraDistMax - cameraDistMin));

        cameraTargetOffset = Vector3.Lerp(Vector3.zero, cameraMoveOffset,
         (cameraDistMax - cameraDist) / (cameraDistMax - cameraDistMin));


        cameraDist = Mathf.SmoothDamp(cameraDist, setCameraDist, ref cameraDistVelocity, cameraDistHokanTime);
        cameraPitch = Mathf.SmoothDampAngle(cameraPitch, setCameraPitch, ref cameraPitchVelocity, cameraAngleHokanTime);
        cameraYaw = Mathf.SmoothDampAngle(cameraYaw, setCameraYaw, ref cameraYawVelocity, cameraAngleHokanTime);


        Quaternion q = Quaternion.Euler(cameraPitch, cameraYaw + cameraTarget.rotation.eulerAngles.y, 0);
        q = transform.rotation * q; // コンポーネントの回転
        Vector3 v = new Vector3(0, 0, -cameraDist);
        Vector3 pos = q * v;


        Vector3 tarpos = cameraTargetPos + cameraTargetOffset + cameraStaticOffset;
        Vector3 fixpos = tarpos + pos;
        cameraTransform.localPosition = fixpos;

        Vector3 relativePos = tarpos - cameraTransform.position;
        Quaternion rot = Quaternion.LookRotation(relativePos);
        cameraTransform.rotation = rot;
    }

    public void UpdatePitch(float value)
    {
        setCameraPitch = Mathf.Lerp(cameraPitchMin, cameraPitchMax, value);
    }
    public void UpdateYaw(float value)
    {
        setCameraYaw = Mathf.Lerp(0, 360, value);
    }

    public void UpdateOffset(Vector2 speed)
    {
        Vector3 offset = cameraTransform.up * -speed.y * moveSpeed;
        offset += cameraTransform.right * -speed.x * moveSpeed;

        cameraTargetOffset += offset;
    }

    public void UpdateZoom(float value)
    {
        setCameraDist = Mathf.Clamp(value, cameraDistMin, cameraDistMax);
        if (cameraDist > cameraDistMin + 0.05f)
        {
            cameraMoveOffset = Vector3.zero;
        }
    }

    public void ResetCam()
    {
        setCameraDist = cameraDist;
        setCameraPitch = cameraPitch;
        setCameraYaw = cameraYaw;
        cameraMoveOffset = Vector3.zero;
    }
}
