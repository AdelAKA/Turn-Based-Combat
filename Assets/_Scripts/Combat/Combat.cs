using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    [Serializable]
    public class Combat
    {
        public List<Fighter> _heros;
        public List<Fighter> _monsters;

        private Queue<Fighter> turnQueue = new Queue<Fighter>();
        public Fighter CurrentFighterTurn { get; private set; }
        public CombatState CurrentCombatState { get; set; }
        public CombatResult CurrentCombatResult { get; private set; }
        public List<Fighter> TurnList => turnQueue.ToList();

        public Fighter GetHero(int index) => _heros[index];
        public Fighter GetMonster(int index) => _monsters[index];
        public int GetHeroIndex(Fighter hero) => _heros.IndexOf(hero);
        public int GetMonsterIndex(Fighter monster) => _monsters.IndexOf(monster);

        // Collections
        private SkillsCollection skillsCollection;
        private QuickActionsCollection quickActionsCollection;
        private PassiveSkillsCollection passiveSkillsCollection;

        public Combat(
            List<Fighter> heros,
            List<Fighter> monsters,
            SkillsCollection skillsCollection,
            QuickActionsCollection quickActionsCollection,
            PassiveSkillsCollection passiveSkillsCollection
            )
        {
            _heros = heros;
            _monsters = monsters;

            List<Fighter> orderedList = new List<Fighter>();
            orderedList.AddRange(heros);
            orderedList.AddRange(monsters);
            orderedList = orderedList.OrderBy(x => x.Speed).Reverse().ToList();
            turnQueue.Clear();
            foreach (Fighter fighter in orderedList)
                turnQueue.Enqueue(fighter);

            CurrentCombatResult = CombatResult.NotDecided;

            this.skillsCollection = skillsCollection;
            this.quickActionsCollection = quickActionsCollection;
            this.passiveSkillsCollection = passiveSkillsCollection;
        }

        public void SwitchFighter()
        {
            do
            {
                CurrentFighterTurn = turnQueue.Dequeue();
                turnQueue.Enqueue(CurrentFighterTurn); // Add Back to queue
            } while (CurrentFighterTurn.IsDead);

            CurrentFighterTurn.OnNewTurn();
        }

        public (bool, string) CheckIfMoveValid(int initiatorIndex, bool isHero, CombatMove move)
        {
            Fighter initiator = isHero ? _heros[initiatorIndex] : _monsters[initiatorIndex];
            SkillName chosenSkill = initiator.SkillNames[move.skillIndex];

            SkillsCollection.SkillInfo skillInfo = skillsCollection.GetSkillByIdentifier(chosenSkill);
            if (skillInfo == null) return (false, $"no info on skill {chosenSkill}");

            Fighter targetedFighter = move.targetHeroIndex != -1 ? _heros[move.targetHeroIndex] : _monsters[move.targetMonsterIndex];

            if (!CheckIfCurrentFighterTurn(initiator)) return (false, $"not the fighters turn");
            if (!CheckIfFighterHasSkill(initiator, chosenSkill)) return (false, $"fighter doesn't have this skill");
            if (!CheckForRequiredMP(initiator, skillInfo)) return (false, $"not enough mp");
            if (!CheckIfSkillTargetIsValid(skillInfo.targetType, targetedFighter)) return (false, $"target not valid");

            return (true, "Valid");
        }

        public (bool, string) CheckIfQuickActionValid(int initiatorIndex, bool isHero, CombatQuickActionMove move)
        {
            Fighter initiator = isHero ? _heros[initiatorIndex] : _monsters[initiatorIndex];
            QuickActionName chosenQuickAction = initiator.QuickActions[move.quickActionIndex].QuickActionName;

            QuickActionsCollection.QuickActionInfo quckActionInfo = quickActionsCollection.GetQuickActionByIdentifier(chosenQuickAction);
            if (quckActionInfo == null) return (false, $"no info on quick action {chosenQuickAction}");

            Fighter targetedFighter = move.targetHeroIndex != -1 ? _heros[move.targetHeroIndex] : _monsters[move.targetMonsterIndex];

            if (!CheckIfCurrentFighterTurn(initiator)) return (false, $"not the fighters turn");
            if (!CheckIfFighterCanUseQuickAction(initiator)) return (false, $"fighter already used quick aciton this turn");
            if (!CheckIfFighterHasQuickAction(initiator, chosenQuickAction)) return (false, $"fighter doesn't have this quick action");
            if (!CheckIfFighterHasEnoughQuickActions(initiator, chosenQuickAction)) return (false, $"fighter doesn't have enough quick action");
            if (!CheckIfSkillTargetIsValid(quckActionInfo.targetType, targetedFighter)) return (false, $"target not valid");

            return (true, "Valid");
        }

        private bool CheckIfCurrentFighterTurn(Fighter fighter) => CurrentFighterTurn == fighter;
        private bool CheckIfFighterHasSkill(Fighter fighter, SkillName chosenSkill) => fighter.SkillNames.Any(X => X == chosenSkill);
        private bool CheckForRequiredMP(Fighter fighter, SkillsCollection.SkillInfo skillInfo) => fighter.CurrentMana >= skillInfo.requiredMP;
        private bool CheckIfSkillTargetIsValid(SkillTargetType chosenSkillTargetType, Fighter target) => GetTargetableFighters(chosenSkillTargetType).Contains(target);
        private bool CheckIfFighterHasQuickAction(Fighter fighter, QuickActionName chosenQuickAction) => fighter.QuickActions.Any(q => q.QuickActionName == chosenQuickAction);
        private bool CheckIfFighterCanUseQuickAction(Fighter fighter) => !fighter.IsQuickActionUsed;
        private bool CheckIfFighterHasEnoughQuickActions(Fighter fighter, QuickActionName chosenQuickAction) => fighter.CanConsume(chosenQuickAction);


        public List<Fighter> GetTargetableFighters(SkillTargetType skillTargetType)
        {
            List<Fighter> targetableFighters = new List<Fighter>();

            //SkillsCollection.SkillInfo skillInfo = OfflineCombatManager.Instance.skillsCollection.GetSkillByIdentifier(chosenSkill);
            //if (skillInfo == null) return targetableFighters;

            switch (skillTargetType)
            {
                case SkillTargetType.None:
                    break;
                case SkillTargetType.Self:
                    targetableFighters.Add(CurrentFighterTurn);
                    break;
                case SkillTargetType.MeleeClosestColumn:
                    List<Fighter> targetGroup = new List<Fighter>();
                    if (CurrentFighterTurn.IsLocalPlayer) targetGroup = _monsters;
                    else targetGroup = _heros;

                    foreach (Fighter target in targetGroup)
                    {
                        if (!targetGroup.Any(
                            t => t != target
                            && !t.IsDead
                            && t.Position.rowIndex == target.Position.rowIndex
                            && t.Position.columnIndex < target.Position.columnIndex))
                        {
                            targetableFighters.Add(target);
                        }
                    }
                    break;
                case SkillTargetType.RangeAnyCell:
                    if (CurrentFighterTurn.IsLocalPlayer) targetableFighters = _monsters.Where(h => !h.IsDead).ToList();
                    else targetableFighters = _heros.Where(m => !m.IsDead).ToList();
                    break;
                case SkillTargetType.AnyAlly:
                    if (CurrentFighterTurn.IsLocalPlayer) targetableFighters = _heros.Where(h => !h.IsDead).ToList();
                    else targetableFighters = _monsters.Where(m => !m.IsDead).ToList();
                    break;
                default:
                    break;
            }
            return targetableFighters;
        }

        public List<Fighter> GetAffectedFighters(Fighter targetedFighter, SkillAOE skillAOE)
        {
            List<Fighter> affectedFighters = new List<Fighter>();

            //SkillsCollection.SkillInfo skillInfo = OfflineCombatManager.Instance.skillsCollection.GetSkillByIdentifier(chosenSkill);
            //if (skillAOE == null) return affectedFighters;

            switch (skillAOE)
            {
                case SkillAOE.Target:
                    affectedFighters.Add(targetedFighter);
                    break;
                case SkillAOE.Field:
                    affectedFighters.AddRange(targetedFighter.IsLocalPlayer ? _heros.Where(h => !h.IsDead) : _monsters.Where(m => !m.IsDead));
                    break;
                case SkillAOE.Column:
                    {
                        List<Fighter> targetGroup;
                        if (CurrentFighterTurn.IsLocalPlayer) targetGroup = _monsters;
                        else targetGroup = _heros;

                        foreach (Fighter target in targetGroup)
                        {
                            if (!target.IsDead && targetedFighter.Position.columnIndex == target.Position.columnIndex)
                            {
                                affectedFighters.Add(target);
                            }
                        }
                    }
                    break;
                case SkillAOE.Row:
                    {
                        List<Fighter> targetGroup;
                        if (CurrentFighterTurn.IsLocalPlayer) targetGroup = _monsters;
                        else targetGroup = _heros;

                        foreach (Fighter target in targetGroup)
                        {
                            if (!target.IsDead && targetedFighter.Position.rowIndex == target.Position.rowIndex)
                            {
                                affectedFighters.Add(target);
                            }
                        }
                    }
                    break;
                case SkillAOE.OneCellNeighborInColumn:
                    {
                        List<Fighter> targetGroup;
                        if (CurrentFighterTurn.IsLocalPlayer) targetGroup = _monsters;
                        else targetGroup = _heros;

                        foreach (Fighter target in targetGroup)
                        {
                            if (!target.IsDead
                                && targetedFighter.Position.columnIndex == target.Position.columnIndex
                                && Mathf.Abs(targetedFighter.Position.rowIndex - target.Position.rowIndex) <= 1)
                            {
                                affectedFighters.Add(target);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            return affectedFighters;
        }

        public void ApplyMove(int initiatorIndex, bool isHero, CombatMove move)
        {
            Fighter initiator = isHero ? _heros[initiatorIndex] : _monsters[initiatorIndex];
            Fighter targeted = move.targetHeroIndex != -1 ? _heros[move.targetHeroIndex] : _monsters[move.targetMonsterIndex];
            SkillName chosenSkill = initiator.SkillNames[move.skillIndex];

            SkillsCollection.SkillInfo chosenSkillInfo = skillsCollection.GetSkillByIdentifier(chosenSkill);
            initiator.CurrentMana -= chosenSkillInfo.requiredMP;

            // Basic skill will add 10% mana
            if (move.skillIndex == 0) initiator.CurrentMana += initiator.MaxMana * 0.1f;

            List<Fighter> targetFighters = GetAffectedFighters(targeted, chosenSkillInfo.aoe);

            Dictionary<EffectType, float> amountsDictionary = new Dictionary<EffectType, float>();

            foreach (var effect in chosenSkillInfo.effects)
            {
                amountsDictionary.Add(effect.Type, 0);
                foreach (Fighter target in targetFighters)
                {
                    amountsDictionary[effect.Type] += effect.Apply(initiator, target);
                }
                initiator.OnAmountDone?.Invoke(effect.Type, amountsDictionary[effect.Type]);
            }
            CheckForCasualties();
        }

        public void ApplyQuickAction(int initiatorIndex, bool isHero, CombatQuickActionMove quickActionMove)
        {
            Fighter initiator = isHero ? _heros[initiatorIndex] : _monsters[initiatorIndex];
            Fighter targeted = quickActionMove.targetHeroIndex != -1 ? _heros[quickActionMove.targetHeroIndex] : _monsters[quickActionMove.targetMonsterIndex];
            // TODO: Only Heros can use quick actions

            QuickActionName invokedQuickAction = initiator.InvokeQuickAction(quickActionMove.quickActionIndex);

            QuickActionsCollection.QuickActionInfo chosenQuickActionInfo = quickActionsCollection.GetQuickActionByIdentifier(invokedQuickAction);

            List<Fighter> targetFighters = GetAffectedFighters(targeted, chosenQuickActionInfo.aoe);

            foreach (Fighter target in targetFighters)
            {
                chosenQuickActionInfo.effect.Apply(initiator, target);
            }
            CheckForCasualties();
        }

        public List<CombatPassiveMove> CheckForPassiveSkills()
        {
            List<CombatPassiveMove> combatPassiveMoves = new List<CombatPassiveMove>();

            for (int i = 0; i < _heros.Count; i++)
            {
                combatPassiveMoves.AddRange(_heros[i].CheckForPassiveSkills());
            }

            for (int i = 0; i < _monsters.Count; i++)
            {
                combatPassiveMoves.AddRange(_monsters[i].CheckForPassiveSkills());
            }
            return combatPassiveMoves;
        }

        public void ApplyPassiveMoves(List<CombatPassiveMove> passiveMoves)
        {
            foreach (var passiveMove in passiveMoves)
            {
                Fighter initiator = passiveMove.targetHeroIndex >= 0 ? _heros[passiveMove.targetHeroIndex] : _monsters[passiveMove.targetMonsterIndex];
                PassiveSkillName invokedPassiveSkill = initiator.InvokePassiveSkill(passiveMove.passiveSkillIndex);

                //WeaponsCollection.WeaponInfo weaponInfo = OfflineCombatManager.Instance.weaponsCollection.GetWeaponByIdentifier(initiator.HeldWeapon);
                // TODO: Calculate skill values from weapon

                PassiveSkillsCollection.PassiveSkillInfo invokedPassiveInfo = passiveSkillsCollection.GetPassiveSkillByIdentifier(invokedPassiveSkill);
                initiator.ApplyHeal(invokedPassiveInfo.amount);
                // TODO: Apply Different Effects After Deciding How To Choose Target for Damaging Skills
            }
        }

        public bool ApplyAndCheckCurrentFighterStatusEffects()
        {
            CurrentFighterTurn.InvokeDPSstatusEffects();
            CheckForCasualties();

            // Check skipping turn before status effect turn is updated
            bool skipTurn = CurrentFighterTurn.IsStunned || CurrentFighterTurn.IsDead;

            // Update and remove finished status effects
            CurrentFighterTurn.CheckStatusEffectsTurns();
            return skipTurn;
        }

        private void CheckForCasualties()
        {
            Queue<Fighter> tempTurnQueue = new Queue<Fighter>(turnQueue);
            turnQueue.Clear();

            while (tempTurnQueue.TryDequeue(out Fighter queuedFighter))
            {
                if (!queuedFighter.IsDead && queuedFighter.CurrentHealth <= 0)
                {
                    queuedFighter.IsDead = true;
                    Debug.Log($"{queuedFighter} fainted");
                }
                turnQueue.Enqueue(queuedFighter);
            }
        }

        public CombatResult CheckCombatResult()
        {
            if (_heros.All(h => h.CurrentHealth <= 0))
            {
                CurrentCombatState = CombatState.Finished;
                CurrentCombatResult = CombatResult.Lost;
                return CombatResult.Lost;
            }
            if (_monsters.All(m => m.CurrentHealth <= 0))
            {
                CurrentCombatState = CombatState.Finished;
                CurrentCombatResult = CombatResult.Won;
                return CombatResult.Won;
            }
            return CombatResult.NotDecided;
        }
    }
}
