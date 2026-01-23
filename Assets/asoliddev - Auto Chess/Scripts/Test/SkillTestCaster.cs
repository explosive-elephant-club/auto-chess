using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;

/// <summary>
/// 技能测试专用施法者
/// 采用轻量级方式直接测试技能效果，绕过复杂的游戏逻辑依赖
/// </summary>
public class SkillTestCaster : MonoBehaviour
{
    [Header("=== 施法者属性 ===")]
    public float attackPower = 100f;
    public float critChance = 0.1f;
    public float critMultiplier = 2f;
    public float mana = 100f;
    public float maxMana = 100f;
    
    [Header("=== 状态（只读）===")]
    [SerializeField] private SkillPhase _currentPhase = SkillPhase.Idle;
    [SerializeField] private int _currentEffectCount = 0;
    
    public SkillPhase CurrentPhase => _currentPhase;
    public int CurrentEffectCount => _currentEffectCount;
    
    // 模拟ChampionController需要的属性
    public ChampionTeam team = ChampionTeam.Player;
    
    // 技能执行相关
    private SkillTestController _testController;
    private SkillData _currentSkillData;
    private SkillTestTarget _currentTarget;
    
    // 技能效果相关
    private SkillEffectFactory _effectFactory;
    private List<SkillInstance> _activeInstances = new();
    private float _castDuration;
    private float _intervalTimer;
    private int _effectCountTarget;
    
    // 发射点
    public Transform[] skillCastPoints;
    private int _currentCastPointIndex;
    
    public void Initialize(SkillTestController controller)
    {
        _testController = controller;
        _effectFactory = new SkillEffectFactory();
        
        // 创建默认发射点
        var castPoint = new GameObject("CastPoint");
        castPoint.transform.parent = transform;
        castPoint.transform.localPosition = Vector3.forward * 0.5f + Vector3.up;
        skillCastPoints = new Transform[] { castPoint.transform };
    }

    public void CastSkill(SkillData skillData, SkillTestTarget target)
    {
        _currentSkillData = skillData;
        _currentTarget = target;
        _currentPhase = SkillPhase.Idle;
        _currentEffectCount = 0;
        _castDuration = 0;
        _intervalTimer = 0;
        _currentCastPointIndex = 0;
        _effectCountTarget = skillData.effectCounts;
        
        // 清理之前的技能实例
        ClearInstances();
        
        // 转向目标
        if (target != null)
        {
            Vector3 dir = target.transform.position - transform.position;
            dir.y = 0;
            if (dir.magnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
        
        _currentPhase = SkillPhase.Casting;
        
        // 立即执行第一次效果
        ExecuteEffect();
        
        Debug.Log($"[SkillTestCaster] 开始释放技能: {skillData.name}, 生效次数目标: {_effectCountTarget}");
    }

    public void UpdateSkill()
    {
        if (_currentPhase == SkillPhase.Idle || _currentSkillData == null) return;
        
        _castDuration += Time.deltaTime;
        
        // 更新所有活动的技能实例
        UpdateInstances();
        
        switch (_currentPhase)
        {
            case SkillPhase.Casting:
                HandleCasting();
                break;
                
            case SkillPhase.Executing:
                HandleExecuting();
                break;
        }
    }

    private void HandleCasting()
    {
        // 检查是否需要生成更多效果
        if (_currentEffectCount < _effectCountTarget)
        {
            float intervalTime = _currentSkillData.duration / Mathf.Max(1, _effectCountTarget);
            _intervalTimer += Time.deltaTime;
            
            if (_intervalTimer >= intervalTime)
            {
                _intervalTimer = 0;
                ExecuteEffect();
            }
        }
        
        // 所有效果已发出，进入执行阶段
        if (_currentEffectCount >= _effectCountTarget)
        {
            _currentPhase = SkillPhase.Executing;
            Debug.Log($"[SkillTestCaster] 所有效果已发出，进入执行阶段");
        }
    }

    private void HandleExecuting()
    {
        // 检查技能是否应该结束
        float totalDuration = _currentSkillData.duration + _currentSkillData.delay;
        
        // 如果所有实例都已销毁或超时，结束技能
        bool allInstancesDestroyed = _activeInstances.Count == 0 || _activeInstances.TrueForAll(i => i == null);
        bool timeExpired = _castDuration >= totalDuration + 5f; // 额外5秒超时保护
        
        if (allInstancesDestroyed || timeExpired)
        {
            _currentPhase = SkillPhase.Finished;
            Debug.Log($"[SkillTestCaster] 技能执行完成，持续时间: {_castDuration:F2}s");
        }
    }

    private void ExecuteEffect()
    {
        if (_currentSkillData == null) return;
        
        if (_currentSkillData.isDirectEffect)
        {
            ExecuteDirectEffect();
        }
        else
        {
            ExecuteProjectileEffect();
        }
        
        _currentEffectCount++;
        Debug.Log($"[SkillTestCaster] 执行效果 #{_currentEffectCount}");
    }

    private void ExecuteDirectEffect()
    {
        // 直接效果：对目标造成伤害
        if (_currentTarget != null && _currentSkillData.damageData != null)
        {
            _currentTarget.TakeDamage(_currentSkillData.damageData);
        }
        
        // 生成命中特效（如果有）
        Transform castPoint = GetCurrentCastPoint();
        _effectFactory.CreateEmitEffect(_currentSkillData.ID, castPoint.position, castPoint.rotation, SkillConstants.DEFAULT_EMIT_EFFECT_DURATION);
        
        MoveToNextCastPoint();
    }

    private void ExecuteProjectileEffect()
    {
        if (_currentTarget == null) return;
        
        Transform castPoint = GetCurrentCastPoint();
        
        // 创建投射物
        var createContext = new EffectCreateContext
        {
            SkillData = _currentSkillData,
            Owner = null, // 测试模式下没有真正的Owner
            Target = null, // 测试模式下没有真正的Target ChampionController
            CastPoint = castPoint,
            UseOwnerPosition = false,
            ExecutionContext = new TestExecutionContext(_currentSkillData, _currentTarget, team)
        };

        var skillInstance = _effectFactory.CreateProjectile(createContext);
        if (skillInstance != null)
        {
            // 使用测试专用的执行上下文初始化
            var testContext = new TestExecutionContext(_currentSkillData, _currentTarget, team);
            skillInstance.Init(testContext, transform, _currentTarget.transform);
            _activeInstances.Add(skillInstance);
            Debug.Log($"[SkillTestCaster] 创建投射物实例");
        }
        else
        {
            Debug.LogWarning($"[SkillTestCaster] 未能创建投射物，请检查VFX预制体路径: {SkillHelper.GetVFXPath(_currentSkillData.ID)}");
        }

        MoveToNextCastPoint();
    }

    private void UpdateInstances()
    {
        for (int i = _activeInstances.Count - 1; i >= 0; i--)
        {
            if (_activeInstances[i] != null)
            {
                _activeInstances[i].UpDateSkill();
            }
            else
            {
                _activeInstances.RemoveAt(i);
            }
        }
    }

    private void ClearInstances()
    {
        foreach (var instance in _activeInstances)
        {
            if (instance != null)
            {
                instance.DestroySelf();
            }
        }
        _activeInstances.Clear();
    }

    private Transform GetCurrentCastPoint()
    {
        if (skillCastPoints == null || skillCastPoints.Length == 0)
            return transform;
        return skillCastPoints[_currentCastPointIndex];
    }

    private void MoveToNextCastPoint()
    {
        if (skillCastPoints != null && skillCastPoints.Length > 0)
        {
            _currentCastPointIndex = (_currentCastPointIndex + 1) % skillCastPoints.Length;
        }
    }

    public void StopSkill()
    {
        _currentPhase = SkillPhase.Idle;
        ClearInstances();
    }

    private void OnDestroy()
    {
        ClearInstances();
    }

    private void OnDrawGizmos()
    {
        // 绘制施法者
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // 绘制朝向
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * 2);
        
        // 绘制发射点
        if (skillCastPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in skillCastPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.2f);
                }
            }
        }
    }
}

