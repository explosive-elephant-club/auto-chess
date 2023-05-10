using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionAttributesController
{
    public ChampionAttribute attackDamage;
    public ChampionAttribute defenseArmor;
    public ChampionAttribute attackRange;
    public ChampionAttribute attackSpeed;
    public ChampionAttribute moveSpeed;
    public ChampionAttribute maxHealth;
    public ChampionAttribute maxMana;

    public ChampionAttribute dodgeChange;
    public ChampionAttribute critChange;
    public ChampionAttribute critMultiple;

    public ChampionAttribute fireDefenseRate;
    public ChampionAttribute waterDefenseRate;
    public ChampionAttribute lightingDefenseRate;
    public ChampionAttribute soilDefenseRate;

    public float curHealth;
    public float curMana;

    public ChampionAttributesController(Champion champion)
    {
        attackDamage = new ChampionAttribute(0);
        defenseArmor = new ChampionAttribute(0);
        attackRange = new ChampionAttribute(0);
        attackSpeed = new ChampionAttribute(0);
        moveSpeed = new ChampionAttribute(0);
        maxHealth = new ChampionAttribute(0);
        maxMana = new ChampionAttribute(0);
        dodgeChange = new ChampionAttribute(0);
        critChange = new ChampionAttribute(0);
        critMultiple = new ChampionAttribute(0);
        fireDefenseRate = new ChampionAttribute(0);
        waterDefenseRate = new ChampionAttribute(0);
        lightingDefenseRate = new ChampionAttribute(0);
        soilDefenseRate = new ChampionAttribute(0);
        UpdateLevelAttributes(champion, 1);
    }

    public void UpdateLevelAttributes(Champion champion, int level)
    {
        ChampionAttributeData attributesData = champion.level1_Attribute;
        switch (level)
        {
            case 1:
                attributesData = champion.level1_Attribute;
                break;
            case 2:
                attributesData = champion.level2_Attribute;
                break;
            case 3:
                attributesData = champion.level3_Attribute;
                break;
        }
        attackDamage.baseValue = attributesData.attackDamage;
        defenseArmor.baseValue = attributesData.defenseArmor;
        attackRange.baseValue = attributesData.attackRange;
        attackSpeed.baseValue = attributesData.attackSpeed;
        moveSpeed.baseValue = attributesData.moveSpeed;
        maxHealth.baseValue = attributesData.maxHealth;
        maxMana.baseValue = attributesData.maxMana;

        curHealth = maxHealth.GetTrueLinearValue();
        curMana = maxMana.GetTrueLinearValue();
    }

    //物理减伤率
    public float GetPhysicalDefenseRate()
    {
        if (defenseArmor.GetTrueLinearValue() >= 0)
            return 13 * defenseArmor.GetTrueLinearValue() / (225 + 12 * defenseArmor.GetTrueLinearValue());
        else
            return 1 + defenseArmor.GetTrueLinearValue() / 100;
    }

    //攻击频率
    public float GetAttackIntervel()
    {
        float tempValue = 200 / (100 + attackSpeed.GetTrueLinearValue());
        return tempValue < 0.2f ? 0.2f : tempValue;
    }

    public bool DodgeCheck()
    {
        float randomValue = Random.Range(0, 1f);
        return randomValue <= dodgeChange.GetTrueMultipleValue();
    }

    public float ApplyDamage(float dmg, DamageType type)
    {
        float trueDamage = dmg;
        switch (type)
        {
            case DamageType.Physical:
                trueDamage *= (1 - GetPhysicalDefenseRate());
                break;
            case DamageType.Pure:
                break;
            case DamageType.Fire:
                trueDamage *= (1 - fireDefenseRate.GetTrueMultipleValue());
                break;
            case DamageType.Water:
                trueDamage *= (1 - waterDefenseRate.GetTrueMultipleValue());
                break;
            case DamageType.Lightning:
                trueDamage *= (1 - lightingDefenseRate.GetTrueMultipleValue());
                break;
            case DamageType.Soil:
                trueDamage *= (1 - soilDefenseRate.GetTrueMultipleValue());
                break;
        }

        if (curHealth > trueDamage)
        {
            curHealth -= Mathf.Floor(trueDamage);
        }
        else
        {
            curHealth = 0;
        }
        return trueDamage;
    }

    public void Reset()
    {
        curHealth = maxHealth.GetTrueLinearValue();
        curMana = maxMana.GetTrueLinearValue();
    }
}
