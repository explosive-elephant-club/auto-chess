using System;
using System.Collections.Generic;
using ExcelConfig;
using UnityEngine;

/// <summary>
/// 技能执行器
/// 把技能的数据和行为解耦
/// 后续需要做的事情：
/// 主执行器作为一个概念，每个技能在释放阶段(寻敌，释放动作)都需要占用，占用时不能直接执行下一个技能
/// 脱手技能实际为持续释放时不占用主执行器，主执行器继续释放下一个技能
/// </summary>
public class SkillExeProcess
{
    private SingleSkillExeProcess _globalProcess;
    private System.Action _sellSkill;
    private System.Action _sellSkillReset;
    private readonly Stack<SingleSkillExeProcess> _skillPool = new();

    public void RegisterSellSkill(System.Action reset, System.Action sellSkill)
    {
        _sellSkill += sellSkill;
        _sellSkillReset += reset;
    }
    
    public void UnRegisterSellSkill(System.Action reset, System.Action sellSkill)
    {
        _sellSkill -= sellSkill;
        _sellSkillReset -= reset;
    }

    public void SetCurSkill(ISkillState skill)
    {
        if(skill == null) return;
        var process = _skillPool.Count > 0 ? _skillPool.Pop() : new SingleSkillExeProcess(this);
        _globalProcess = process;
        _globalProcess.InitSkill(skill);
    }

    public void ReturnToPool(SingleSkillExeProcess process)
    {
        _skillPool.Push(process);
    }

    // 脱手技能施法动作结束, 进入自动释放阶段, 开始进入下一个技能
    public void SetToSellCast()
    {
        _globalProcess = null;
    }
    
    public void ExecuteSkill()
    {
        if(_globalProcess != null)
            _globalProcess.TickSkill();
    }
    public void ExecuteSellSkill()
    {
        _sellSkill?.Invoke();
    }

    public SkillPhase GetCurState()
    {
        return _globalProcess?.GetCurState() ?? SkillPhase.Idle;
    }

    public void Reset()
    {
        _sellSkillReset?.Invoke();
        if(_globalProcess != null)
            _globalProcess.Reset();
        _globalProcess = null;
    }
}

public class SingleSkillExeProcess
{
    private readonly SkillExeProcess _skillExeProcess;
    private ISkillState _skill;
    private readonly SkillRuntime _runtime;
    private readonly SkillStateMachine _stateMachine;
    private SkillPhase _state;
    private bool _isInSellCast;
    
    public SingleSkillExeProcess(SkillExeProcess skillExeProcess)
    {
        _skillExeProcess = skillExeProcess;
        _runtime = new SkillRuntime();
        _stateMachine = new SkillStateMachine(_runtime);
    }

    public void InitSkill(ISkillState skill)
    {
        _skill = skill;
        _skill.ResetSkillContext();
        _stateMachine.Initialize(_skill);
        _isInSellCast = false;
        _state = _skill.HaveTargetInRange() ? SkillPhase.Casting : SkillPhase.FindingTarget;
    }

    private void InvokeSkill()
    {
        TickSkill();
    }

    public SkillPhase TickSkill()
    {
        switch (_state)
        {
            case SkillPhase.FindingTarget:
                if (_skill.HaveTargetInRange())
                {
                    _state = SkillPhase.Casting;
                }
                break;
            case SkillPhase.Casting:
                var castResult = _stateMachine.Tick();
                if (castResult == SkillPhase.Executing)
                {
                    _state = SkillPhase.Executing;
                    if (_skill.CheckIsSellSkill())
                    {
                        _skillExeProcess.SetToSellCast();
                        _skillExeProcess.RegisterSellSkill(Reset, InvokeSkill);
                        _isInSellCast = true;
                    }
                }
                else if (castResult == SkillPhase.Failed)
                {
                    OnSkillEnd();
                }
                break;
            case SkillPhase.Executing:
                var result = _stateMachine.Tick();
                _state = result is SkillPhase.Finished or SkillPhase.Failed ? SkillPhase.Finished : SkillPhase.Executing;
                if (_state == SkillPhase.Finished)
                {
                    OnSkillEnd();
                }
                break;
        }
        return _state;
    }

    public SkillPhase GetCurState()
    {
        return _state;
    }

    private void OnSkillEnd()
    {
        if (_isInSellCast)
        {
            _skillExeProcess.UnRegisterSellSkill(Reset, InvokeSkill);
        }
        _skillExeProcess.ReturnToPool(this);
        _stateMachine.Release();
    }

    public void Reset()
    {
        OnSkillEnd();
    }
}
