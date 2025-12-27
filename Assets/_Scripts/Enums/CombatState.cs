namespace GP7.Prodigy.Combat
{
    public enum CombatState
    {
        NotStarted,
        Starting,
        CheckingStatusEffects,
        DecidingMove,
        MakingMove,
        CheckingPassives,
        InvokingPassive,
        OnHold,
        Finished
    }
}
