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
            [SerializeReference] public List<AbilityEffect> effects;
            
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
        }

        public SkillInfo GetSkillById(int targetId) => Skills[targetId];
        public SkillInfo GetSkillByIdentifier(SkillName targetName) => Skills.FirstOrDefault(x => x.identifier == targetName);
    }
}
