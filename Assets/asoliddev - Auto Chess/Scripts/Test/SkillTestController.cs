using System;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

/// <summary>
/// 技能测试控制器
/// 用于在Unity编辑器中快速测试技能效果
/// </summary>
public class SkillTestController : MonoBehaviour
{
    [Header("=== 技能配置 ===")]
    [Tooltip("要测试的技能ID")]
    public int skillID = 1;
    
    [Tooltip("自动从配置表加载技能数据")]
    public bool autoLoadFromConfig = true;
    
    [Header("=== 技能参数覆盖 ===")]
    [Tooltip("是否启用参数覆盖（用于测试不同数值）")]
    public bool enableOverride = false;
    
    [Tooltip("覆盖：技能持续时间")]
    public float overrideDuration = 1f;
    
    [Tooltip("覆盖：生效次数")]
    public int overrideEffectCounts = 1;
    
    [Tooltip("覆盖：技能射程")]
    public int overrideDistance = 10;
    
    [Tooltip("覆盖：技能范围")]
    public int overrideRange = 5;
    
    [Tooltip("覆盖：移动速度")]
    public float overrideMoveSpeed = 10f;
    
    [Tooltip("覆盖：攻击类型")]
    public SkillHelper.SkillAttackType overrideAttackType = SkillHelper.SkillAttackType.BulletLinearTrajectory;
    
    [Header("=== 目标设置 ===")]
    [Tooltip("目标对象列表")]
    public List<SkillTestTarget> targets = new();
    
    [Tooltip("自动生成目标")]
    public bool autoGenerateTargets = true;
    
    [Tooltip("自动生成目标的数量")]
    public int autoTargetCount = 3;
    
    [Tooltip("目标生成半径")]
    public float targetSpawnRadius = 5f;
    
    [Header("=== 施法者设置 ===")]
    [Tooltip("施法者预制体")]
    public GameObject casterPrefab;
    
    [Tooltip("施法者生成位置")]
    public Transform casterSpawnPoint;
    
    [Header("=== 测试控制 ===")]
    [Tooltip("循环测试间隔（秒），0表示不循环")]
    public float loopInterval = 0f;
    
    [Tooltip("显示调试信息")]
    public bool showDebugInfo = true;
    
    [Header("=== 运行时状态（只读）===")]
    [SerializeField] private string currentSkillName = "";
    [SerializeField] private SkillPhase currentPhase = SkillPhase.Idle;
    [SerializeField] private float currentDuration = 0f;
    [SerializeField] private int currentEffectCount = 0;
    [SerializeField] private bool isTestRunning = false;
    
    // 运行时引用
    private ChampionController _caster;
    private SkillData _skillData;
    private ISkillState _skillState;
    private SkillStateMachine _stateMachine;
    private SkillRuntime _runtime;
    private float _loopTimer;
    private List<GameObject> _generatedTargets = new();
    
    // 简化的测试施法者组件
    private SkillTestCaster _testCaster;

    private void Start()
    {
        if (autoLoadFromConfig)
        {
            LoadSkillData();
        }
    }

    private void Update()
    {
        if (isTestRunning)
        {
            UpdateSkillTest();
        }
        
        // 循环测试
        if (loopInterval > 0 && !isTestRunning)
        {
            _loopTimer += Time.deltaTime;
            if (_loopTimer >= loopInterval)
            {
                _loopTimer = 0;
                CastSkill();
            }
        }
    }

    /// <summary>
    /// 从配置表加载技能数据
    /// </summary>
    [ContextMenu("加载技能数据")]
    public void LoadSkillData()
    {
        if (GameExcelConfig.Instance == null)
        {
            Debug.LogError("[SkillTest] GameExcelConfig.Instance 为空，请确保已加载配置");
            return;
        }
        
        _skillData = GameExcelConfig.Instance.skillDatasArray.Find(s => s.ID == skillID);
        if (_skillData != null)
        {
            currentSkillName = _skillData.name;
            Debug.Log($"[SkillTest] 已加载技能: {currentSkillName} (ID: {skillID})");
        }
        else
        {
            Debug.LogError($"[SkillTest] 未找到技能ID: {skillID}");
        }
    }

    /// <summary>
    /// 初始化测试环境
    /// </summary>
    [ContextMenu("初始化测试环境")]
    public void InitializeTestEnvironment()
    {
        // 清理之前的测试对象
        CleanupTestEnvironment();
        
        // 创建施法者
        CreateCaster();
        
        // 创建目标
        if (autoGenerateTargets)
        {
            GenerateTargets();
        }
        
        Debug.Log("[SkillTest] 测试环境已初始化");
    }

    /// <summary>
    /// 释放技能
    /// </summary>
    [ContextMenu("释放技能")]
    public void CastSkill()
    {
        if (_skillData == null)
        {
            LoadSkillData();
            if (_skillData == null)
            {
                Debug.LogError("[SkillTest] 无法释放技能：技能数据未加载");
                return;
            }
        }
        
        if (_testCaster == null)
        {
            InitializeTestEnvironment();
        }
        
        if (targets.Count == 0 && _generatedTargets.Count == 0)
        {
            Debug.LogWarning("[SkillTest] 没有可用的目标");
            return;
        }

        // 应用参数覆盖
        var effectiveSkillData = enableOverride ? CreateOverriddenSkillData() : _skillData;
        
        // 开始技能测试
        StartSkillTest(effectiveSkillData);
    }

