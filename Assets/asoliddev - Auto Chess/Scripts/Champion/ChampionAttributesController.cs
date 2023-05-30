using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

[SerializeField]
public class ChampionAttributesController
{
    //攻击力
    public ChampionAttribute attackDamage;
    //护甲
    public ChampionAttribute defenseArmor;
    //攻击范围
    public ChampionAttribute attackRange;
    //攻击速度
    public ChampionAttribute attackSpeed;
    //移动
    public ChampionAttribute moveSpeed;
    //生命值
    public ChampionAttribute maxHealth;
    //法力值
    public ChampionAttribute maxMana;


    //闪避率
    public ChampionAttribute dodgeChange;
    //暴击率
    public ChampionAttribute critChange;
    //暴击倍数
    public ChampionAttribute critMultiple;

    //生命恢复
    public ChampionAttribute healthRegeneration;
    //法力恢复
    public ChampionAttribute manaRegeneration;

    //造成的伤害倍率
    public ChampionAttribute takeDamageMultiple;
    //受到的伤害倍率
    public ChampionAttribute applyDamageMultiple;

    //火属性伤害减免
    public ChampionAttribute fireDefenseRate;
    //水属性伤害减免
    public ChampionAttribute waterDefenseRate;
    //雷属性伤害减免
    public ChampionAttribute lightingDefenseRate;
    //土属性伤害减免
    public ChampionAttribute soilDefenseRate;

    public float curHealth;
    public float curMana;

    public ChampionAttributesController(ChampionBaseData champion)
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
        critMultiple = new ChampionAttribute(1);

        healthRegeneration = new ChampionAttribute(0);
        manaRegeneration = new ChampionAttribute(0);
        takeDamageMultiple = new ChampionAttribute(1);
        applyDamageMultiple = new ChampionAttribute(1);

        fireDefenseRate = new ChampionAttribute(0);
        waterDefenseRate = new ChampionAttribute(0);
        lightingDefenseRate = new ChampionAttribute(0);
        soilDefenseRate = new ChampionAttribute(0);
        UpdateLevelAttributes(champion, 1);
    }

    public void UpdateLevelAttributes(ChampionBaseData champion, int level)
    {

        string key = champion.ID + "_" + level;
        ChampionAttributesData attributesData =
            GameData.Instance._eeDataManager.Get<ChampionAttributesData>(key);
        /*switch (level)
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
        }*/
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

    //攻击伤害
    public float GetAttackDamage()
    {
        float randomValue = Random.Range(0, 1f);
        if (randomValue <= critChange.GetTrueMultipleValue())
        {
            return attackDamage.GetTrueLinearValue() * critMultiple.GetTrueLinearValue() * takeDamageMultiple.GetTrueLinearValue();
        }
        else
        {
            return attackDamage.GetTrueLinearValue() * takeDamageMultiple.GetTrueLinearValue();
        }
    }

    public bool DodgeCheck()
    {
        float randomValue = Random.Range(0, 1f);
        return randomValue <= dodgeChange.GetTrueMultipleValue();
    }

    public void Regenerate()
    {
        curHealth += healthRegeneration.GetTrueLinearValue() * Time.deltaTime;
        curMana += manaRegeneration.GetTrueLinearValue() * Time.deltaTime;
    }

    public float ApplyDamage(float dmg, DamageType type)
    {
        float trueDamage = dmg;
        trueDamage *= applyDamageMultiple.GetTrueLinearValue();
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
