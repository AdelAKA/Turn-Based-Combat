using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    [CreateAssetMenu(fileName = "WeaponsCollection", menuName = "Prodigy Combat Resources/WeaponsCollection")]
    public class WeaponsCollection : ScriptableObject
    {
        public List<WeaponInfo> Weapons = new List<WeaponInfo>();

        [Serializable]
        public class WeaponInfo
        {
            public string name;
            public WeaponName identifier;
            public WeaponType type;
            public float damage;
            public List<SkillName> skillNames;
        }

        public WeaponInfo GetWeaponByIdentifier(WeaponName targetName) => Weapons.FirstOrDefault(x => x.identifier == targetName);

    }
}
