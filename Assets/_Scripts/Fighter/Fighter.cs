using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public class Fighter
    {
        public UnityAction<float, float> OnHealthChanged;
        public UnityAction<float, float> OnManaChanged;

        public UnityAction<EffectType, float> OnAmountDone;

        public UnityAction<StatusEffectName> OnStatusEffectApplied;
        public UnityAction<StatusEffectName, float> OnStatusEffectInvoked;

        public Fighter(int index,
            CombatFieldPos position,
            bool isLocalPlayer,
            float maxHealth,
            float maxMana,
            float speed,
            float damage,
            float offHandStats,
            WeaponName heldWeapon,
            OffHandName heldOffHand,
            List<SkillName> skillNames,
            List<PassiveSkillsCollection.PassiveSkillInfo> passiveSkillInfoList,
            List<QuickActionsCollection.QuickActionInfo> quickActionInfoList)
        {
            Index = index;
            Position = position;
            IsLocalPlayer = isLocalPlayer;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            MaxMana = maxMana;
            CurrentMana = maxMana;
            Speed = speed;
            Damage = damage;
            constantDamageModifier = 0;
            percentageDamageModifier = 0;
            OffHandStats = offHandStats;
            HeldWeapon = heldWeapon;
            HeldOffHand = heldOffHand;
            SkillNames = skillNames;
            IsDead = false;
            foreach (var info in passiveSkillInfoList)
            {
                PassiveSkills.Add(new PassiveSkillHandler(this, info));
            }
            foreach (var info in quickActionInfoList)
            {
                QuickActions.Add(new QuickActionHandler(info));
            }
        }

        public int Index { get; private set; }
        public CombatFieldPos Position { get; private set; }
        public bool IsLocalPlayer { get; private set; }
        public float MaxHealth { get; private set; }
        public float MaxMana { get; private set; }
        public float Speed { get; private set; }
        private float _currentDamage;
        public float Damage
        {
            get => _currentDamage + (percentageDamageModifier * _currentDamage) + constantDamageModifier;
            private set
            {
                _currentDamage = value;
            }
        }
        private float percentageDamageModifier;
        private float constantDamageModifier;
        public float OffHandStats { get; private set; }
        public WeaponName HeldWeapon { get; private set; }
        public OffHandName HeldOffHand { get; private set; }
        public List<SkillName> SkillNames { get; private set; }
        public string Name { get; set; }
        public List<PassiveSkillHandler> PassiveSkills { get; private set; } = new List<PassiveSkillHandler>();
        public List<QuickActionHandler> QuickActions { get; private set; } = new List<QuickActionHandler>();
        public List<StatusEffectHandler> StatusEffects { get; private set; } = new List<StatusEffectHandler>();

        public bool IsQuickActionUsed { get; set; }

        //private float _currentHealth;
        public float CurrentHealth { get; private set; }
        //{
        //    get => _currentHealth;
        //    set
        //    {
        //    }
        //}

        private float _currentMana;
        public float CurrentMana
        {
            get => _currentMana;
            set
            {
                float changeAmount = value - _currentMana;
                _currentMana = Mathf.Clamp(value, 0, MaxMana);
                if (changeAmount != 0) OnManaChanged?.Invoke(_currentMana, changeAmount);
            }
        }

        public bool IsDead { get; set; }
        public bool IsStunned => StatusEffects.Any(s => s.StatusEffectName == StatusEffectName.Stun);

        public void OnNewTurn()
        {
            IsQuickActionUsed = false;
        }

        public void CheckStatusEffectsTurns()
        {
            for (int i = StatusEffects.Count - 1; i >= 0; i--)
            {
                bool isEnded = StatusEffects[i].CheckTurnsCounterFinished();
                if (isEnded)
                {
                    if (StatusEffects[i].StatusEffectName == StatusEffectName.DamageBoost)
                        percentageDamageModifier -= StatusEffects[i].Value;
                    //constantDamageModifier -= StatsEffects[i].Value;
                    StatusEffects.RemoveAt(i);
                }
            }
        }

        public float ApplyDamage(float changeAmount)
        {
            float newHealth = CurrentHealth - changeAmount;
            CurrentHealth = Mathf.Clamp(newHealth, 0, MaxHealth);
            if (changeAmount != 0) OnHealthChanged?.Invoke(CurrentHealth, -changeAmount);
            return changeAmount;
        }

        public float ApplyHeal(float changeAmount)
        {
            float newHealth = CurrentHealth + changeAmount;
            CurrentHealth = Mathf.Clamp(newHealth, 0, MaxHealth);
            if (changeAmount != 0) OnHealthChanged?.Invoke(CurrentHealth, changeAmount);
            return changeAmount;
        }

        public void ApplyStatusEffect(StatusEffectHandler appliedStatusEffect)
        {
            StatusEffects.Add(appliedStatusEffect);
            if (appliedStatusEffect.StatusEffectName == StatusEffectName.DamageBoost)
                percentageDamageModifier += appliedStatusEffect.Value;
            //constantDamageModifier += appliedStatsEffect.Value;
            OnStatusEffectApplied?.Invoke(appliedStatusEffect.StatusEffectName);
        }

        public void InvokeDPSstatusEffects()
        {
            Dictionary<StatusEffectName, float> statusEffectsDict = new Dictionary<StatusEffectName, float>();
            for (int i = 0; i < StatusEffects.Count; i++)
            {
                if (StatusEffects[i].StatusEffectName == StatusEffectName.Poison)
                {
                    CurrentHealth -= StatusEffects[i].Value;
                    if (!statusEffectsDict.ContainsKey(StatusEffectName.Poison)) statusEffectsDict.Add(StatusEffectName.Poison, 0);
                    statusEffectsDict[StatusEffectName.Poison] += StatusEffects[i].Value;
                    //OnStatsEffectInvoked?.Invoke(StatsEffectName.Poison, StatsEffects[i].Value);
                }
                if (StatusEffects[i].StatusEffectName == StatusEffectName.Burn)
                {
                    CurrentHealth -= StatusEffects[i].Value;
                    if (!statusEffectsDict.ContainsKey(StatusEffectName.Burn)) statusEffectsDict.Add(StatusEffectName.Burn, 0);
                    statusEffectsDict[StatusEffectName.Burn] += StatusEffects[i].Value;
                    //OnStatsEffectInvoked?.Invoke(StatsEffectName.Burn, StatsEffects[i].Value);
                }
                if (StatusEffects[i].StatusEffectName == StatusEffectName.Stun)
                {
                    if (!statusEffectsDict.ContainsKey(StatusEffectName.Stun)) statusEffectsDict.Add(StatusEffectName.Stun, 0);
                    statusEffectsDict[StatusEffectName.Stun] += StatusEffects[i].TurnCounter;
                    //OnStatsEffectInvoked?.Invoke(StatsEffectName.Stun, StatsEffects[i].TurnCounter);
                }
            }

            foreach (var item in statusEffectsDict)
            {
                OnStatusEffectInvoked?.Invoke(item.Key, item.Value);
            }
        }
    }
}