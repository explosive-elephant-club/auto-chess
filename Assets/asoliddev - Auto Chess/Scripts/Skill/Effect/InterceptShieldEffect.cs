using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;
using System;

public class InterceptShieldEffect : SkillEffect
{
    public bool isAttach;
    public bool isOriginPos;
    protected float physicalCorrection;
    protected float fireCorrection;
    protected float iceCorrection;
    protected float lightningCorrection;
    protected float acidCorrection;
    protected float maxMech;
    protected float curMech;

    public override void Init(Skill _skill, Transform _target)
    {
        base.Init(_skill, _target);
        if (isAttach)
        {
            transform.parent = target.transform;
            if (isOriginPos)
                transform.localPosition = Vector3.zero;
        }

        physicalCorrection = float.Parse(GetParam("physicalCorrection"));
        fireCorrection = float.Parse(GetParam("fireCorrection"));
        iceCorrection = float.Parse(GetParam("iceCorrection"));
        lightningCorrection = float.Parse(GetParam("lightningCorrection"));
        acidCorrection = float.Parse(GetParam("acidCorrection"));
        maxMech = float.Parse(GetParam("maxMech"));
        curMech = maxMech;
    }

    void Update()
    {

    }

    public virtual float GetCorrectedDamage(float dmg, DamageType type)
    {
        float trueDamage = dmg;
        switch (type)
        {
            case DamageType.Physical:
                trueDamage *= physicalCorrection;
                break;
            case DamageType.Pure:
                break;
            case DamageType.Fire:
                trueDamage *= fireCorrection;
                break;
            case DamageType.Ice:
                trueDamage *= iceCorrection;
                break;
            case DamageType.Lightning:
                trueDamage *= lightningCorrection;
                break;
            case DamageType.Acid:
                trueDamage *= acidCorrection;
                break;
        }

        if (trueDamage <= 0)
        {
            trueDamage = 1;
        }
        return trueDamage;
    }

    public virtual void OnGotHit(ChampionController _attacker, SkillData.damageDataClass[] damages)
    {
        if (_attacker == null)
            return;
        _attacker.buffController.eventCenter.Broadcast(BuffActiveMode.BeforeAttack.ToString());
        List<SkillData.damageDataClass> addDamages = new List<SkillData.damageDataClass>();
        for (int i = 0; i < damages.Length; i++)
        {
            float trueDamage = _attacker.attributesController.GetTrueDamage(damages[i].dmg,
                (DamageType)Enum.Parse(typeof(DamageType), damages[i].type), damages[i].correction);
            if (_attacker.attributesController.CritCheck())
            {
                Debug.Log("暴击");
                trueDamage *= _attacker.attributesController.critMultiple.GetTrueValue();
                _attacker.buffController.eventCenter.Broadcast(BuffActiveMode.AfterCrit.ToString());
            }
            addDamages.Add(new SkillData.damageDataClass() { dmg = (int)trueDamage, type = damages[i].type });
        }
        _attacker.buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());

        foreach (var d in addDamages)
        {
            float trueDMG = GetCorrectedDamage(d.dmg, (DamageType)Enum.Parse(typeof(DamageType), d.type));

            if (curMech - trueDMG <= 0)
            {
                BrokenDestroy();
            }
            else
            {
                curMech = curMech - trueDMG;
            }
        }
    }

    public virtual void BrokenDestroy()
    {
        InstantiateHitEffect(transform.position);
        Destroy(gameObject);
    }

    protected override void OnTriggerEnter(Collider hit)
    {
    }

    protected override void OnTriggerExit(Collider hit)
    {
    }
}
