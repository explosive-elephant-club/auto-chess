using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SalovEmitBDecorator : SkillDecorator
{
    public int effectCount;
    public float spacing;
    public override void Init()
    {
        effectCount = int.Parse(GetParam("effectCount"));
        spacing = float.Parse(GetParam("spacing"));
        skill.InstanceEffectFunc = InstanceEffect;
    }

    public override void InstanceEffect()
    {
        float offset = (effectCount - 1) * spacing / 2;
        if (skill.effectPrefab != null)
        {
            for (int i = 0; i < effectCount; i++)
            {
                float a = Mathf.Pow(-1, i);

                GameObject obj = GameObject.Instantiate(skill.effectPrefab);
                obj.transform.position = skill.GetCastPoint().position;
                obj.transform.rotation = skill.GetCastPoint().rotation;

                SkillEffect skillEffect = obj.GetComponent<SkillEffect>();
                skillEffect.Init(skill, skill.selectorResult.targets[0].transform);

                obj.transform.position = skill.GetCastPoint().position + skill.GetCastPoint().right * (offset - spacing * i);

                skill.effectInstances.Add(skillEffect);
            }
        }
        else  //无特效弹道
        {
            skill.DirectEffectFunc();
        }
    }
}