/// <summary>
/// 测试专用的技能执行上下文
/// 提供 SkillInstance 需要的接口，但不依赖真实游戏对象
/// </summary>
public class TestExecutionContext : ISkillExecutionContext
{
    private readonly SkillData _skillData;
    private readonly SkillTestTarget _target;
    private readonly ChampionTeam _team;
    private readonly SkillHelper.SkillLogicData _logicData;
    private readonly SkillHelper.SkillAttackType _attackType;
    private bool _canFinish = false;

    public TestExecutionContext(SkillData skillData, SkillTestTarget target, ChampionTeam team)
    {
        _skillData = skillData;
        _target = target;
        _team = team;
        _attackType = (SkillHelper.SkillAttackType)skillData.AttackType;
        
        if (SkillHelper.AllLogicDataDic.TryGetValue(_attackType, out var logicData))
        {
            _logicData = logicData;
        }
        else
        {
            _logicData = new SkillHelper.SkillLogicData
            {
                MoveLogic = SkillHelper.MoveLogic.MoveForward,
                DamageLogic = SkillHelper.DamageLogic.OnlyCollision
            };
        }
    }

    public SkillHelper.SkillLogicData LogicData => _logicData;
    public SkillHelper.SkillAttackType AttackType => _attackType;
    public SkillTargetType SkillTargetType => SkillTargetType.Enemy;
    public ChampionTeam Team => _team;

    public float GetMoveSpeed() => _skillData.MoveSpeed;

    public Transform ReGetTarget()
    {
        return _target != null && _target.IsAlive ? _target.transform : null;
    }

    public void SetCanFinish(bool canFinish)
    {
        _canFinish = canFinish;
    }

    public void SkillHitEffect(Collider collider, ChampionController target, bool onlyEffect = false)
    {
        // 测试环境下，直接对 SkillTestTarget 造成伤害
        if (_target != null && _skillData.damageData != null && !onlyEffect)
        {
            _target.TakeDamage(_skillData.damageData);
            Debug.Log($"[TestExecutionContext] 技能命中目标，造成伤害");
        }
    }
}
