using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttributeData", menuName = "AutoChess/Champion", order = 5)]
public class ChampionAttributeData : ScriptableObject
{
    public float attackDamage;
    public float defenseArmor;
    public float attackRange;
    public float attackSpeed;
    public float moveSpeed;
    public float maxHealth;
    public float maxMana;
}
