using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TripleLightning : RotateColEffect
{
    public LineRenderer lineRenderer;
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        lineRenderer.SetPositions(new Vector3[] { pos1.position, pos2.position, pos3.position });
    }
}
