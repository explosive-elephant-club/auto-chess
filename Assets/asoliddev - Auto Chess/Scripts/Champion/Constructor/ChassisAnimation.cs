using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.Animations;

public class ChassisAnimation : MonoBehaviour
{
    public Animator animator;
    private Vector3 lastFramePosition;
    float v = 0; float h = 0;

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
        animator.SetBool("IsMove", speed.magnitude > 0);

        float cosAngle = Vector3.Angle(speed, transform.forward);
        v = Mathf.Lerp(v, Mathf.Cos(cosAngle) * speed.magnitude, 2f * Time.deltaTime);
        h = Mathf.Lerp(h, Mathf.Sin(cosAngle) * speed.magnitude, 2f * Time.deltaTime);

        animator.SetFloat("VerticalSpeed", v);
        animator.SetFloat("HorizontalSpeed", h);
        //store last frame position
        lastFramePosition = this.transform.position;
    }
}
