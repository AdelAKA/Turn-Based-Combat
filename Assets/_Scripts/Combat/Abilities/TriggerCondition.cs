using System;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public abstract class TriggerCondition
    {
        protected bool IsConditionFulfilled;

        public TriggerCondition() { }

        public abstract TriggerCondition CreateNew();

        public bool CheckCondition()
        {
            if (IsConditionFulfilled)
            {
                IsConditionFulfilled = false;
                return true;
            }
            return false;
        }
        public abstract void SubscribeToEvent(Fighter monitoredFighter);
    }

    [Serializable]
    public class HealthConditionTrigger : TriggerCondition
    {
        [Range(0,1)]
        public float HealthTriggerPercentage;

        private float _fighterMaxHealth;
        //private float _healthTriggerPercentage;
        //private Fighter _monitoredFighter;

        public HealthConditionTrigger() { }
        public HealthConditionTrigger(float HealthTriggerPercentage)
        {
            this.HealthTriggerPercentage = HealthTriggerPercentage;
        }

        public override TriggerCondition CreateNew() => new HealthConditionTrigger(HealthTriggerPercentage);

        public override void SubscribeToEvent(Fighter monitoredFighter)
        {
            _fighterMaxHealth = monitoredFighter.MaxHealth;
            monitoredFighter.OnHealthChanged += Fighter_OnHealthChanged;
        }

        private void Fighter_OnHealthChanged(float newHealth, float healthChangeAmount)
        {
            bool condition = newHealth > 0
                && newHealth <= _fighterMaxHealth * HealthTriggerPercentage;
            //Debug.Log($"checking Passive Condition {newHealth} <= {_fighterMaxHealth * HealthTriggerPercentage}");
            IsConditionFulfilled = condition;
        }
    }

    [Serializable]
    public class ManaConditionTrigger : TriggerCondition
    {
        public float ManaTriggerPercentage;

        private float _fighterMaxMana;
        //private Fighter _monitoredFighter;

        public ManaConditionTrigger() { }
        public ManaConditionTrigger(float ManaTriggerPercentage)
        {
            this.ManaTriggerPercentage = ManaTriggerPercentage;
        }

        public override TriggerCondition CreateNew() => new ManaConditionTrigger(ManaTriggerPercentage);

        public override void SubscribeToEvent(Fighter monitoredFighter)
        {
            _fighterMaxMana = monitoredFighter.MaxMana;
            monitoredFighter.OnManaChanged += Fighter_OnManaChanged;
        }

        private void Fighter_OnManaChanged(float newMana, float manaChangeAmount)
        {
            bool condition = newMana <= _fighterMaxMana * ManaTriggerPercentage;
            IsConditionFulfilled = condition;
        }
    }

    [Serializable]
    public class CumulativeDamageDoneConditionTrigger : TriggerCondition
    {
        public float DamageTriggerAmount;

        private float _cumulativeDamage;
        //private float _healthTriggerPercentage;
        //private Fighter _monitoredFighter;

        public CumulativeDamageDoneConditionTrigger() { }
        public CumulativeDamageDoneConditionTrigger(float DamageTriggerAmount)
        {
            this.DamageTriggerAmount = DamageTriggerAmount;
        }

        public override TriggerCondition CreateNew() => new CumulativeDamageDoneConditionTrigger(DamageTriggerAmount);

        public override void SubscribeToEvent(Fighter monitoredFighter)
        {
            _cumulativeDamage = 0;
            monitoredFighter.OnAmountDone += Fighter_OnDamageDone;
        }

        private void Fighter_OnDamageDone(EffectType effectType, float amount)
        {
            if(effectType == EffectType.Damage)
            {
                _cumulativeDamage += amount;
                bool condition = _cumulativeDamage >= DamageTriggerAmount;
                IsConditionFulfilled = condition;
            }
        }
    }
}