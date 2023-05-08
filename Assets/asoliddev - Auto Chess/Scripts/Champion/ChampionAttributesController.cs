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

    //抵挡率
    public float defenseRate;
    //攻击频率
    public float attackIntervel;

    public int curHealth;
    public int curMana;

    public ChampionAttributesController(Champion champion)
    {
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
        attackDamage = new ChampionAttribute(attributesData.attackDamage, AttributeEnum.AttackDamage);
        defenseArmor = new ChampionAttribute(attributesData.defenseArmor, AttributeEnum.DefenseArmor);
        attackRange = new ChampionAttribute(attributesData.attackRange, AttributeEnum.AttackRange);
        attackSpeed = new ChampionAttribute(attributesData.attackSpeed, AttributeEnum.AttackSpeed);
        moveSpeed = new ChampionAttribute(attributesData.moveSpeed, AttributeEnum.MoveSpeed);
        maxHealth = new ChampionAttribute(attributesData.maxHealth, AttributeEnum.MaxHealth);
        maxMana = new ChampionAttribute(attributesData.maxMana, AttributeEnum.MaxMana);
    }
}
