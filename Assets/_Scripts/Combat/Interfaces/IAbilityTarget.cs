namespace GP7.Prodigy.Combat
{
    public interface IAbilityTarget
    {
        public float MaxHealth { get; }
        public float Damage { get; }
        public float OffHandStats { get; }

        public float ApplyDamage(float damageAmount);
        public float ApplyHeal(float healAmount);
        public void ApplyStatusEffect(int numberOfTurns, StatusEffectName status, float value);

    }
}