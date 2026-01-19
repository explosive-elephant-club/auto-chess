using UnityEngine;

public static class SkillDamageMethods
{
    public enum ColliderType
    {
        Enter,
        Stay,
        Exit,
    }
    public static void SkillDamage(this SkillInstance instance, SkillHelper.DamageLogic damageLogic, Transform self,
        Collider target, ColliderType colliderType)
    {
        //如果碰撞到护盾，调用 OnCollideShieldBegin(hit) 并中止技能
        if (instance.SkillExeContext.LogicData.DestroyOnColliderShield && target.tag == "Shield")
        {
            var shieldInstance = target.GetComponentInParent<SkillInstance>();
            if (shieldInstance.SkillExeContext.Team != instance.SkillExeContext.Team)
            {
                instance.SkillExeContext.SkillHitEffect(target, target.GetComponent<ChampionController>(), true);
                return;
            }
        }
        
        ChampionController championController = target.gameObject.GetComponentInParent<ChampionController>();
        if (championController == null)
            return;
        //如果碰撞到敌人/队友，则调用 OnCollideChampionBegin(c, pos)
        var canContinue = false;
        if (instance.SkillExeContext.SkillTargetType == SkillTargetType.Teammate)
        {
            if (championController.team == instance.SkillExeContext.Team)
            {
                canContinue  = true;
            }
        }
        else if (instance.SkillExeContext.SkillTargetType == SkillTargetType.Enemy)
        {
            if (championController.team != instance.SkillExeContext.Team)
            {
                canContinue  = true;
            }
        }
        if (!canContinue) return;
        switch (damageLogic)
        {
            case SkillHelper.DamageLogic.Normal:
                Normal(instance, self, championController, target, colliderType);
                break;
            case SkillHelper.DamageLogic.DurationAfterDamage:
                DurationAfterDamage(instance, self, championController, target, colliderType);
                break;
            case SkillHelper.DamageLogic.OnlyCollision:
                OnlyCollision(instance, self, championController, target, colliderType);
                break;
        }
    }

    private static void Normal(SkillInstance instance, Transform self, ChampionController championController, Collider target, ColliderType colliderType)
    {
        if (colliderType == ColliderType.Enter)
        {
            instance.SetLastDamageTime(target.GetInstanceID());
            instance.SkillExeContext.SkillHitEffect(target, championController);
        }
        else if (colliderType == ColliderType.Stay)
        {
            var interval = instance.GetDamageTimeInterval(target.GetInstanceID());
            if (interval > SkillConstants.DAMAGE_INTERVAL)
            {
                instance.SetLastDamageTime(target.GetInstanceID());
                instance.SkillExeContext.SkillHitEffect(target, championController);   
            }
        }
    }
    
    private static void OnlyCollision(SkillInstance instance, Transform self, ChampionController championController, Collider target, ColliderType colliderType)
    {
        if(colliderType !=  ColliderType.Enter)
            return;
        instance.SetLastDamageTime(target.GetInstanceID());
        instance.SkillExeContext.SkillHitEffect(target, championController);
    }
    
    private static void DurationAfterDamage(SkillInstance instance, Transform self, ChampionController championController, Collider target, ColliderType colliderType)
    {
        
    }
}
