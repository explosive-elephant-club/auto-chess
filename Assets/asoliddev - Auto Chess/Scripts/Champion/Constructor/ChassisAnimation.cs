using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Animations;

public class ChassisAnimation : MonoBehaviour
{
    public Animator animator;
    private Vector3 lastFramePosition;
    private Quaternion lastFrameRotarion;
    float v = 0; float h = 0;
    bool isMove = false;

    /// Start is called before the first frame update
    void Start()
    {
        animator = transform.GetComponent<Animator>();
    }


    /// Update is called once per frame
    void Update()
    {
        Vector3 speed = (this.transform.position - lastFramePosition) / Time.deltaTime;
        speed.y = 0;
        Vector3 rot = (this.transform.rotation.eulerAngles - lastFrameRotarion.eulerAngles) / Time.deltaTime;
        isMove = speed.magnitude > 0 || rot.y > 0.03f;
        animator.SetBool("IsMove", isMove);

        if (isMove)
        {
            float cosAngle = Vector3.Angle(speed, transform.forward);
            v = Mathf.Lerp(v, Mathf.Cos(cosAngle * Mathf.Deg2Rad) * speed.magnitude, 20f * Time.deltaTime);
            h = Mathf.Lerp(h, Mathf.Sin(cosAngle * Mathf.Deg2Rad), 20f * Time.deltaTime);
        }
        else
        {
            v = 0;
            h = 0;
        }



        animator.SetFloat("VerticalSpeed", v);
        animator.SetFloat("HorizontalSpeed", h);
        //store last frame position
        lastFramePosition = this.transform.position;
        lastFrameRotarion = this.transform.rotation;
    }
}
