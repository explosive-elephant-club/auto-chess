using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;
using UnityEngine.UIElements;

[SerializeField]
public class ChampionAttributesController
{
    ChampionController championController;

    //护盾值
    public ChampionAttribute maxArmor;
    //机体值
    public ChampionAttribute maxHealth;
    //法力值
    public ChampionAttribute maxMana;

    //移动
    public ChampionAttribute moveSpeed;
    //技能范围增强
    public ChampionAttribute addRange;
    //提供电力
    public ChampionAttribute electricPower;
    //施法延迟
    public ChampionAttribute castDelay;
    //充能延迟
    public ChampionAttribute chargingDelay;


    //受击命中率 闪避率=1-受击命中率 hitRate*-0.3=闪避率+30%
    public ChampionAttribute hitRate;
    //非暴击率 暴击率=1-非暴击率 nonCritChange*-0.3=暴击率+30%
    public ChampionAttribute nonCritChange;
    //暴击倍数
    public ChampionAttribute critMultiple;

    //护甲恢复
    public ChampionAttribute armorRegeneration;
    //法力恢复
    public ChampionAttribute manaRegeneration;

    //造成的伤害倍率
    public ChampionAttribute takeDamageMultiple;
    //受到的伤害倍率
    public ChampionAttribute applyDamageMultiple;

    //物理属性伤害
    public ChampionAttribute physicalDamage;
    //燃烧属性伤害
    public ChampionAttribute fireDamage;
    //冰冻属性伤害
    public ChampionAttribute iceDamage;
    //电击属性伤害
    public ChampionAttribute lightingDamage;
    //酸蚀属性伤害
    public ChampionAttribute acidDamage;

    //物理属性伤害承受率 减伤率=1-伤害承受率 DamageApplyRate*-0.3=减伤率+30%
    public ChampionAttribute physicalDamageApplyRate;
    //燃烧属性伤害承受率
    public ChampionAttribute fireDamageApplyRate;
    //冰冻属性伤害承受率
    public ChampionAttribute iceDamageApplyRate;
    //电击属性伤害承受率
    public ChampionAttribute lightingDamageApplyRate;
    //酸蚀属性伤害承受率
    public ChampionAttribute acidDamageApplyRate;

    //火焰抗性
    public Resistance fireResistance;
    //冰冻抗性
    public Resistance iceResistance;
    //电击抗性
    public Resistance lightningResistance;
    //酸蚀抗性
    public Resistance acidResistance;

    public float curHealth;
    public float curArmor;
    public float curMana;

    float healthRate = 1f;

    public ChampionAttributesController(ChampionController _championController)
    {
        championController = _championController;

        maxArmor = new ChampionAttribute(0, "MaxArmor");
        maxHealth = new ChampionAttribute(0, "maxHealth");
        maxMana = new ChampionAttribute(0, "maxMana");

        moveSpeed = new ChampionAttribute(0, "MoveSpeed");
        addRange = new ChampionAttribute(0, "AddRange");
        electricPower = new ChampionAttribute(0, "ElectricPower");
        castDelay = new ChampionAttribute(2, "CastDelay");
        chargingDelay = new ChampionAttribute(2, "ChargingDelay");

        hitRate = new ChampionAttribute(1, "HitRate");
        nonCritChange = new ChampionAttribute(1, "CritChange");
        critMultiple = new ChampionAttribute(1.5f, "CritMultiple");

        armorRegeneration = new ChampionAttribute(1, "ArmorRegeneration");
        manaRegeneration = new ChampionAttribute(4, "ManaRegeneration");
        takeDamageMultiple = new ChampionAttribute(1, "TakeDamageMultiple");
        applyDamageMultiple = new ChampionAttribute(1, "ApplyDamageMultiple");

        physicalDamage = new ChampionAttribute(0, "PhysicalDamage");
        fireDamage = new ChampionAttribute(0, "FireDamage");
        iceDamage = new ChampionAttribute(0, "IceDamage");
        lightingDamage = new ChampionAttribute(0, "LightingDamage");
        acidDamage = new ChampionAttribute(0, "AcidDamage");

        physicalDamageApplyRate = new ChampionAttribute(1, "PhysicalDamageApplyRate");
        fireDamageApplyRate = new ChampionAttribute(1, "FireDamageApplyRate");
        iceDamageApplyRate = new ChampionAttribute(1, "IceDamageApplyRate");
        lightingDamageApplyRate = new ChampionAttribute(1, "LightingDamageApplyRate");
        acidDamageApplyRate = new ChampionAttribute(1, "AcidDamageApplyRate");

        fireResistance = new Resistance(10, 50, 10);
        iceResistance = new Resistance(10, 50, 10);
        lightningResistance = new Resistance(10, 50, 10);
        acidResistance = new Resistance(10, 50, 10);

        fireResistance.callAction = () => { championController.buffController.AddBuff(301); };
        iceResistance.callAction = () => { championController.buffController.AddBuff(302); };
        lightningResistance.callAction = () => { championController.buffController.AddBuff(303); };
        acidResistance.callAction = () => { championController.buffController.AddBuff(304); };

        healthRate = 1f;
    }

