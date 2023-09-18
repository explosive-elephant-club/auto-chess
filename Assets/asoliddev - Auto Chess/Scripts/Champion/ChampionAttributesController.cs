using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;
using UnityEngine.UIElements;

[SerializeField]
public class ChampionAttributesController
{
    //护盾值
    public ChampionAttribute maxArmor;
    //机体值
    public ChampionAttribute maxHealth;
    //移动
    public ChampionAttribute moveSpeed;
    //技能范围增强
    public ChampionAttribute addRange;
    //提供电力
    public ChampionAttribute electricPower;


    //闪避率
    public ChampionAttribute dodgeChange;
    //暴击率
    public ChampionAttribute critChange;
    //暴击倍数
    public ChampionAttribute critMultiple;

    //护甲恢复
    public ChampionAttribute armorRegeneration;

    //造成的伤害倍率
    public ChampionAttribute takeDamageMultiple;
    //受到的伤害倍率
    public ChampionAttribute applyDamageMultiple;

    //物理属性伤害增强
    public ChampionAttribute physicalDamage;
    //燃烧属性伤害增强
    public ChampionAttribute fireDamage;
    //冰冻属性伤害增强
    public ChampionAttribute iceDamage;
    //电击属性伤害增强
    public ChampionAttribute lightingDamage;
    //酸蚀属性伤害增强
    public ChampionAttribute acidDamage;

    //物理属性伤害减免
    public ChampionAttribute physicalDefenseRate;
    //燃烧属性伤害减免
    public ChampionAttribute fireDefenseRate;
    //冰冻属性伤害减免
    public ChampionAttribute iceDefenseRate;
    //电击属性伤害减免
    public ChampionAttribute lightingDefenseRate;
    //酸蚀属性伤害减免
    public ChampionAttribute acidDefenseRate;

    public float curHealth;
    public float curArmor;

    public ChampionAttributesController()
    {
        maxArmor = new ChampionAttribute(0);
        moveSpeed = new ChampionAttribute(0);
        maxHealth = new ChampionAttribute(0);
        addRange = new ChampionAttribute(0);
        electricPower = new ChampionAttribute(0);

        dodgeChange = new ChampionAttribute(0);
        critChange = new ChampionAttribute(0);
        critMultiple = new ChampionAttribute(1);

        armorRegeneration = new ChampionAttribute(0);
        takeDamageMultiple = new ChampionAttribute(1);
        applyDamageMultiple = new ChampionAttribute(1);

        physicalDamage = new ChampionAttribute(0);
        fireDamage = new ChampionAttribute(0);
        iceDamage = new ChampionAttribute(0);
        lightingDamage = new ChampionAttribute(0);
        acidDamage = new ChampionAttribute(0);

        physicalDefenseRate = new ChampionAttribute(0);
        fireDefenseRate = new ChampionAttribute(0);
        iceDefenseRate = new ChampionAttribute(0);
        lightingDefenseRate = new ChampionAttribute(0);
        acidDefenseRate = new ChampionAttribute(0);
    }

    public bool DodgeCheck()
    {
        float randomValue = Random.Range(0, 1f);
        return randomValue <= dodgeChange.GetTrueMultipleValue();
    }

    public void Regenerate()
    {
        if ((curArmor + armorRegeneration.GetTrueLinearValue() * Time.deltaTime) < maxArmor.GetTrueLinearValue())
            curArmor += armorRegeneration.GetTrueLinearValue() * Time.deltaTime;
        else
            curArmor = maxArmor.GetTrueLinearValue();
    }

    public float GetTrueDamage(float dmg, DamageType type)
    {
        float trueDamage = dmg;
        trueDamage *= takeDamageMultiple.GetTrueLinearValue();
        switch (type)
        {
            case DamageType.Physical:
                trueDamage = physicalDamage.GetTrueLinearValue(trueDamage);
                break;
            case DamageType.Pure:
                break;
            case DamageType.Fire:
                trueDamage = fireDamage.GetTrueLinearValue(trueDamage);
                break;
            case DamageType.Ice:
                trueDamage = iceDamage.GetTrueLinearValue(trueDamage);
                break;
            case DamageType.Lightning:
                trueDamage = lightingDamage.GetTrueLinearValue(trueDamage);
                break;
            case DamageType.Acid:
                trueDamage = acidDamage.GetTrueLinearValue(trueDamage);
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
        trueDamage *= applyDamageMultiple.GetTrueLinearValue();
        switch (type)
        {
            case DamageType.Physical:
                trueDamage *= (1 - physicalDefenseRate.GetTrueMultipleValue());
                break;
            case DamageType.Pure:
                break;
            case DamageType.Fire:
                trueDamage *= (1 - fireDefenseRate.GetTrueMultipleValue());
                break;
            case DamageType.Ice:
                trueDamage *= (1 - iceDefenseRate.GetTrueMultipleValue());
                break;
            case DamageType.Lightning:
                trueDamage *= (1 - lightingDefenseRate.GetTrueMultipleValue());
                break;
            case DamageType.Acid:
                trueDamage *= (1 - acidDefenseRate.GetTrueMultipleValue());
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
        curHealth = maxHealth.GetTrueLinearValue();
        curArmor = maxArmor.GetTrueLinearValue();
    }
}