    /// <summary>
    /// 停止技能测试
    /// </summary>
    [ContextMenu("停止测试")]
    public void StopSkillTest()
    {
        isTestRunning = false;
        currentPhase = SkillPhase.Idle;
        currentDuration = 0;
        currentEffectCount = 0;
        
        if (_stateMachine != null)
        {
            _stateMachine.Release();
        }
        
        Debug.Log("[SkillTest] 测试已停止");
    }

    /// <summary>
    /// 清理测试环境
    /// </summary>
    [ContextMenu("清理测试环境")]
    public void CleanupTestEnvironment()
    {
        StopSkillTest();
        
        // 清理生成的目标
        foreach (var target in _generatedTargets)
        {
            if (target != null)
            {
                if (Application.isPlaying)
                    Destroy(target);
                else
                    DestroyImmediate(target);
            }
        }
        _generatedTargets.Clear();
        
        // 清理施法者
        if (_testCaster != null)
        {
            if (Application.isPlaying)
                Destroy(_testCaster.gameObject);
            else
                DestroyImmediate(_testCaster.gameObject);
            _testCaster = null;
        }
        
        Debug.Log("[SkillTest] 测试环境已清理");
    }

    /// <summary>
    /// 重置目标状态
    /// </summary>
    [ContextMenu("重置目标")]
    public void ResetTargets()
    {
        foreach (var target in targets)
        {
            if (target != null)
            {
                target.ResetState();
            }
        }
        
        foreach (var go in _generatedTargets)
        {
            var target = go.GetComponent<SkillTestTarget>();
            if (target != null)
            {
                target.ResetState();
            }
        }
        
        Debug.Log("[SkillTest] 目标状态已重置");
    }

    private void CreateCaster()
    {
        var casterGO = new GameObject("SkillTestCaster");
        casterGO.transform.position = casterSpawnPoint != null 
            ? casterSpawnPoint.position 
            : transform.position;
        casterGO.transform.parent = transform;
        
        _testCaster = casterGO.AddComponent<SkillTestCaster>();
        _testCaster.Initialize(this);
    }

    private void GenerateTargets()
    {
        for (int i = 0; i < autoTargetCount; i++)
        {
            float angle = (360f / autoTargetCount) * i;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * targetSpawnRadius;
            Vector3 position = transform.position + offset;
            
            var targetGO = new GameObject($"TestTarget_{i}");
            targetGO.transform.position = position;
            targetGO.transform.parent = transform;
            
            // 添加碰撞器（技能检测需要）
            var collider = targetGO.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = Vector3.up;
            
            // 添加可视化
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.parent = targetGO.transform;
            visual.transform.localPosition = Vector3.up;
            visual.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            // 设置为敌人标签
            targetGO.tag = "Enemy";
            
            var target = targetGO.AddComponent<SkillTestTarget>();
            target.Initialize();
            targets.Add(target);
            
            _generatedTargets.Add(targetGO);
        }
    }

    private SkillData CreateOverriddenSkillData()
    {
        // 注意：由于SkillData的字段是私有的且只有getter，
        // 在实际使用时需要通过反射或其他方式来覆盖参数
        // 这里返回原始数据，实际覆盖在运行时处理
        return _skillData;
    }

    private void StartSkillTest(SkillData skillData)
    {
        if (_testCaster == null)
        {
            Debug.LogError("[SkillTest] 施法者未初始化");
            return;
        }
        
        isTestRunning = true;
        currentDuration = 0;
        currentEffectCount = 0;
        
        _testCaster.CastSkill(skillData, GetFirstValidTarget());
        
        Debug.Log($"[SkillTest] 开始释放技能: {skillData.name}");
    }

    private void UpdateSkillTest()
    {
        if (_testCaster == null) return;
        
        currentDuration += Time.deltaTime;
        currentPhase = _testCaster.CurrentPhase;
        currentEffectCount = _testCaster.CurrentEffectCount;
        
        _testCaster.UpdateSkill();
        
        if (currentPhase == SkillPhase.Finished || currentPhase == SkillPhase.Failed)
        {
            OnSkillTestComplete();
        }
    }

    private void OnSkillTestComplete()
    {
        isTestRunning = false;
        
        if (showDebugInfo)
        {
            Debug.Log($"[SkillTest] 技能测试完成 - 阶段: {currentPhase}, 持续时间: {currentDuration:F2}s, 生效次数: {currentEffectCount}");
        }
    }

    private SkillTestTarget GetFirstValidTarget()
    {
        foreach (var target in targets)
        {
            if (target != null && target.IsAlive)
                return target;
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // 绘制施法者位置
        Vector3 casterPos = casterSpawnPoint != null ? casterSpawnPoint.position : transform.position;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(casterPos, 0.5f);
        
        // 绘制目标生成范围
        if (autoGenerateTargets)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, targetSpawnRadius);
        }
        
        // 绘制技能射程
        if (_skillData != null || overrideDistance > 0)
        {
            int distance = enableOverride ? overrideDistance : (_skillData?.distance ?? 10);
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(casterPos, distance);
        }
    }

    private void OnValidate()
    {
        // 编辑器中修改技能ID时自动加载
        if (autoLoadFromConfig && Application.isPlaying)
        {
            LoadSkillData();
        }
    }
}
