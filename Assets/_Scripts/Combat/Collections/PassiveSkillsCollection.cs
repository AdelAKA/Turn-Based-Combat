using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace GP7.Prodigy.Combat
{
    [CreateAssetMenu(fileName = "PassiveSkillsCollection", menuName = "Prodigy Combat Resources/PassiveSkillsCollection")]
    public class PassiveSkillsCollection : ScriptableObject
    {
        public List<PassiveSkillInfo> PassiveSkills = new List<PassiveSkillInfo>();

        [Serializable]
        public class PassiveSkillInfo
        {
            public string name;
            public PassiveSkillName identifier;
            [SerializeReference] public TriggerCondition condition;
            public SkillTargetType targetType;
            public EffectType type;
            public float amount;
            public Sprite icon;
            public ParticleSystem skillEffect;
        }

        public PassiveSkillInfo GetPassiveSkillByIdentifier(PassiveSkillName targetName) => PassiveSkills.FirstOrDefault(x => x.identifier == targetName);
    }
}