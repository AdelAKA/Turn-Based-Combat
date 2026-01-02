using System.Collections.Generic;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    public class MonsterFighter : Fighter
    {
        public MonsterName MonsterNameReference { get; private set; }

        public MonsterFighter(MonsterName monsterName,
            int index,
            CombatFieldPos position,
            bool isLocalPlayer,
            float maxHP,
            float maxMP,
            float speed,
            float damage,
            float offHandStats,
            WeaponName heldWeapon,
            OffHandName heldOffHand,
            List<SkillName> skillNames,
            List<PassiveSkillsCollection.PassiveSkillInfo> passiveSkillInfoList,
            List<QuickActionsCollection.QuickActionInfo> quickActionInfoList,
            SkillsCollection skillsCollection)
            : base(index, position, isLocalPlayer, maxHP, maxMP, speed, damage, offHandStats, heldWeapon, heldOffHand, skillNames, passiveSkillInfoList, quickActionInfoList, skillsCollection)
        {
            Name = monsterName.ToString();
            MonsterNameReference = monsterName;
        }

        public CombatMove ChoooseRandomMove(Combat involvedCombat)
        {
            // TODO: check for bets target according to skills

            List<SkillsCollection.SkillInfo> possibleSkills = new List<SkillsCollection.SkillInfo>();
            for (int i = 0; i < SkillNames.Count; i++)
            {
                SkillsCollection.SkillInfo skillInfo = skillsCollection.GetSkillByIdentifier(SkillNames[i]);
                //Debug.Log($"{skillInfo.name}: ({CurrentMana}, {skillInfo.requiredMP})");
                if (CurrentMana >= skillInfo.requiredMP)
                    possibleSkills.Add(skillInfo);
                else
                    Debug.Log("Not Valid");
            }

            SkillsCollection.SkillInfo randomSkill = possibleSkills.Random();
            List<Fighter> possibleFighters = involvedCombat.GetTargetableFighters(randomSkill.targetType);
            Fighter randomFighter = possibleFighters.Random();
            int RandomHeroIndex = randomFighter.IsLocalPlayer ? involvedCombat.GetHeroIndex(randomFighter) : -1;
            int RandomMonsterIndex = !randomFighter.IsLocalPlayer ? involvedCombat.GetMonsterIndex(randomFighter) : -1;

            return new CombatMove(
                skillIndex: SkillNames.IndexOf(randomSkill.identifier),
                //targetHeroIndex: new List<int>() { involvedCombat._heros.IndexOf(involvedCombat._heros.Random()) },
                targetHeroIndex: RandomHeroIndex,
                targetMonsterIndex: RandomMonsterIndex
            );
        }
    }
}
