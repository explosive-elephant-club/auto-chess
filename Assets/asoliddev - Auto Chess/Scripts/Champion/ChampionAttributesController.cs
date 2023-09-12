using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

[SerializeField]
public class ChampionAttributesController
{
    //护甲
    public ChampionAttribute defenseArmor;
    //移动
    public ChampionAttribute moveSpeed;
    //生命值
    public ChampionAttribute maxHealth;

    //技能范围增强
    public ChampionAttribute addRange;


    //闪避率
    public ChampionAttribute dodgeChange;
    //暴击率
    public ChampionAttribute critChange;
    //暴击倍数
    public ChampionAttribute critMultiple;

    //生命恢复
    public ChampionAttribute healthRegeneration;

    //造成的伤害倍率
    public ChampionAttribute takeDamageMultiple;
    //受到的伤害倍率
    public ChampionAttribute applyDamageMultiple;

    //燃烧属性伤害增强
    public ChampionAttribute fireDamage;
    //冰冻属性伤害增强
    public ChampionAttribute iceDamage;
    //电击属性伤害增强
    public ChampionAttribute lightingDamage;
    //酸蚀属性伤害增强
    public ChampionAttribute acidDamage;

    //燃烧属性伤害减免
    public ChampionAttribute fireDefenseRate;
    //冰冻属性伤害减免
    public ChampionAttribute iceDefenseRate;
    //电击属性伤害减免
    public ChampionAttribute lightingDefenseRate;
    //酸蚀属性伤害减免
    public ChampionAttribute acidDefenseRate;

    public float curHealth;

    public ChampionAttributesController(ChampionBaseData champion)
    {
        defenseArmor = new ChampionAttribute(0);
        moveSpeed = new ChampionAttribute(0);
        maxHealth = new ChampionAttribute(0);
        addRange = new ChampionAttribute(0);

        dodgeChange = new ChampionAttribute(0);
        critChange = new ChampionAttribute(0);
        critMultiple = new ChampionAttribute(1);

        healthRegeneration = new ChampionAttribute(0);
        takeDamageMultiple = new ChampionAttribute(1);
        applyDamageMultiple = new ChampionAttribute(1);

        fireDamage = new ChampionAttribute(0);
        iceDamage = new ChampionAttribute(0);
        lightingDamage = new ChampionAttribute(0);
        acidDamage = new ChampionAttribute(0);

        fireDefenseRate = new ChampionAttribute(0);
        iceDefenseRate = new ChampionAttribute(0);
        lightingDefenseRate = new ChampionAttribute(0);
        acidDefenseRate = new ChampionAttribute(0);
    }

    public void UpdateLevelAttributes(ChampionBaseData champion, int level)
    {

        string key = champion.ID + "_" + level;
        ChampionAttributesData attributesData =
            GameData.Instance._eeDataManager.Get<ChampionAttributesData>(key);
        defenseArmor.baseValue = attributesData.defenseArmor;
        moveSpeed.baseValue = attributesData.moveSpeed;
        maxHealth.baseValue = attributesData.maxHealth;

        curHealth = maxHealth.GetTrueLinearValue();
    }

    //物理减伤率
    public float GetPhysicalDefenseRate()
    {
        if (defenseArmor.GetTrueLinearValue() >= 0)
            return 13 * defenseArmor.GetTrueLinearValue() / (225 + 12 * defenseArmor.GetTrueLinearValue());
        else
            return 1 + defenseArmor.GetTrueLinearValue() / 100;
    }

    public bool DodgeCheck()
    {
        float randomValue = Random.Range(0, 1f);
        return randomValue <= dodgeChange.GetTrueMultipleValue();
    }

    public void Regenerate()
    {
        curHealth += healthRegeneration.GetTrueLinearValue() * Time.deltaTime;
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
    }
}
