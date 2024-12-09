using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineEffect : SkillEffect
{
    LineRenderer lineRenderer;
    public float textureScrollSpeed = 8f; //How fast the texture scrolls along the beam
    public float textureLengthScale = 3; //Length of the beam texture
    float t = 0;
    float intervel = 0;
    int effectEffectiveTimes;

    Vector3 targetPoint;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        lineRenderer = GetComponent<LineRenderer>();
        effectEffectiveTimes = int.Parse(GetParam("effectEffectiveTimes"));
        intervel = duration / effectEffectiveTimes;
        InstantiateEmitEffect();
        InstantiateHitEffect(target.position);
    }

    protected override void InstantiateEmitEffect()
    {
        base.InstantiateEmitEffect();
        emitParticleInstance.transform.parent = transform;

        hitParticleInstance = Instantiate(skill.hitFXPrefab, target.position, Quaternion.FromToRotation(Vector3.up, Vector3.zero)) as GameObject;
        hitParticleInstance.transform.parent = transform;
    }

    protected override void InstantiateHitEffect(Vector3 pos)
    {
        base.InstantiateHitEffect(pos);
        hitParticleInstance.transform.parent = transform;
    }

    void ShootLineInDir()
    {
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, skill.GetCastPoint().position);
        emitParticleInstance.transform.position = skill.GetCastPoint().position;

        hitParticleInstance.transform.position = targetPoint;
        lineRenderer.SetPosition(1, targetPoint);

        emitParticleInstance.transform.LookAt(hitParticleInstance.transform.position);
        hitParticleInstance.transform.LookAt(emitParticleInstance.transform.position);

        float distance = Vector3.Distance(skill.GetCastPoint().position, targetPoint);
        lineRenderer.sharedMaterial.mainTextureScale = new Vector2(distance / textureLengthScale, 1);
        lineRenderer.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * textureScrollSpeed, 0);
    }

    void RayCheck()
    {
        RaycastHit[] raycastHits;
        raycastHits = Physics.RaycastAll(skill.GetCastPoint().position, skill.GetCastPoint().forward, 100f);
        hits.Clear();
        for (int i = 0; i < raycastHits.Length; i++)
        {
            RaycastHit hit = raycastHits[i];
            ChampionController c = hit.transform.GetComponentInParent<ChampionController>();

            if (c)
            {
                if (skill.skillTargetType == SkillTargetType.Teammate)
                {
                    if (c.team == skill.owner.team)
                    {
                        hits.Add(c);
                        targetPoint = hit.point;
                        return;
                    }
                }
                else if (skill.skillTargetType == SkillTargetType.Enemy)
                {
                    if (c.team != skill.owner.team)
                    {
                        hits.Add(c);
                        targetPoint = hit.point;
                        return;
                    }
                }
            }
        }
        targetPoint = skill.GetCastPoint().position + skill.GetCastPoint().forward * 100f;
        return;
    }
    // Update is called once per frame
    public virtual void Update()
    {
        if (t < intervel)
        {
            t += Time.deltaTime;
        }
        else
        {
            t = 0;
            skill.DirectEffectFunc();
        }
        if (curTime >= skill.skillData.duration)
        {
            DestroySelf();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        RayCheck();
        ShootLineInDir();
    }

    protected override void OnCollideShieldBegin(Collider hit)
    {
        InstantiateHitEffect(hit.bounds.ClosestPoint(transform.position));
    }
}
