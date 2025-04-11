using ExcelConfig;
using UnityEngine;

namespace WeSkill
{
    public class BaseSkillContext
    {
        // 生效(释放)次数
        public int EffectCount;
        // 基础伤害比例
        public int DamageProportion;
        // 持续时间
        public float Duration;
        // 冷却时间
        public float Cooldown;
        // 效果范围
        public float Range;
        // 效果目标类型
        public SkillTargetType SkillTargetType;
        // 效果目标选中方式
        public SkillRangeSelectorType SkillRangeSelectorType;
        // 效果目标选择器类型
        public SkillTargetSelectorType SkillTargetSelectorType;
    }

    public class SkillBase : IState
    {
        private SkillData _skillCfg;
        private BaseSkillContext _skillContext;
        private Animator _animator;

        public void InitState(BlackBoard blackBoard)
        {
            var a = blackBoard as ChampionBlackBoard;
        }
        public StateExeResult ExeState()
        {
            return StateExeResult.Done;
        }
        
        public void Init(SkillData skillData, Animator animator)
        {
            _skillCfg = skillData;
            _animator = animator;
            _skillContext = new BaseSkillContext()
            {

            };
        }
        
        private void LoadEffect()
        {
            
        }
    }
}