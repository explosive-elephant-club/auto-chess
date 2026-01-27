using System;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

/// <summary>
/// 技能测试模式
/// </summary>
public enum SkillTestMode
{
    /// <summary>
    /// 单技能测试
    /// </summary>
    SingleSkill,
    /// <summary>
    /// 技能链测试（多技能按顺序执行）
    /// </summary>
    SkillChain
}

/// <summary>
/// 技能链覆盖模式
/// </summary>
public enum SkillChainOverrideMode
{
    /// <summary>
    /// 不覆盖，使用配置表参数
    /// </summary>
    None,
    /// <summary>
    /// 统一覆盖，所有技能使用相同参数
    /// </summary>
    Unified,
    /// <summary>
    /// 单独覆盖，每个技能可单独配置
    /// </summary>
    Individual
}

/// <summary>
/// 技能链中单个技能的覆盖配置
/// </summary>
[System.Serializable]
public class SkillChainOverrideItem
{
    [Tooltip("技能ID")]
    public int skillID;
    
    [Tooltip("技能名称（只读）")]
    public string skillName;
    
    [Tooltip("是否启用此技能的覆盖")]
    public bool enabled = false;
    
    [Tooltip("覆盖参数")]
    public SkillTestOverrides overrides = new();
    
    public SkillChainOverrideItem(int id, string name = "")
    {
        skillID = id;
        skillName = name;
        enabled = false;
        overrides = new SkillTestOverrides();
    }
}

/// <summary>
/// 技能测试控制器
/// 用于在Unity编辑器中快速测试技能效果
/// 支持单技能测试和技能链测试
/// </summary>
public class SkillTestController : MonoBehaviour
{
    [Header("=== 技能模式 ===")]
    [Tooltip("测试模式：单技能或技能链")]
    public SkillTestMode testMode = SkillTestMode.SingleSkill;
    
    [Header("=== 单技能配置 ===")]
    [Tooltip("要测试的技能ID")]
    public int skillID = 1;
    
    [Tooltip("自动从配置表加载技能数据")]
    public bool autoLoadFromConfig = true;
    
    [Header("=== 技能链配置 ===")]
    [Tooltip("技能链ID列表（按顺序执行）")]
    public List<int> skillChainIDs = new();
    
    [Tooltip("技能链循环次数（0=无限循环）")]
    public int chainLoopCount = 1;
    
    [Tooltip("技能链内技能间隔时间")]
    public float skillInterval = 0.5f;
    
    [Tooltip("技能链循环间隔时间（一轮结束后的等待时间）")]
    public float chainLoopInterval = 2f;
    
    [Header("=== 技能参数覆盖 ===")]
    [Tooltip("是否启用参数覆盖（用于测试不同数值）")]
    public bool enableOverride = false;
    
    [Tooltip("技能链覆盖模式：统一覆盖或单独覆盖")]
    public SkillChainOverrideMode chainOverrideMode = SkillChainOverrideMode.None;
    
    [Tooltip("技能链单独覆盖配置列表")]
    public List<SkillChainOverrideItem> skillChainOverrides = new();
    
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
    
    [Header("=== 技能链状态（只读）===")]
    [SerializeField] private int currentChainIndex = 0;
    [SerializeField] private int currentLoopCount = 0;
    [SerializeField] private string chainStatusInfo = "";
    [SerializeField] private bool chainCastingComplete = false;
    
