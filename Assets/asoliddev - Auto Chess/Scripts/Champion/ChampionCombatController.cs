using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;
using System;
public class ChampionCombatController
{
    ChampionController championController;
    BuffController buffController;
    ChampionAttributesController attributesController;
    public ChampionCombatController(ChampionController _championController)
    {
        championController = _championController;
    }

    public void TakeDamage(ChampionController _target, SkillData.damageDataClass[] damages, float fix = 1)
    {
        buffController = championController.buffController;
        attributesController = championController.attributesController;

        if (_target == null)
            return;
        buffController.eventCenter.Broadcast(BuffActiveMode.BeforeAttack.ToString());

        if (_target.skillController.curVoidShieldEffect != null)
        {
            _target.skillController.curVoidShieldEffect.OnGotHit(championController, damages);
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());
            return;
        }

        float crit = 1;
        if (attributesController.CritCheck())
        {
            Debug.Log("暴击");
            crit = attributesController.critMultiple.GetTrueValue();
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterCrit.ToString());
        }
        List<SkillData.damageDataClass> addDamages = new List<SkillData.damageDataClass>();
        for (int i = 0; i < damages.Length; i++)
        {
            float trueDamage = attributesController.GetTrueDamage(damages[i].dmg,
                (DamageType)Enum.Parse(typeof(DamageType), damages[i].type), damages[i].correction);
            trueDamage *= fix;
            trueDamage *= crit;

            championController.totalDamage += trueDamage;
            addDamages.Add(new SkillData.damageDataClass() { dmg = (int)trueDamage, type = damages[i].type });
        }
        _target.championCombatController.OnGotHit(addDamages);
        buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());
    }

    public void TakeDamage(ChampionController _target, float dmg, DamageType damageType, float correction = 0, float fix = 1)
    {
        buffController = championController.buffController;
        attributesController = championController.attributesController;

        if (_target == null)
            return;
        buffController.eventCenter.Broadcast(BuffActiveMode.BeforeAttack.ToString());
        float crit = 1;
        if (attributesController.CritCheck())
        {
            Debug.Log("暴击");
            crit = attributesController.critMultiple.GetTrueValue();
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterCrit.ToString());
        }
        float trueDamage = attributesController.GetTrueDamage(dmg, damageType, correction);
        trueDamage *= fix;
        trueDamage *= crit;
        OnGotHit(trueDamage, damageType);

        championController.totalDamage += trueDamage;

        buffController.eventCenter.Broadcast(BuffActiveMode.AfterAttack.ToString());
    }

    public bool OnGotHit(List<SkillData.damageDataClass> addDamages)
    {
        buffController = championController.buffController;
        attributesController = championController.attributesController;

        if (attributesController.HitCheck())
        {
            buffController.eventCenter.Broadcast(BuffActiveMode.BeforeHit.ToString());

            foreach (var d in addDamages)
            {
                float trueDMG = attributesController.ApplyDamage(d.dmg, (DamageType)Enum.Parse(typeof(DamageType), d.type));

                //add floating text
                WorldCanvasController.Instance.AddDamageText(championController.transform.position + new Vector3(0, 2.5f, 0), trueDMG);
                //death
                if (attributesController.curHealth <= 0)
                {
                    championController.Dead();
                }
            }

            buffController.eventCenter.Broadcast(BuffActiveMode.AfterHit.ToString());
        }
        else
        {
            Debug.Log("闪避");
            WorldCanvasController.Instance.AddDamageText(championController.transform.position + new Vector3(0, 2.5f, 0), 0);
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterDodge.ToString());
        }
        return championController.isDead;
    }

    public bool OnGotHit(float dmg, DamageType damageType)
    {
        buffController = championController.buffController;
        attributesController = championController.attributesController;

        if (attributesController.HitCheck())
        {
            buffController.eventCenter.Broadcast(BuffActiveMode.BeforeHit.ToString());

            float trueDMG = attributesController.ApplyDamage(dmg, damageType);
            //add floating text
            WorldCanvasController.Instance.AddDamageText(championController.transform.position + new Vector3(0, 2.5f, 0), trueDMG);
            switch (damageType)
            {
                case DamageType.Fire:
                    attributesController.fireResistance.OnGetHit(trueDMG);
                    break;
                case DamageType.Ice:
                    attributesController.iceResistance.OnGetHit(trueDMG);
                    break;
                case DamageType.Lightning:
                    attributesController.lightningResistance.OnGetHit(trueDMG);
                    break;
                case DamageType.Acid:
                    attributesController.acidResistance.OnGetHit(trueDMG);
                    break;
                default:
                    break;
            }
            //death
            if (attributesController.curHealth <= 0)
            {
                championController.Dead();
            }
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterHit.ToString());
        }
        else
        {
            Debug.Log("闪避");
            buffController.eventCenter.Broadcast(BuffActiveMode.AfterDodge.ToString());
        }
        return championController.isDead;
    }
}
