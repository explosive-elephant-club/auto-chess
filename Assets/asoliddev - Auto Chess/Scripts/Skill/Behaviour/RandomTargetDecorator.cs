using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTargetDecorator : SkillDecorator
{

    public override void TryInstanceEffect()
    {
        //生成技能特效弹道
        if (effectPrefab != null)
        {
            GameObject obj = GameObject.Instantiate(effectPrefab);
            obj.transform.localPosition = GetCastPoint().position;
            obj.transform.localRotation = GetCastPoint().rotation;

            SkillEffect skillEffect = obj.GetComponent<SkillEffect>();
            skillEffect.Init(this, targets[Random.Range(0, targets.Count)].transform);
            effectInstances.Add(skillEffect);

        }
        else  //无特效弹道
        {
            Effect();
        }
    }
}
