using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomProjectileEffect : TrackProjectileEffect
{
    float XRotateSpeed;
    float YRotateSpeed;
    float ZRotateSpeed;
    public Vector3 offset;
    public float rotateSpeed = 5f;
    public float randomIntervel = .5f;
    float randomTime;
    public override void Init(Skill _skill, Transform _target)
    {
        skill = _skill;
        target = _target;
        curTime = 0;
        oringinTarget = target.position + new Vector3(0, 1.5f, 0);

        isMoving = true;
        isTrack = false;

        XRotateSpeed = YRotateSpeed = ZRotateSpeed = rotateSpeed * Time.fixedDeltaTime;
        float a = -1;
        if (randomTime >= randomIntervel)
        {
            randomTime = 0;
            a = Mathf.Pow(-1, Random.Range(0, 2));
            XRotateSpeed *= a;
            a = Mathf.Pow(-1, Random.Range(0, 2));
            YRotateSpeed *= a;
            a = Mathf.Pow(-1, Random.Range(0, 2));
            ZRotateSpeed *= a;
        }
        duration = trackDuration + 5;

        InstantiateEmitEffect();
        PointedAtOffset();
    }

    void Update()
    {
        float a = -1;
        randomTime += Time.deltaTime;
        if (randomTime >= randomIntervel)
        {
            randomTime = 0;
            a = Mathf.Pow(-1, Random.Range(0, 2));
            XRotateSpeed *= a;
            a = Mathf.Pow(-1, Random.Range(0, 2));
            YRotateSpeed *= a;
            a = Mathf.Pow(-1, Random.Range(0, 2));
            ZRotateSpeed *= a;
        }
    }

    protected void PointedAtOffset()
    {
        Vector3 relativePos = target.position + offset - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        this.transform.rotation = rotation;
    }

    protected override void ParabolaMoving()
    {

        transform.Rotate(new Vector3(XRotateSpeed, YRotateSpeed, ZRotateSpeed));
        transform.Translate(transform.forward * speed * Time.fixedDeltaTime, Space.World);

        if (Vector3.Distance(oringinTarget, target.position) <= 1f || curTime > trackDuration)
        {
            isTrack = true;
        }
    }
}
