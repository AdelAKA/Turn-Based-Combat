public struct CombatPassiveMove
{
    public CombatPassiveMove(int passiveSkillIndex, int targetHeroIndex, int targetMonsterIndex)
    {
        this.passiveSkillIndex = passiveSkillIndex;
        this.targetHeroIndex = targetHeroIndex;
        this.targetMonsterIndex = targetMonsterIndex;
    }

    public int passiveSkillIndex;
    public int targetHeroIndex;
    public int targetMonsterIndex;
}
