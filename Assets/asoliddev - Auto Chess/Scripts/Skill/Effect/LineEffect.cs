using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffect : SkillEffect
{
    LineRenderer lineRenderer;
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        lineRenderer = GetComponent<LineRenderer>();
        InstantiateEmitEffect();
    }

    protected override void InstantiateEmitEffect()
    {
        emitParticleInstance = Instantiate(skill.emitPrefab, transform.position, transform.rotation) as GameObject;
        emitParticleInstance.transform.parent = transform;
        hitParticleInstance = Instantiate(skill.hitFXPrefab, target.position, Quaternion.FromToRotation(Vector3.up, Vector3.zero)) as GameObject;
        hitParticleInstance.transform.parent = transform;
    }

    void ShootLineInDir()
    {
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, skill.GetCastPoint().position);
        emitParticleInstance.transform.position = skill.GetCastPoint().position;

        hitParticleInstance.transform.position = target.position;
        lineRenderer.SetPosition(1, target.position);

        emitParticleInstance.transform.LookAt(hitParticleInstance.transform.position);
        hitParticleInstance.transform.LookAt(emitParticleInstance.transform.position);

        float distance = Vector3.Distance(skill.GetCastPoint().position, target.position);
        lineRenderer.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        lineRenderer.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
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

}
