using System;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public abstract class AbilityEffect
    {
        public EffectType Type;
        public string effectSummery;
        public abstract float Apply(IAbilityTarget initiator, IAbilityTarget target);
        protected float CalculateValue(IAbilityTarget initiator, IAbilityTarget targetFighter, SkillValueCalculationType calculationType, float value)
        {
            switch (calculationType)
            {
                case SkillValueCalculationType.MultipliedDamage:
                    return MultipliedDamage(initiator.Damage, value);
                case SkillValueCalculationType.PercentageDamage:
                    return PercentageDamage(targetFighter.MaxHealth, value);
                case SkillValueCalculationType.RawValueHealing:
                    return RawHealing(value);
                case SkillValueCalculationType.PercentageHealing:
                    return PercentageHealing(targetFighter.MaxHealth, value);
                case SkillValueCalculationType.AdditiveOffHandDamage:
                    return AdditiveOffHandDamage(initiator.OffHandStats, value);
                default:
                    return 0;
            }
        }

        protected float MultipliedDamage(float baseDamage, float multiplier) => baseDamage * multiplier;
        protected float PercentageDamage(float fullHealth, float percentage) => fullHealth * percentage;
        protected float RawHealing(float rawValue) => rawValue;
        protected float PercentageHealing(float fullHealth, float percentage) => fullHealth * percentage;
        protected float AdditiveOffHandDamage(float offHandStats, float extraDamage) => offHandStats + extraDamage;
    }

    [Serializable]
    public class DamageEffect : AbilityEffect
    {
        public SkillValueCalculationType calculationType;
        public float value;

        public override float Apply(IAbilityTarget initiator, IAbilityTarget target)
        {
            return target.ApplyDamage(CalculateValue(initiator, target, calculationType, value));
        }
    }

    [Serializable]
    public class HealEffect : AbilityEffect
    {
        public SkillValueCalculationType calculationType;
        public float value;

        public override float Apply(IAbilityTarget initiator, IAbilityTarget target)
        {
            return target.ApplyHeal(CalculateValue(initiator, target, calculationType, value));
            //target.ApplyDamage(CalculateValue(initiator, target, calculationType, value));
        }
    }

    [Serializable]
    public class StatusApplyingEffect : AbilityEffect
    {
        public StatusEffectName stats;
        public int numberOfTurns;
        public float value;

        public override float Apply(IAbilityTarget initiator, IAbilityTarget target)
        {
            target.ApplyStatusEffect(numberOfTurns, stats, value);
            return 0;
        }
    }
}