    // 运行时引用
    private SkillData _skillData;
    private List<SkillData> _skillChainData = new();
    private float _loopTimer;
    private float _skillIntervalTimer;
    private bool _waitingForNextSkill;
    private bool _waitingForChainLoop;
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
            if (testMode == SkillTestMode.SingleSkill)
            {
                UpdateSkillTest();
            }
            else
            {
                UpdateSkillChainTest();
            }
        }
        
        // 循环测试（仅单技能模式）
        if (testMode == SkillTestMode.SingleSkill && loopInterval > 0 && !isTestRunning)
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
    /// 更新技能链测试
    /// </summary>
    private void UpdateSkillChainTest()
    {
        if (_testCaster == null) return;
        
        // 更新脱手技能实例（让它们在后台继续运行）
        _testCaster.UpdateSellSkillInstances();
        
        // 技能链释放已完成，只需继续更新脱手技能实例
        if (chainCastingComplete)
        {
            // 检查是否还有脱手技能在运行
            int sellSkillCount = _testCaster.GetSellSkillInstanceCount();
            if (sellSkillCount > 0)
            {
                chainStatusInfo = $"技能链释放完成，{sellSkillCount}个脱手技能运行中...";
            }
            else
            {
                chainStatusInfo = $"技能链测试完成（共 {currentLoopCount} 轮）";
            }
            return;
        }
        
        // 等待技能链循环间隔
        if (_waitingForChainLoop)
        {
            _skillIntervalTimer += Time.deltaTime;
            if (_skillIntervalTimer >= chainLoopInterval)
            {
                _waitingForChainLoop = false;
                _skillIntervalTimer = 0;
                StartSkillChainTest();
            }
            return;
        }
        
        // 等待技能间隔
        if (_waitingForNextSkill)
        {
            _skillIntervalTimer += Time.deltaTime;
            if (_skillIntervalTimer >= skillInterval)
            {
                _waitingForNextSkill = false;
                _skillIntervalTimer = 0;
                StartSkillChainTest();
            }
            return;
        }
        
        // 更新当前技能
        currentDuration += Time.deltaTime;
        currentPhase = _testCaster.CurrentPhase;
        currentEffectCount = _testCaster.CurrentEffectCount;
        
        _testCaster.UpdateSkill();
        
        // 检查当前技能是否完成
        // 对于脱手技能：进入Executing阶段且已释放后，立即进入下一个技能
        bool skillComplete = currentPhase == SkillPhase.Finished || currentPhase == SkillPhase.Failed;
        bool sellSkillReleased = _testCaster.IsSellSkill && _testCaster.SellSkillReleased && currentPhase == SkillPhase.Executing;
        
        if (skillComplete || sellSkillReleased)
        {
            OnChainSkillComplete();
        }
    }
    
    /// <summary>
    /// 技能链中单个技能完成
    /// </summary>
    private void OnChainSkillComplete()
    {
        if (showDebugInfo)
        {
            var skillName = currentChainIndex < _skillChainData.Count ? _skillChainData[currentChainIndex].name : "未知";
            Debug.Log($"[SkillTest] 技能链 - 技能 [{currentChainIndex + 1}] {skillName} 完成，持续时间: {currentDuration:F2}s");
        }
        
        // 移动到下一个技能
        currentChainIndex++;
        currentDuration = 0;
        currentEffectCount = 0;
        
        if (currentChainIndex < _skillChainData.Count)
        {
            // 等待技能间隔后释放下一个技能
            _waitingForNextSkill = true;
            _skillIntervalTimer = 0;
            chainStatusInfo = $"等待下一个技能... ({skillInterval}s)";
        }
        else
        {
            OnSkillChainLoopComplete();
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
        
        if (testMode == SkillTestMode.SingleSkill)
        {
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
        else
        {
            LoadSkillChainData();
        }
    }
    
    /// <summary>
    /// 加载技能链数据
    /// </summary>
    [ContextMenu("加载技能链数据")]
    public void LoadSkillChainData()
    {
        if (GameExcelConfig.Instance == null)
        {
            Debug.LogError("[SkillTest] GameExcelConfig.Instance 为空，请确保已加载配置");
            return;
        }
        
        _skillChainData.Clear();
        
        if (skillChainIDs.Count == 0)
        {
            Debug.LogWarning("[SkillTest] 技能链ID列表为空");
            return;
        }
        
        foreach (var id in skillChainIDs)
        {
            var data = GameExcelConfig.Instance.skillDatasArray.Find(s => s.ID == id);
            if (data != null)
            {
                _skillChainData.Add(data);
                Debug.Log($"[SkillTest] 已加载技能链技能: {data.name} (ID: {id})");
            }
            else
            {
                Debug.LogWarning($"[SkillTest] 技能链中未找到技能ID: {id}");
            }
        }
        
        if (_skillChainData.Count > 0)
        {
            currentSkillName = $"技能链 [{_skillChainData.Count}个技能]";
            chainStatusInfo = $"已加载 {_skillChainData.Count}/{skillChainIDs.Count} 个技能";
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
        if (testMode == SkillTestMode.SingleSkill)
        {
            CastSingleSkill();
        }
        else
        {
            CastSkillChain();
        }
    }
    
    /// <summary>
    /// 释放单个技能
    /// </summary>
    private void CastSingleSkill()
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
        
        // 开始技能测试（覆盖参数在 StartSkillTest 中处理）
        StartSkillTest(_skillData);
    }
    
    /// <summary>
    /// 释放技能链
    /// </summary>
    [ContextMenu("释放技能链")]
    public void CastSkillChain()
    {
        if (_skillChainData.Count == 0)
        {
            LoadSkillChainData();
            if (_skillChainData.Count == 0)
            {
                Debug.LogError("[SkillTest] 无法释放技能链：技能链数据未加载");
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
        
        // 重置技能链状态（但不清理之前的脱手技能实例）
        currentChainIndex = 0;
        currentLoopCount = 0;
        _waitingForNextSkill = false;
        _waitingForChainLoop = false;
        _skillIntervalTimer = 0;
        chainCastingComplete = false;
        
        // 开始第一个技能
        StartSkillChainTest();
    }
    
    /// <summary>
    /// 开始技能链测试
    /// </summary>
    private void StartSkillChainTest()
    {
        if (currentChainIndex >= _skillChainData.Count)
        {
            OnSkillChainLoopComplete();
            return;
        }
        
        var skillData = _skillChainData[currentChainIndex];
        currentSkillName = $"[{currentChainIndex + 1}/{_skillChainData.Count}] {skillData.name}";
        chainStatusInfo = $"循环 {currentLoopCount + 1}/{(chainLoopCount == 0 ? "∞" : chainLoopCount.ToString())} | 技能 {currentChainIndex + 1}/{_skillChainData.Count}: {skillData.name}";
        
        Debug.Log($"[SkillTest] 技能链 - {chainStatusInfo}");
        StartSkillTest(skillData);
    }
    
    /// <summary>
    /// 技能链单轮完成
    /// </summary>
    private void OnSkillChainLoopComplete()
    {
        currentLoopCount++;
        
        // 检查是否需要继续循环
        if (chainLoopCount == 0 || currentLoopCount < chainLoopCount)
        {
            currentChainIndex = 0;
            _waitingForChainLoop = true;
            _skillIntervalTimer = 0;
            chainStatusInfo = $"等待下一轮... ({chainLoopInterval}s)";
            Debug.Log($"[SkillTest] 技能链第 {currentLoopCount} 轮完成，等待 {chainLoopInterval}s 后开始下一轮");
        }
        else
        {
            chainStatusInfo = $"技能链释放完成（共 {currentLoopCount} 轮），等待效果结束...";
            Debug.Log($"[SkillTest] 技能链释放完成，共执行 {currentLoopCount} 轮，脱手技能继续运行中");
            // 不停止更新，让脱手技能继续运行
            chainCastingComplete = true;
        }
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
        
        // 重置技能链状态
        currentChainIndex = 0;
        currentLoopCount = 0;
        _waitingForNextSkill = false;
        _waitingForChainLoop = false;
        _skillIntervalTimer = 0;
        chainStatusInfo = "";
        chainCastingComplete = false;
        
        if (_testCaster != null)
        {
            _testCaster.StopSkill();
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
        
        // 添加 AudioListener 避免警告日志
        if (FindObjectOfType<AudioListener>() == null)
        {
            casterGO.AddComponent<AudioListener>();
        }
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
            
            // 添加刚体（Trigger检测需要至少一方有Rigidbody）
            var rb = targetGO.AddComponent<Rigidbody>();
            rb.isKinematic = true; // 不受物理影响
            rb.useGravity = false;
            
            // 添加碰撞器（技能检测需要，必须设置为Trigger）
            var collider = targetGO.AddComponent<CapsuleCollider>();
            collider.height = 2f;
            collider.radius = 0.5f;
            collider.center = Vector3.up;
            collider.isTrigger = true;
            
            // 添加可视化
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.parent = targetGO.transform;
            visual.transform.localPosition = Vector3.up;
            visual.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            // 移除可视化对象的碰撞器
            var visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null) DestroyImmediate(visualCollider);
            
            // 设置为敌人标签
            targetGO.tag = "Enemy";
            
            // 添加测试目标组件
            var target = targetGO.AddComponent<SkillTestTarget>();
            target.Initialize();
            targets.Add(target);
            
            // 添加 ChampionController 用于伤害检测（技能系统通过此组件识别目标）
            var champion = targetGO.AddComponent<ChampionController>();
            champion.team = ChampionTeam.Oponent; // 设为敌方
            // 初始化必要的子控制器以避免空引用
            champion.attributesController = new ChampionAttributesController(champion);
            champion.buffController = new BuffController(champion);
            champion.skillController = new SkillController(champion);
            
            _generatedTargets.Add(targetGO);
        }
    }

    private SkillTestOverrides CreateOverrides(int skillID = -1)
    {
        // 单技能模式：使用 enableOverride 控制
        if (testMode == SkillTestMode.SingleSkill)
        {
            if (!enableOverride)
                return null;
                
            return CreateUnifiedOverrides();
        }
        
        // 技能链模式：根据覆盖模式处理
        switch (chainOverrideMode)
        {
            case SkillChainOverrideMode.None:
                return null;
                
            case SkillChainOverrideMode.Unified:
                return CreateUnifiedOverrides();
                
            case SkillChainOverrideMode.Individual:
                return CreateIndividualOverrides(skillID);
                
            default:
                return null;
        }
    }
    
    /// <summary>
    /// 创建统一覆盖参数
    /// </summary>
    private SkillTestOverrides CreateUnifiedOverrides()
    {
        return new SkillTestOverrides
        {
            Duration = overrideDuration,
            EffectCounts = overrideEffectCounts,
            Distance = overrideDistance,
            Range = overrideRange,
            MoveSpeed = overrideMoveSpeed,
            AttackType = overrideAttackType
        };
    }
    
    /// <summary>
    /// 创建单独覆盖参数（根据技能ID查找）
    /// </summary>
    private SkillTestOverrides CreateIndividualOverrides(int skillID)
    {
        var item = skillChainOverrides.Find(x => x.skillID == skillID);
        if (item != null && item.enabled)
        {
            return item.overrides;
        }
        return null;
    }
    
    /// <summary>
    /// 同步技能链覆盖配置列表
    /// 当技能链ID列表变化时调用
    /// </summary>
    public void SyncSkillChainOverrides()
    {
        // 移除不在技能链中的覆盖配置
        skillChainOverrides.RemoveAll(x => !skillChainIDs.Contains(x.skillID));
        
        // 添加新技能的覆盖配置
        foreach (var id in skillChainIDs)
        {
            if (!skillChainOverrides.Exists(x => x.skillID == id))
            {
                string skillName = GetSkillNameByID(id);
                skillChainOverrides.Add(new SkillChainOverrideItem(id, skillName));
            }
        }
        
        // 按技能链顺序排序
        skillChainOverrides.Sort((a, b) => skillChainIDs.IndexOf(a.skillID).CompareTo(skillChainIDs.IndexOf(b.skillID)));
    }
    
    private string GetSkillNameByID(int skillID)
    {
        if (GameExcelConfig.Instance == null) return "未知";
        var skill = GameExcelConfig.Instance.skillDatasArray?.Find(s => s.ID == skillID);
        return skill?.name ?? "未知";
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
        
        // 传递覆盖参数（传入技能ID用于查找单独覆盖配置）
        var overrides = CreateOverrides(skillData.ID);
        _testCaster.CastSkill(skillData, GetFirstValidTarget(), overrides);
        
        if (overrides != null)
        {
            Debug.Log($"[SkillTest] 开始释放技能: {skillData.name} (使用覆盖参数: 持续时间={overrides.Duration}, 生效次数={overrides.EffectCounts}, 攻击类型={overrides.AttackType})");
        }
        else
        {
            Debug.Log($"[SkillTest] 开始释放技能: {skillData.name}");
        }
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
    
    /// <summary>
    /// 获取当前技能链进度信息
    /// </summary>
    public string GetChainProgressInfo()
    {
        if (testMode != SkillTestMode.SkillChain || !isTestRunning)
            return "";
            
        return $"循环: {currentLoopCount + 1}/{(chainLoopCount == 0 ? "∞" : chainLoopCount.ToString())} | " +
               $"技能: {currentChainIndex + 1}/{_skillChainData.Count}";
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 获取当前单技能的覆盖参数（用于保存）
    /// </summary>
    public SkillTestOverrides GetCurrentOverrides()
    {
        if (!enableOverride) return null;
        
        return new SkillTestOverrides
        {
            Duration = overrideDuration,
            EffectCounts = overrideEffectCounts,
            Distance = overrideDistance,
            Range = overrideRange,
            MoveSpeed = overrideMoveSpeed,
            AttackType = overrideAttackType
        };
    }
    
    /// <summary>
    /// 获取当前技能ID
    /// </summary>
    public int GetCurrentSkillID()
    {
        return skillID;
    }
    
    /// <summary>
    /// 获取启用了覆盖的技能链项目列表
    /// </summary>
    public List<(int skillID, SkillTestOverrides overrides)> GetEnabledChainOverrides()
    {
        var result = new List<(int, SkillTestOverrides)>();
        
        foreach (var item in skillChainOverrides)
        {
            if (item.enabled)
            {
                result.Add((item.skillID, item.overrides));
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// 从配置表加载参数到覆盖设置（用于编辑前初始化）
    /// </summary>
    public void LoadOverridesFromConfig()
    {
        if (GameExcelConfig.Instance == null)
        {
            Debug.LogWarning("[SkillTestController] GameExcelConfig 未加载");
            return;
        }
        
        if (testMode == SkillTestMode.SingleSkill)
        {
            var skillData = GameExcelConfig.Instance.skillDatasArray?.Find(s => s.ID == skillID);
            if (skillData != null)
            {
                overrideDuration = skillData.duration;
                overrideEffectCounts = skillData.effectCounts;
                overrideDistance = skillData.distance;
                overrideRange = skillData.range;
                overrideMoveSpeed = skillData.MoveSpeed;
                overrideAttackType = (SkillHelper.SkillAttackType)skillData.AttackType;
                Debug.Log($"[SkillTestController] 已从配置加载技能 [{skillID}] 的参数到覆盖设置");
            }
        }
        else
        {
            foreach (var item in skillChainOverrides)
            {
                var skillData = GameExcelConfig.Instance.skillDatasArray?.Find(s => s.ID == item.skillID);
                if (skillData != null)
                {
                    item.overrides.Duration = skillData.duration;
                    item.overrides.EffectCounts = skillData.effectCounts;
                    item.overrides.Distance = skillData.distance;
                    item.overrides.Range = skillData.range;
                    item.overrides.MoveSpeed = skillData.MoveSpeed;
                    item.overrides.AttackType = (SkillHelper.SkillAttackType)skillData.AttackType;
                }
            }
            Debug.Log($"[SkillTestController] 已从配置加载技能链参数到覆盖设置");
        }
    }
#endif
}
