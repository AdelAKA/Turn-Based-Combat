using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GP7.Prodigy.Combat
{
    [CreateAssetMenu(fileName = "MonstersCollection", menuName = "Prodigy Combat Resources/MonstersCollection")]
    public class MonstersCollection : ScriptableObject
    {
        public List<MonsterInfo> monstersInfos = new List<MonsterInfo>();

        [Serializable]
        public class MonsterInfo
        {
            public MonsterName name;
            public float hP;
            public float mP;
            public float speed;
            public WeaponName heldWeapon;
            //public List<SkillName> skillNames;
            public Sprite monsterSprite;
        }

        public MonsterInfo GetMonsterInfoById(int targetId) => monstersInfos[targetId];
        public MonsterInfo GetMonsterInfoByName(MonsterName targetName) => monstersInfos.FirstOrDefault(x => x.name == targetName);
    }
}
