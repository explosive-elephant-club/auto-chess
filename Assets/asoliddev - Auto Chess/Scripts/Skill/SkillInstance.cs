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
    
    private SkillExeContext _skillExeContext;
    public SkillExeContext SkillExeContext => _skillExeContext;

    private bool _isDamageDestroySkill;
    public void Init(SkillExeContext skillExeContext, Transform self, Transform target)
    {
        var collider = gameObject.GetComponent<Collider>();
        var ttt = gameObject.GetComponent<SkillEffect>();
        if (ttt != null) ttt.enabled = false;
        if(collider == null)
        {
            Debug.LogError($"{gameObject.name} has no collider!");
            return;
        }

        _duration = 0;
        if(scalePart == null)
            scalePart = transform;
        this.self = self;
        this._skillExeContext = skillExeContext;
        _lastDamageTime ??= new();
        _lastDamageTime.Clear();

        _isDamageDestroySkill = SkillHelper.CheckIsDamageDestroySkill(skillExeContext.AttackType);
        this.SkillMove(_skillExeContext.LogicData.MoveLogic, self, target, out _isPathMove);
        
        if(_isDamageDestroySkill)
            _skillExeContext.SetCanFinish(false);
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
        if (!_isPathMove)
        {
            this.SkillMove(_skillExeContext.LogicData.MoveLogic, self, _skillExeContext.ReGetTarget, out _isPathMove);
        }

        if (_isDamageDestroySkill && _duration >= maxDuration)
        {
            _skillExeContext.SetCanFinish(true);
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
        // if (hit.CompareTag("floor"))
            // MoveParam2 = true;
        this.SkillDamage(_skillExeContext.LogicData.DamageLogic, self, hit, SkillDamageMethods.ColliderType.Enter);
    }

    protected virtual void OnTriggerStay(Collider hit)
    {
        var id = hit.GetHashCode();
        if (!_lastDamageTime.ContainsKey(id))
        {
            
        }
        this.SkillDamage(_skillExeContext.LogicData.DamageLogic, self, hit, SkillDamageMethods.ColliderType.Stay);
    }
    
    /// <summary>
    /// 碰撞检测结束
    /// </summary>
    /// <param name="hit">碰撞对象</param>
    protected virtual void OnTriggerExit(Collider hit)
    {
        // if (hit.CompareTag("floor"))
            // MoveParam2 = false;
        ChampionController c = hit.gameObject.GetComponentInParent<ChampionController>();
        if (c == null)
            return;
        this.SkillDamage(_skillExeContext.LogicData.DamageLogic, self, hit, SkillDamageMethods.ColliderType.Exit);
    }
    #endregion

    public void DestroySelf()
    {
        if(_lastDamageTime  != null)
            _lastDamageTime.Clear();
        Destroy(transform.gameObject);
    }
    
    
    #region 移动时使用的参数
    [HideInInspector]
    public bool MoveParam1;
    [HideInInspector]
    public bool MoveParam2;
    #endregion
}
