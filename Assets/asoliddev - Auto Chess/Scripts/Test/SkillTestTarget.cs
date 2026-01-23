using UnityEngine;
using ExcelConfig;
using System.Collections.Generic;

/// <summary>
/// 技能测试目标单位
/// 模拟被技能攻击的目标，可配置属性
/// </summary>
public class SkillTestTarget : MonoBehaviour
{
    [Header("=== 基础属性 ===")]
    [Tooltip("最大生命值")]
    public float maxHealth = 1000f;
    
    [Tooltip("当前生命值")]
    public float currentHealth = 1000f;
    
    [Tooltip("防御力")]
    public float defense = 50f;
    
    [Header("=== 抗性 ===")]
    [Tooltip("火焰抗性")]
    [Range(0, 1)]
    public float fireResistance = 0f;
    
    [Tooltip("冰霜抗性")]
    [Range(0, 1)]
    public float iceResistance = 0f;
    
    [Tooltip("雷电抗性")]
    [Range(0, 1)]
    public float lightningResistance = 0f;
    
    [Tooltip("腐蚀抗性")]
    [Range(0, 1)]
    public float acidResistance = 0f;
    
    [Header("=== 测试选项 ===")]
    [Tooltip("是否为无敌模式（只显示伤害不扣血）")]
    public bool invincible = false;
    
    [Tooltip("显示伤害数字")]
    public bool showDamageNumbers = true;
    
    [Tooltip("受击时闪烁")]
    public bool flashOnHit = true;
    
    [Header("=== 状态（只读）===")]
    [SerializeField] private bool isAlive = true;
    [SerializeField] private float totalDamageTaken = 0f;
    [SerializeField] private int hitCount = 0;
    [SerializeField] private List<string> damageLog = new();
    
    public bool IsAlive => isAlive;
    public float TotalDamageTaken => totalDamageTaken;
    public int HitCount => hitCount;
    
    // 用于模拟ChampionController的必要属性
    public ChampionTeam Team { get; private set; } = ChampionTeam.Oponent;
    
    private Renderer _renderer;
    private Color _originalColor;
    private float _flashTimer;
    private const float FLASH_DURATION = 0.1f;

    public void Initialize()
    {
        currentHealth = maxHealth;
        isAlive = true;
        totalDamageTaken = 0;
        hitCount = 0;
        damageLog.Clear();
        
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
        }
    }

    private void Update()
    {
        // 处理受击闪烁
        if (_flashTimer > 0)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0 && _renderer != null)
            {
                _renderer.material.color = _originalColor;
            }
        }
    }

    /// <summary>
    /// 接收伤害
    /// </summary>
    public void TakeDamage(float rawDamage, DamageType damageType = DamageType.Physical)
    {
        if (!isAlive) return;
        
        float resistance = GetResistance(damageType);
        float actualDamage = CalculateActualDamage(rawDamage, resistance);
        
        hitCount++;
        totalDamageTaken += actualDamage;
        
        string logEntry = $"[{hitCount}] {damageType}: {rawDamage:F0} -> {actualDamage:F0} (抗性:{resistance:P0})";
        damageLog.Add(logEntry);
        
        if (showDamageNumbers)
        {
            Debug.Log($"[SkillTestTarget] {gameObject.name} {logEntry}");
        }
        
        if (!invincible)
        {
            currentHealth -= actualDamage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath();
            }
        }
        
        if (flashOnHit)
        {
            Flash();
        }
    }

    /// <summary>
    /// 接收技能伤害数据
    /// </summary>
    public void TakeDamage(SkillData.damageDataClass[] damages)
    {
        if (damages == null) return;
        
        foreach (var dmg in damages)
        {
            DamageType type = DamageType.Physical;
            if (!string.IsNullOrEmpty(dmg.type))
            {
                System.Enum.TryParse(dmg.type, out type);
            }
            TakeDamage(dmg.dmg, type);
        }
    }

    /// <summary>
    /// 重置状态
    /// </summary>
    public void ResetState()
    {
        currentHealth = maxHealth;
        isAlive = true;
        totalDamageTaken = 0;
        hitCount = 0;
        damageLog.Clear();
        
        if (_renderer != null)
        {
            _renderer.material.color = _originalColor;
        }
        
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 清空伤害日志
    /// </summary>
    [ContextMenu("清空伤害日志")]
    public void ClearDamageLog()
    {
        damageLog.Clear();
        totalDamageTaken = 0;
        hitCount = 0;
    }

    private float GetResistance(DamageType type)
    {
        return type switch
        {
            DamageType.Fire => fireResistance,
            DamageType.Ice => iceResistance,
            DamageType.Lightning => lightningResistance,
            DamageType.Acid => acidResistance,
            _ => 0f
        };
    }

    private float CalculateActualDamage(float rawDamage, float resistance)
    {
        // 简化的伤害计算：原始伤害 * (1 - 抗性) * 防御减免
        float defenseMultiplier = 100f / (100f + defense);
        return rawDamage * (1f - resistance) * defenseMultiplier;
    }

    private void OnDeath()
    {
        isAlive = false;
        Debug.Log($"[SkillTestTarget] {gameObject.name} 死亡! 总受伤: {totalDamageTaken:F0}, 受击次数: {hitCount}");
        
        if (_renderer != null)
        {
            _renderer.material.color = Color.gray;
        }
    }

    private void Flash()
    {
        if (_renderer != null)
        {
            _renderer.material.color = Color.red;
            _flashTimer = FLASH_DURATION;
        }
    }

    private void OnDrawGizmos()
    {
        // 绘制生命值指示
        float healthRatio = maxHealth > 0 ? currentHealth / maxHealth : 0;
        
        Gizmos.color = Color.Lerp(Color.red, Color.green, healthRatio);
        Vector3 healthBarPos = transform.position + Vector3.up * 2.5f;
        Gizmos.DrawCube(healthBarPos, new Vector3(healthRatio, 0.1f, 0.1f));
        
        // 外框
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(healthBarPos, new Vector3(1, 0.1f, 0.1f));
    }
}
