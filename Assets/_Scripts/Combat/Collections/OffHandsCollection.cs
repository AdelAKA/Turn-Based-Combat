using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    [CreateAssetMenu(fileName = "OffHandsCollection", menuName = "Prodigy Combat Resources/OffHandsCollection")]
    public class OffHandsCollection : ScriptableObject
    {
        public List<OffhandInfo> OffHands = new List<OffhandInfo>();

        [Serializable]
        public class OffhandInfo
        {
            public string name;
            public OffHandName identifier;
            public float extraHealth;
            public float extraMana;
            public SkillName skillName;
            public PassiveSkillName passiveSkillName;
        }

        public OffhandInfo GetOffHandByIdentifier(OffHandName targetName) => OffHands.FirstOrDefault(x => x.identifier == targetName);
    }
}