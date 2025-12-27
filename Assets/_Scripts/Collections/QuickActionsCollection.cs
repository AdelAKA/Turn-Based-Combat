using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace GP7.Prodigy.Combat
{
    [CreateAssetMenu(fileName = "QuickActionsCollection", menuName = "Prodigy Combat Resources/QuickActionsCollection")]
    public class QuickActionsCollection : ScriptableObject
    {
        public List<QuickActionInfo> Skills = new List<QuickActionInfo>();

        [Serializable]
        public class QuickActionInfo
        {
            public string name;
            public QuickActionName identifier;
            public SkillTargetType targetType;
            public SkillAOE aoe;
            public int startCount;
            [SerializeReference] public AbilityEffect effect;
            public Sprite icon;
            public ParticleSystem skillEffect;

            public bool IsDamageType() => effect.Type == EffectType.Damage;


            public string SummarizeSkillInfo()
            {
                string content = "";
                content += $"Target: {targetType}\n";
                content += $"AOE: {aoe}\n";
                content += "-" + effect.effectSummery + "\n";

                return content;
            }
        }

        public QuickActionInfo GetSkillById(int targetId) => Skills[targetId];
        public QuickActionInfo GetQuickActionByIdentifier(QuickActionName targetName) => Skills.FirstOrDefault(x => x.identifier == targetName);
    }
}