    public bool HitCheck()
    {
        float randomValue = Random.Range(0, 1f);
        return randomValue <= hitRate.GetTrueValue();
    }

    public bool CritCheck()
    {
        float randomValue = Random.Range(0, 1f);
        return randomValue <= (1 - nonCritChange.GetTrueValue());
    }

    public void Regenerate()
    {
        if ((curArmor + armorRegeneration.GetTrueValue() * Time.deltaTime) < maxArmor.GetTrueValue())
            curArmor += armorRegeneration.GetTrueValue() * Time.deltaTime;
        else
            curArmor = maxArmor.GetTrueValue();

        if ((curMana + manaRegeneration.GetTrueValue() * Time.deltaTime) < maxMana.GetTrueValue())
            curMana += manaRegeneration.GetTrueValue() * Time.deltaTime;
        else
            curMana = maxMana.GetTrueValue();


        fireResistance.Recover();
        iceResistance.Recover();
        lightningResistance.Recover();
        acidResistance.Recover();

    }

    public float GetTrueDamage(float dmg, DamageType type, float correction = 1)
    {
        float trueDamage = dmg;
        trueDamage *= takeDamageMultiple.GetTrueValue();
        switch (type)
        {
            case DamageType.Physical:
                trueDamage += physicalDamage.GetTrueValue() * correction;
                break;
            case DamageType.Pure:
                break;
            case DamageType.Fire:
                trueDamage += fireDamage.GetTrueValue() * correction;
                break;
            case DamageType.Ice:
                trueDamage += iceDamage.GetTrueValue() * correction;
                break;
            case DamageType.Lightning:
                trueDamage += lightingDamage.GetTrueValue() * correction;
                break;
            case DamageType.Acid:
                trueDamage += acidDamage.GetTrueValue() * correction;
                break;
        }

        if (trueDamage <= 0)
        {
            trueDamage = 1;
        }
        return trueDamage;
    }

    public float ApplyDamage(float dmg, DamageType type)
    {
        float trueDamage = dmg;
        trueDamage *= applyDamageMultiple.GetTrueValue();
        switch (type)
        {
            case DamageType.Physical:
                trueDamage *= physicalDamageApplyRate.GetTrueValue();
                break;
            case DamageType.Pure:
                break;
            case DamageType.Fire:
                trueDamage *= fireDamageApplyRate.GetTrueValue();
                fireResistance.OnGetHit(trueDamage);
                break;
            case DamageType.Ice:
                trueDamage *= iceDamageApplyRate.GetTrueValue();
                iceResistance.OnGetHit(trueDamage);
                break;
            case DamageType.Lightning:
                trueDamage *= lightingDamageApplyRate.GetTrueValue();
                lightningResistance.OnGetHit(trueDamage);
                break;
            case DamageType.Acid:
                trueDamage *= acidDamageApplyRate.GetTrueValue();
                acidResistance.OnGetHit(trueDamage);
                break;
        }
        trueDamage = Mathf.Floor(trueDamage);
        if (curArmor > trueDamage)
        {
            curArmor -= trueDamage;
        }
        else
        {
            float overflowDamage = trueDamage - curArmor;
            curArmor = 0;//击破护甲
            if (curHealth > trueDamage)
            {
                curHealth -= trueDamage;
            }
            else
            {
                curHealth = 0;
            }
        }


        return trueDamage;
    }

    public void Reset()
    {
        healthRate = curHealth / maxHealth.GetTrueValue();
        curArmor = maxArmor.GetTrueValue();
        curMana = maxMana.GetTrueValue();

        fireResistance.Reset();
        iceResistance.Reset();
        lightningResistance.Reset();
        acidResistance.Reset();
    }

    //重新计算最大值增加后的数值
    public void RecalculateAfterMaxChange()
    {
        curHealth = Mathf.FloorToInt(healthRate * maxHealth.GetTrueValue());
        curArmor = maxArmor.GetTrueValue();
        curMana = maxMana.GetTrueValue();
    }
}
