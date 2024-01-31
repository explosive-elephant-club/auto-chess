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
        isMove = speed.magnitude > 0;
        animator.SetBool("IsMove", isMove);

        if (isMove)
        {
            float cosAngle = Vector3.Angle(speed, transform.forward);
            v = Mathf.Lerp(v, Mathf.Cos(cosAngle * Mathf.Deg2Rad) * speed.magnitude, 20f * Time.deltaTime);
            h = Mathf.Lerp(h, Mathf.Sin(cosAngle * Mathf.Deg2Rad), 20f * Time.deltaTime);

            Debug.Log("Angle:" + cosAngle);
            Debug.Log("v:" + v);
            Debug.Log("h:" + h);
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
    }
}
