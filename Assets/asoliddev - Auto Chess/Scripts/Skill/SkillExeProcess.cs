using System.Collections.Generic;

/// <summary>
/// 技能执行器
/// 把技能的数据和行为解耦
/// </summary>
public class SkillExeProcess
{
    private SingleSkillExeProcess _globalProcess;
    private System.Action _sellSkill;
    private Stack<SingleSkillExeProcess> _skillPool = new();
    public SkillExeProcess()
    {
        InitProcess();
    }
    /// <summary>
    /// 初始化执行器
    /// </summary>
    private void InitProcess()
    {
        _globalProcess = new(this);
    }

    public void RegisterSellSkill(System.Action sellSkill)
    {
        _sellSkill += sellSkill;
    }
    
    public void UnRegisterSellSkill(System.Action sellSkill)
    {
        _sellSkill -= sellSkill;
    }

    public void SetCurSkill(ISkillState skill)
    {
        if(skill == null) return;
        if (skill.CheckIsSellSkill())
        {
            _globalProcess.InitSkill(skill);
        }
        else
        {
            if(_skillPool.Count > 0)
            {
                var process = _skillPool.Pop();
                process.InitSkill(skill);
            }
            else
            {
                new SingleSkillExeProcess(this, true).InitSkill(skill);
            }
        }
    }

    public void ReturnToPool(SingleSkillExeProcess process)
    {
        _skillPool.Push(process);
    }
    
    public void ExecuteSkill()
    {
        _globalProcess.TickSkill();
    }
    public void ExecuteSellSkill()
    {
        _sellSkill?.Invoke();
    }

    public ProcessExeState GetCurState()
    {
        return _globalProcess.GetCurState();
    }
}

public class SingleSkillExeProcess
{
    private readonly SkillExeProcess _skillExeProcess;
    private bool _isSellSkill;
    private ISkillState _skill;
    
    private ProcessExeState _state;
    public SingleSkillExeProcess(SkillExeProcess skillExeProcess, bool isSellSkill = false)
    {
        _skillExeProcess = skillExeProcess;
        _isSellSkill = isSellSkill;
    }

    public void InitSkill(ISkillState skill)
    {
        _skill = skill;
        _skill.ResetSkillContext();
        if(_isSellSkill)
            _skillExeProcess.RegisterSellSkill(InvokeSkill);
        _state = _skill.HaveTargetInRange() ? ProcessExeState.Ing : ProcessExeState.FindTarget;

    }

    private void InvokeSkill()
    {
        TickSkill();
    }

    public ProcessExeState TickSkill()
    {
        switch (_state)
        {
            case ProcessExeState.FindTarget:
                if (_skill.HaveTargetInRange())
                {
                    _state = ProcessExeState.Ing;
                }
                break;
            case ProcessExeState.Ing:
                var state = _skill?.ExeState();
                _state = state is SkillExeResult.Done or SkillExeResult.Fail ? ProcessExeState.Done : ProcessExeState.Ing;
                break;
            case ProcessExeState.Done:
                break;
        }
        return _state;
    }

    public ProcessExeState GetCurState()
    {
        return _state;
    }

    private void OnSkillEnd()
    {
        if (_isSellSkill)
        {
            _state = ProcessExeState.None;
            _skillExeProcess.UnRegisterSellSkill(InvokeSkill);
            _skillExeProcess.ReturnToPool(this);
        }
    }
}
