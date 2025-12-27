namespace GP7.Prodigy.Combat
{
    public struct BaseStats
    {
        public float health;
        public float mana;
        public float speed;
        public float damage;
        public float offHand;

        public BaseStats(float health, float mana, float speed, float damage, float offHand)
        {
            this.health = health;
            this.mana = mana;
            this.speed = speed;
            this.damage = damage;
            this.offHand = offHand;
        }
    }
}
