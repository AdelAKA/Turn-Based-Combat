using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace GP7.Prodigy.Combat
{
    [CreateAssetMenu(fileName = "SkillsCollection", menuName = "Prodigy Combat Resources/SkillsCollection")]
    public class SkillsCollection : ScriptableObject
    {
        public List<SkillInfo> Skills = new List<SkillInfo>();

        [Serializable]
        public class SkillInfo
        {
            public string name;
            public SkillName identifier;
            public Sprite icon;
            public ParticleSystem skillEffect;
            public float requiredMP;
            public SkillTargetType targetType;
            public SkillAOE aoe;
            // AbilityEffect
            //public EffectType type;
            //public SkillValueCalculationType calculationType;
            //public List<float> values;
            [SerializeReference] public List<AbilityEffect> effects;
            //public StatsEffectName statsEffect;
            //public float multiplier;

            public bool IsDamageType() => effects.Any(e => e.Type == EffectType.Damage);

            public string SummarizeSkillInfo()
            {
                string content = "";
                content += $"MP Cost: {requiredMP}\n";
                content += $"Target: {targetType}\n";
                content += $"AOE: {aoe}\n";
                foreach (var effect in effects)
                {
                    content += "-" + effect.effectSummery + "\n";
                }
                return content;
            }

            //public float CalculateValue(Fighter initiator, Fighter targetFighter)
            //{
            //    switch (calculationType)
            //    {
            //        case SkillValueCalculationType.MultipliedDamage:
            //            return MultipliedDamage(initiator.Damage, values[0]);
            //        case SkillValueCalculationType.PercentageDamage:
            //            return PercentageDamage(targetFighter.MaxHealth, values[0]);
            //        case SkillValueCalculationType.RawValueHealing:
            //            return RawHealing(values[0]);
            //        case SkillValueCalculationType.PercentageHealing:
            //            return PercentageHealing(targetFighter.MaxHealth, values[0]);
            //        case SkillValueCalculationType.AdditiveOffHandDamage:
            //            return AdditiveOffHandDamage(initiator.OffHandStats, values[0]);
            //        default:
            //            return 0;
            //    }
            //}

            //public float MultipliedDamage(float baseDamage, float multiplier) => baseDamage * multiplier;
            //public float PercentageDamage(float fullHealth, float percentage) => fullHealth * percentage;
            //public float RawHealing(float rawValue) => rawValue;
            //public float PercentageHealing(float fullHealth, float percentage) => fullHealth * percentage;
            //public float AdditiveOffHandDamage(float offHandStats, float extraDamage) => offHandStats + extraDamage;
        }

        public SkillInfo GetSkillById(int targetId) => Skills[targetId];
        public SkillInfo GetSkillByIdentifier(SkillName targetName) => Skills.FirstOrDefault(x => x.identifier == targetName);
    }
}
