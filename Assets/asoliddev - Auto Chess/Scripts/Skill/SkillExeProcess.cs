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
    private Stack<SingleSkillExeProcess> _skillPool = new();
    public bool GlobalProcessIsUsing;
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
        if (skill.CheckIsSellSkill())
        {
            _globalProcess.InitSkill(skill, true);
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

    public void Reset()
    {
        _sellSkillReset?.Invoke();
        _globalProcess.Reset();
    }
}

public class SingleSkillExeProcess
{
    private readonly SkillExeProcess _skillExeProcess;
    private bool _isSellSkill;
    private ISkillState _skill;
    private readonly SkillRuntime _runtime;
    private readonly SkillStateMachine _stateMachine;
    private ProcessExeState _state;
    private bool _isInSellCast;
    
    public SingleSkillExeProcess(SkillExeProcess skillExeProcess)
    {
        _skillExeProcess = skillExeProcess;
        _runtime = new SkillRuntime();
        _stateMachine = new SkillStateMachine(_runtime);
    }

    private bool _isGlobalProcess;
    public void InitSkill(ISkillState skill, bool isGlobalProcess = false)
    {
        _skill = skill;
        this._isGlobalProcess = isGlobalProcess;
        _skill.ResetSkillContext();
        _stateMachine.Initialize(_skill);
        _isInSellCast = false;
        _state = _skill.HaveTargetInRange() ? ProcessExeState.Casting : ProcessExeState.FindTarget;
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
                    _state = ProcessExeState.Casting;
                }
                break;
            case ProcessExeState.Casting:
                var castResult = _stateMachine.Tick();
                if (castResult == SkillExeResult.Ing)
                {
                    _state = ProcessExeState.Ing;
                    if (_skill.CheckIsSellSkill())
                    {
                        _skillExeProcess.SetToSellCast();
                        _skillExeProcess.RegisterSellSkill(Reset, InvokeSkill);
                        _isInSellCast = true;
                    }
                }
                else if (castResult == SkillExeResult.Fail)
                {
                    OnSkillEnd();
                }
                break;
            case ProcessExeState.Ing:
                var result = _stateMachine.Tick();
                _state = result is SkillExeResult.Done or SkillExeResult.Fail ? ProcessExeState.Done : ProcessExeState.Ing;
                if (_state == ProcessExeState.Done)
                {
                    OnSkillEnd();
                }
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
            _skillExeProcess.UnRegisterSellSkill(Reset, InvokeSkill);
            _skillExeProcess.ReturnToPool(this);
            _skillExeContext.Release();
        }
        _skillExeProcess.ReturnToPool(this);
        _stateMachine.Release();
    }

    public void Reset()
    {
        OnSkillEnd();
    }
}
