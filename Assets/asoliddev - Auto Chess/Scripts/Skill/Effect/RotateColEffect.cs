using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateColEffect : StaticColEffect
{
    public bool isOriginPos;
    public float rotateSpeed;
    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        if (isOriginPos)
            transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
