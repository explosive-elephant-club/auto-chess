using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffect : SkillEffect
{
    public GameObject lineRendererPrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;

    public GameObject lineRendererInstance;
    public GameObject startInstance;
    public GameObject endInstance;

    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    protected Transform target;
    public override void Init(Skill _skill)
    {
        base.Init(_skill);
        target = skill.targets[0].transform;
        InstantiateProjectileEffect();
    }

    protected void InstantiateProjectileEffect()
    {
        lineRendererInstance = Instantiate(lineRendererPrefab, transform.position, transform.rotation) as GameObject;
        lineRendererInstance.transform.parent = transform;
        startInstance = Instantiate(startPrefab, transform.position, transform.rotation) as GameObject;
        startInstance.transform.parent = transform;
        endInstance = Instantiate(endPrefab, target.position, Quaternion.FromToRotation(Vector3.up, Vector3.zero)) as GameObject;
        endInstance.transform.parent = transform;
    }

    void ShootLineInDir()
    {
        LineRenderer line = lineRendererInstance.GetComponent<LineRenderer>();
        line.positionCount = 2;

        line.SetPosition(0, skill.GetCastPoint().position);
        startInstance.transform.position = skill.GetCastPoint().position;

        endInstance.transform.position = target.position;
        line.SetPosition(1, target.position);

        startInstance.transform.LookAt(endInstance.transform.position);
        endInstance.transform.LookAt(startInstance.transform.position);

        float distance = Vector3.Distance(skill.GetCastPoint().position, target.position);
        line.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }
    // Update is called once per frame
    void Update()
    {
        if (curTime >= skill.skillData.duration)
        {
            DestroySelf();
        }
        else
        {
            ShootLineInDir();
        }

    }

    public override void DestroySelf()
    {
        Destroy(lineRendererInstance);
        if (startInstance != null)
            Destroy(startInstance);
        if (endInstance != null)
            Destroy(endInstance);
        base.DestroySelf();

    }
}
