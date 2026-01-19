using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;
using System;

public class LightningShieldEffect : VoidShieldEffect
{
    float t = 0;
    float intervel = 2f;
    float reverse = 0;

    public ParticleSystem lensEffect;
    public ParticleSystem lightningEffect;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        intervel = float.Parse(GetParam("intervel"));
    }

    void Update()
    {
        if (t > 0)
        {
            t -= Time.deltaTime;
        }
        else
        {
            t = 0;
        }
    }

    public override void OnGotHit(ChampionController _attacker, SkillData.damageDataClass[] damages)
    {
        base.OnGotHit(_attacker, damages);
        ReverseDamage(_attacker);
    }

    void ReverseDamage(ChampionController _attacker)
    {
        t = intervel;
        lensEffect.Play();
        var main = lightningEffect.main;
        main.startSizeY = skill.owner.GetDistance(_attacker) * 2f;
        lightningEffect.transform.LookAt(_attacker.transform.position + new Vector3(0, 1f, 0));
        lightningEffect.Play();
        skill.AddDMGToTarget(_attacker);
    }
}
