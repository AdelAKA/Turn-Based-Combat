using System;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public struct CombatMove
    {
        public CombatMove(int skillIndex, int targetHeroIndex, int targetMonsterIndex)
        {
            this.skillIndex = skillIndex;
            this.targetHeroIndex = targetHeroIndex;
            this.targetMonsterIndex = targetMonsterIndex;
        }

        public int skillIndex;
        public int targetHeroIndex;
        public int targetMonsterIndex;
    }
}
