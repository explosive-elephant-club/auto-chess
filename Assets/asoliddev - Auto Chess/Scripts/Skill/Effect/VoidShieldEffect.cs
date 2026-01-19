using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExcelConfig;
using System;

public class VoidShieldEffect : InterceptShieldEffect
{
    public override void Init(Skill _skill, Transform _target)
    {
        if (_skill.skillController.curVoidShieldEffect != null)
        {

            _skill.skillController.curVoidShieldEffect.BrokenDestroy();
        }
        base.Init(_skill, _target);
        skill.skillController.curVoidShieldEffect = this;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void BrokenDestroy()
    {
        skill.skillController.curVoidShieldEffect = null;
        base.BrokenDestroy();
    }
}
