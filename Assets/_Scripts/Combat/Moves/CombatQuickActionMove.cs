namespace GP7.Prodigy.Combat
{
    public struct CombatQuickActionMove
    {
        public CombatQuickActionMove(int quickActionIndex, int targetHeroIndex, int targetMonsterIndex)
        {
            this.quickActionIndex = quickActionIndex;
            this.targetHeroIndex = targetHeroIndex;
            this.targetMonsterIndex = targetMonsterIndex;
        }

        public int quickActionIndex;
        public int targetHeroIndex;
        public int targetMonsterIndex;
    }
}
