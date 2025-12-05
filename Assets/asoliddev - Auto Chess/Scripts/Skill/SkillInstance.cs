using UnityEngine;

public class SkillInstance : MonoBehaviour
{
    [SerializeField]
    private SkillHelper.MoveLogic moveLogic;
    [SerializeField]
    private SkillHelper.DamageLogic damageLogic;
    [SerializeField]
    private Transform self;
    [SerializeField] 
    private Transform scalePart;
    [SerializeField] 
    [Header("发射出去类弹道, 这类技能弹道的最大飞行(移动时间)")]
    private float maxDuration = 5f;
    [SerializeField] 
    private bool haveDurationPath;
    
    private SkillExeContext _skillExeContext;
    public SkillExeContext SkillExeContext => _skillExeContext;
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
        if(scalePart == null)
            scalePart = transform;
        this.self = self;
        this._skillExeContext = skillExeContext;
        haveDurationPath = this.SkillMove(_skillExeContext.LogicData.MoveLogic, self, target, out _isPathMove);
        _skillExeContext.SetCanFinish(!haveDurationPath);
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

        if (haveDurationPath)
        {
            if (_duration >= maxDuration)
            {
                _skillExeContext.SetCanFinish(true);
            }
        }
    }
    
    /// <summary>
    /// 碰撞检测开始
    /// </summary>
    /// <param name="hit">碰撞对象</param>
    protected virtual void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "SkillEffectCol")
            return;
        // //如果碰撞到护盾，调用 OnCollideShieldBegin(hit) 并中止技能
        // if (hit.tag == "Shield")
        // {
        //     InterceptShieldEffect shieldEffect = hit.GetComponentInParent<InterceptShieldEffect>();
        //     if (shieldEffect.skill.owner.team != skill.owner.team)
        //     {
        //         OnCollideShieldBegin(hit);
        //         return;
        //     }
        // }
        // ChampionController c = hit.gameObject.GetComponentInParent<ChampionController>();
        // if (c == null)
        //     return;
        // //如果碰撞到敌人/队友，则调用 OnCollideChampionBegin(c, pos)
        // if (skill.skillTargetType == SkillTargetType.Teammate)
        // {
        //     if (c.team == skill.owner.team)
        //     {
        //         OnCollideChampionBegin(c, hit.bounds.ClosestPoint(transform.position));
        //     }
        // }
        // else if (skill.skillTargetType == SkillTargetType.Enemy)
        // {
        //     if (c.team != skill.owner.team)
        //     {
        //         OnCollideChampionBegin(c, hit.bounds.ClosestPoint(transform.position));
        //     }
        // }
    }

    protected virtual void OnTriggerStay(Collider hit)
    {
        
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
        //如果碰撞离开敌人/队友，则调用 OnCollideChampionEnd(c, pos)
        // if (skill.skillTargetType == SkillTargetType.Teammate)
        // {
        //     if (c.team == skill.owner.team)
        //     {
        //         OnCollideChampionEnd(c, hit.bounds.ClosestPoint(transform.position));
        //     }
        // }
        // else if (skill.skillTargetType == SkillTargetType.Enemy)
        // {
        //     if (c.team != skill.owner.team)
        //     {
        //         OnCollideChampionEnd(c, hit.bounds.ClosestPoint(transform.position));
        //     }
        // }
    }

    public void DestroySelf()
    {
        Destroy(transform.gameObject);
    }
    
    
    #region 移动时使用的参数
    public bool MoveParam1; 
    #endregion
}
