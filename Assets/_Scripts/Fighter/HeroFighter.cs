using System.Collections.Generic;

namespace GP7.Prodigy.Combat
{
    public class HeroFighter : Fighter
    {

        public HeroFighter(int index,
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
            List<QuickActionsCollection.QuickActionInfo> quickActionInfoList)
            : base(index, position, isLocalPlayer, maxHP, maxMP, speed, damage, offHandStats, heldWeapon, heldOffHand, skillNames, passiveSkillInfoList, quickActionInfoList)
        {
            Name = "Player";
        }
}
}
