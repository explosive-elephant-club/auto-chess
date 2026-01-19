using System.Collections.Generic;
using UnityEngine;

public class SkillInstance : MonoBehaviour
{
    [SerializeField]
    private Transform self;
    [SerializeField] 
    private Transform scalePart;
    [SerializeField] 
    [Header("发射出去类弹道, 这类技能弹道的最大飞行(移动时间)")]
    private float maxDuration = 5f;
    [SerializeField] 
    private bool haveDurationPath;
    
    private ISkillExecutionContext _executionContext;
    public ISkillExecutionContext SkillExeContext => _executionContext;

    /// <summary>
    /// 新的移动上下文，替代 MoveParam1/MoveParam2
    /// </summary>
    private SkillMoveContext _moveContext;
    public SkillMoveContext MoveContext => _moveContext;

    private bool _isDamageDestroySkill;
    
    public void Init(ISkillExecutionContext executionContext, Transform self, Transform target)
    {
        var collider = gameObject.GetComponent<Collider>();
        if(collider == null)
        {
            Debug.LogError($"{gameObject.name} has no collider!");
            return;
        }
        if(scalePart == null)
            scalePart = transform;
        this.self = self;
        this._executionContext = executionContext;
        _lastDamageTime ??= new();
        _lastDamageTime.Clear();

        // 初始化移动上下文
        _moveContext ??= new SkillMoveContext();
        _moveContext.Initialize(transform, self, target, executionContext.GetMoveSpeed());

        _isDamageDestroySkill = SkillHelper.CheckIsDamageDestroySkill(executionContext.AttackType);
        this.SkillMove(_executionContext.LogicData.MoveLogic, self, target, out _isPathMove);
        
        // 同步路径移动标记
        _moveContext.IsPathMove = _isPathMove;
        
        if(_isDamageDestroySkill)
            _executionContext.SetCanFinish(false);
    }
    
    /// <summary>
    /// 如果是路径类的移动,比如火箭弹,需要用dotween做路径动画,即不用每次tick更新移动位置,而是用路径动画来驱动
    /// </summary>
    private bool _isPathMove;
    private float _duration;
    /// <summary>
    /// 做非匀速运动时，可以用这个时间来做参考
    /// </summary>
    public float Duration => _duration;
    
    public void UpDateSkill()
    {
        _duration += Time.deltaTime;
        
        // 同步移动上下文的持续时间
        if (_moveContext != null)
        {
            _moveContext.UpdateDuration(Time.deltaTime);
        }
        
        if (!_isPathMove)
        {
            this.SkillMove(_executionContext.LogicData.MoveLogic, self, _executionContext.ReGetTarget, out _isPathMove);
            
            // 同步路径移动标记
            if (_moveContext != null)
            {
                _moveContext.IsPathMove = _isPathMove;
            }
        }

        if (haveDurationPath)
        {
            _executionContext.SetCanFinish(true);
        }
    }

    #region 伤害相关
    private Dictionary<int, float> _lastDamageTime;
    public float GetDamageTimeInterval(int instanceId)
    {
        return _lastDamageTime.GetValueOrDefault(instanceId, -1) - _duration;
    }
    public void SetLastDamageTime(int instanceId)
    {
        _lastDamageTime[instanceId] = _duration;
    }
    /// <summary>
    /// 碰撞检测开始
    /// </summary>
    /// <param name="hit">碰撞对象</param>
    protected virtual void OnTriggerEnter(Collider hit)
    {
        if (hit.CompareTag("SkillEffectCol"))
            return;
        this.SkillDamage(_executionContext.LogicData.DamageLogic, self, hit, SkillDamageMethods.ColliderType.Enter);
    }

    protected virtual void OnTriggerStay(Collider hit)
    {
        var id = hit.GetHashCode();
        if (!_lastDamageTime.ContainsKey(id))
        {
            
        }
        this.SkillDamage(_executionContext.LogicData.DamageLogic, self, hit, SkillDamageMethods.ColliderType.Stay);
    }
    
    /// <summary>
    /// 碰撞检测结束
    /// </summary>
    /// <param name="hit">碰撞对象</param>
    protected virtual void OnTriggerExit(Collider hit)
    {
        ChampionController c = hit.gameObject.GetComponentInParent<ChampionController>();
        if (c == null)
            return;
        this.SkillDamage(_executionContext.LogicData.DamageLogic, self, hit, SkillDamageMethods.ColliderType.Exit);
    }
    #endregion

    public void DestroySelf()
    {
        if(_lastDamageTime  != null)
            _lastDamageTime.Clear();
        Destroy(transform.gameObject);
    }
    
    
    #region 移动时使用的参数
    /// <summary>
    /// 旧的移动参数1（保留用于向后兼容）
    /// 新代码应使用 MoveContext.SweepDirectionPositive 等有语义的属性
    /// </summary>
    [HideInInspector]
    public bool MoveParam1
    {
        get => _moveContext?.SweepDirectionPositive ?? false;
        set
        {
            if (_moveContext != null)
                _moveContext.SweepDirectionPositive = value;
        }
    }
    
    /// <summary>
    /// 旧的移动参数2（保留用于向后兼容）
    /// 新代码应使用 MoveContext.HasLanded 等有语义的属性
    /// </summary>
    [HideInInspector]
    public bool MoveParam2
    {
        get => _moveContext?.HasLanded ?? false;
        set
        {
            if (_moveContext != null)
                _moveContext.HasLanded = value;
        }
    }
    #endregion
}
