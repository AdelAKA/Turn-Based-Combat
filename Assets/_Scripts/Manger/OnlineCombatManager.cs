using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace GP7.Prodigy.Combat
{
    public class OnlineCombatManager : NetworkBehaviour
    {

        public static OnlineCombatManager Instance { get; protected set; }

        [Header("Resources")]
        public WeaponsCollection weaponsCollection;
        public SkillsCollection skillsCollection;
        public MonstersCollection monstersCollection;
        public PassiveSkillsCollection passiveSkillsCollection;
        public OffHandsCollection offHandsCollection;
        public QuickActionsCollection quickActionsCollection;

        [Header("Testing")]
        [SerializeField] protected bool isDebug;

        // Server Variables
        [SerializedDictionary("CombatId", "Combat")]
        protected SerializedDictionary<int, Combat> combatsDictionary = new SerializedDictionary<int, Combat>();
        //protected int lastCombatId = 0;

        // Client Variables
        protected Combat localCombat;
        protected int localCombatId;
        protected CombatMove queuedMove;
        protected CombatQuickActionMove queuedQuickActionMove;

        protected List<MonsterName> requestedMonstersList;
        protected List<CombatFieldPos> requestedMonstersPositionsList;
        protected BaseStats requestedHeroStats;
        protected WeaponName requestedHeroWeapon;
        protected OffHandName requestedHeroOffhand;

        // Properties
        public CombatState CurrentCombatState => localCombat != null ? localCombat.CurrentCombatState : CombatState.NotStarted;
        public CombatResult CurrentCombatResult => localCombat != null ? localCombat.CurrentCombatResult : CombatResult.NotDecided;

        public void OnClientDiconnected(int clientId)
        {
            if (combatsDictionary.ContainsKey(clientId))
                combatsDictionary.Remove(clientId);
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public Combat InitializeCombat(List<MonsterName> monstersNames, List<CombatFieldPos> monstersPositions, BaseStats heroBaseStats, WeaponName heroWeapon, OffHandName heroOffhand)
        {
            Fighter hero;
            {
                float maxHP = heroBaseStats.health;
                float maxMP = heroBaseStats.mana;

                WeaponsCollection.WeaponInfo heroWeaponInfo = weaponsCollection.GetWeaponByIdentifier(heroWeapon);
                List<SkillName> skillNames = new List<SkillName>(heroWeaponInfo.skillNames);

                List<PassiveSkillsCollection.PassiveSkillInfo> passiveSkills = new List<PassiveSkillsCollection.PassiveSkillInfo>();
                if (heroOffhand != OffHandName.None)
                {
                    OffHandsCollection.OffhandInfo offHandInfo = offHandsCollection.GetOffHandByIdentifier(heroOffhand);
                    maxHP += offHandInfo.extraHealth;
                    maxMP += offHandInfo.extraMana;

                    if (offHandInfo.skillName != SkillName.None)
                    {
                        skillNames.Add(offHandInfo.skillName);
                    }
                    if (offHandInfo.passiveSkillName != PassiveSkillName.None)
                    {
                        PassiveSkillsCollection.PassiveSkillInfo passiveSkillInfo = passiveSkillsCollection.GetPassiveSkillByIdentifier(offHandInfo.passiveSkillName);
                        passiveSkills.Add(passiveSkillInfo);
                    }
                }

                QuickActionsCollection.QuickActionInfo rock = quickActionsCollection.GetQuickActionByIdentifier(QuickActionName.Rock);
                QuickActionsCollection.QuickActionInfo healPotion = quickActionsCollection.GetQuickActionByIdentifier(QuickActionName.healingPotion);
                QuickActionsCollection.QuickActionInfo damageBoost = quickActionsCollection.GetQuickActionByIdentifier(QuickActionName.DamgeBoostPotion);

                hero = new HeroFighter(
                        index: 0,
                        new CombatFieldPos() { rowIndex = 1, columnIndex = 0, isLocal = true },
                        isLocalPlayer: true,
                        maxHP: maxHP,
                        maxMP: maxMP,
                        speed: heroBaseStats.speed,
                        damage: heroBaseStats.damage + heroWeaponInfo.damage,
                        offHandStats: heroBaseStats.offHand,
                        heldWeapon: heroWeapon,
                        heldOffHand: heroOffhand,
                        skillNames: skillNames,
                        passiveSkillInfoList: passiveSkills,
                        quickActionInfoList: new() { rock, healPotion, damageBoost }
                    );
            }

            List<Fighter> monsters = new List<Fighter>();
            for (int i = 0; i < monstersNames.Count; i++)
            {
                MonstersCollection.MonsterInfo monsterInfo = monstersCollection.GetMonsterInfoByName(monstersNames[i]);
                if (monsterInfo == null) continue;
                WeaponsCollection.WeaponInfo monsterWeaponInfo = weaponsCollection.GetWeaponByIdentifier(monsterInfo.heldWeapon);
                PassiveSkillsCollection.PassiveSkillInfo passiveSkillInfo = passiveSkillsCollection.GetPassiveSkillByIdentifier(PassiveSkillName.healPercentage);

                Fighter monster = new MonsterFighter(
                    monsterName: monsterInfo.name,
                    index: i,
                    monstersPositions[i],
                    isLocalPlayer: false,
                    maxHP: monsterInfo.hP,
                    maxMP: monsterInfo.mP,
                    speed: monsterInfo.speed,
                    damage: monsterWeaponInfo.damage,
                    offHandStats: 0,
                    heldWeapon: monsterInfo.heldWeapon,
                    heldOffHand: OffHandName.None,
                    skillNames: monsterWeaponInfo.skillNames,
                    passiveSkillInfoList: new() { passiveSkillInfo },
                    quickActionInfoList: new() { }
                );
                monsters.Add(monster);
            }
            Combat newCombat = new Combat(new List<Fighter>() { hero }, monsters);
            return newCombat;
        }

        #region ServerRPC
        [ServerRpc(RequireOwnership = false)]
        private void RequestCombat(NetworkConnection requester, List<MonsterName> monstersName, List<CombatFieldPos> monstersPositions, BaseStats heroBaseStats, WeaponName heroWeapon, OffHandName heroOffhand)
        {
            //lastCombatId++;
            Combat newCombat = InitializeCombat(monstersName, monstersPositions, heroBaseStats, heroWeapon, heroOffhand);
            combatsDictionary.Add(requester.ClientId, newCombat);
            //combatsDictionary[lastCombatId].SwitchFighter();

            //InitializeField(newCombatId);
            //combatsDictionary[newCombatId].SwitchFighter();

            RequestCombat_Response(requester, requester.ClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestFinishCombat(NetworkConnection requester, int combatId)
        {
            if (combatsDictionary.ContainsKey(combatId))
                combatsDictionary.Remove(combatId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void NextRoundRPC_Request(NetworkConnection requester, int combatId)
        {
            if (isDebug) Debug.Log("checking next move server");

            combatsDictionary[combatId].SwitchFighter();

            //UpdateUiForCurrentFighter(); // not needed
            //if (!combatsDictionary[localCombatId].CurrentFighterTurn.IsLocalPlayer)
            //{
            //    RequestAiMoveRPC(localCombatId, combatsDictionary[localCombatId].CurrentFighterIndex());
            //} // not needed

            NextRoundRPC_Response(requester);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckStatusEffectInvokeRPC_Request(NetworkConnection requester, int combatId)
        {
            bool skipTurn = combatsDictionary[combatId].ApplyAndCheckCurrentFighterStatusEffects();
            CheckStatusEffectInvokeRPC_Response(requester, skipTurn);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckCombatEnd_Request(NetworkConnection requester, int combatId, CombatResult predictedResult)
        {
            if (isDebug) Debug.Log("checking combat end");

            CombatResult actualResult = combatsDictionary[combatId].CheckCombatResult();
            CheckCombatEndRPC_Response(requester, actualResult == predictedResult, actualResult);
            if (actualResult != CombatResult.NotDecided)
            {
                combatsDictionary.Remove(combatId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckMoveRPC_Request(NetworkConnection requester, int combatId, int fighterIndex, CombatMove move)
        {
            bool isMoveValid;
            string message;

            // Try Make Move
            (isMoveValid, message) = combatsDictionary[combatId].CheckIfMoveValid(fighterIndex, true, move);

            if (isDebug) Debug.Log(message);
            if (isMoveValid)
            {
                combatsDictionary[combatId].ApplyMove(fighterIndex, true, move);
                CheckMoveRPC_Response(requester, true);
            }
            else
                CheckMoveRPC_Response(requester, false, message);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckAiMoveRPC_Request(NetworkConnection requester, int combatId, int monsterIndex)
        {
            Fighter fighter = combatsDictionary[combatId]._monsters[monsterIndex];
            CombatMove move = (fighter as MonsterFighter).ChoooseRandomMove(combatsDictionary[combatId]);

            bool isMoveValid;
            string message;
            // Try Make Move
            (isMoveValid, message) = combatsDictionary[combatId].CheckIfMoveValid(monsterIndex, false, move);

            //(bool isMoveValid, string message) = combatsDictionary[combatId].CheckIfMoveValid(monsterIndex, false, move);
            if (isDebug) Debug.Log(message);
            if (isMoveValid)
            {
                combatsDictionary[combatId].ApplyMove(monsterIndex, false, move);
                CheckAiMoveRPC_Response(requester, true, move: move);
            }
            else
                CheckAiMoveRPC_Response(requester, false, message);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckQuickActionRPC_Request(NetworkConnection requester, int combatId, int fighterIndex, CombatQuickActionMove quickActionMove)
        {
            bool isMoveValid;
            string message;

            // Try Make Move
            (isMoveValid, message) = combatsDictionary[combatId].CheckIfQuickActionValid(fighterIndex, true, quickActionMove);

            if (isDebug) Debug.Log(message);
            if (isMoveValid)
            {
                combatsDictionary[combatId].ApplyQuickAction(fighterIndex, true, quickActionMove);
                CheckQuickActionRPC_Response(requester, true);
            }
            else
                CheckQuickActionRPC_Response(requester, false, message);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CheckPassiveInvokeRPC_Request(NetworkConnection requester, int combatId)
        {
            List<CombatPassiveMove> combatPassiveMoves = combatsDictionary[combatId].CheckForPassiveSkills();
            combatsDictionary[combatId].ApplyPassiveMoves(combatPassiveMoves);
            CheckPassiveInvokeRPC_Response(requester, combatPassiveMoves);
        }
        #endregion

        #region ClientRPC
        [TargetRpc(RunLocally = false)]
        private void RequestCombat_Response(NetworkConnection conn, int combatId)
        {
            if (isDebug) Debug.Log($"Starting Battle of Index {combatId}");
            localCombatId = combatId;
            localCombat = InitializeCombat(requestedMonstersList, requestedMonstersPositionsList, requestedHeroStats, requestedHeroWeapon, requestedHeroOffhand);
            localCombat.CurrentCombatState = CombatState.Starting;
            //InitializeField(newCombatId);
            CombatField.Instance.InitializeField(localCombat);
            CombatUI.Instance.ToggleUI(true);

            NextRoundRPC_Request(LocalConnection, localCombatId);
            //SwitchTurns();
            //combatsDictionary[combatId].SwitchFighter();
            //UpdateUiForCurrentFighter();
        }

        [TargetRpc(RunLocally = false)]
        private void NextRoundRPC_Response(NetworkConnection conn)
        {
            if (isDebug) Debug.Log("checking next move");
            //combatsDictionary[localCombatId].SwitchFighter();
            SwitchTurns();
            CheckStatusEffectInvokeRPC_Request(LocalConnection, localCombatId);
        }

        [TargetRpc(RunLocally = false)]
        private void CheckStatusEffectInvokeRPC_Response(NetworkConnection conn, bool skipTurn)
        {
            CheckStatusEffectInvoke(skipTurn);
        }
        private async void CheckStatusEffectInvoke(bool skipTurn)
        {
            localCombat.CurrentCombatState = CombatState.CheckingStatusEffects;

            //await CombatField.Instance.InvokeFighterStats(localCombat.CurrentFighterTurn);
            await Awaitable.WaitForSecondsAsync(1);

            bool confirmSkipTurn = localCombat.ApplyAndCheckCurrentFighterStatusEffects();
            CombatField.Instance.CheckForDeaths();
            CombatUI.Instance.UpdateTurnsBar(localCombat.CurrentFighterTurn, localCombat.TurnList);
            CombatField.Instance.UpdateFieldFightersStatusVisuals();

            bool isCombatEnded = CheckCombatEnd();
            if (isCombatEnded) return;

            if (confirmSkipTurn)
            {
                NextRoundRPC_Request(LocalConnection, localCombatId);
            }
            else
            {
                localCombat.CurrentCombatState = CombatState.DecidingMove;
                if (localCombat.CurrentFighterTurn.IsLocalPlayer)
                {
                    // TODO: Show Education UI And Wait For Response
                    //EducationUIManager.Instance.RequestOneQuestion();
                    //UpdateUiForCurrentFighter();
                }
                else
                {
                    UpdateUiForCurrentFighter();
                    CheckAiMoveRPC_Request(LocalConnection, localCombatId, localCombat.CurrentFighterTurn.Index);
                }
            }
        }

        [TargetRpc(RunLocally = false)]
        private void CheckCombatEndRPC_Response(NetworkConnection conn, bool isConfirmed, CombatResult actualResult)
        {
            if (isDebug) Debug.Log("confirm combat end");
            if (isConfirmed)
            {
                CombatUI.Instance.ShowFinishCombatScreen(actualResult);
                FinishGame();
            }
            else
            {
                if (isDebug) Debug.LogError($"Local Result: {localCombat.CurrentCombatResult}. Doesn't Match Server Result: {actualResult}");
            }
        }

        [TargetRpc(RunLocally = false)]
        private void CheckMoveRPC_Response(NetworkConnection conn, bool isValid, string message = "")
        {
            CheckMove(isValid, message);
        }
        private async void CheckMove(bool isValid, string message = "")
        {
            // Check if Valid
            // Notify False Move
            // Apply Move Locally
            if (isDebug) Debug.Log($"Chosen Move validity is: {isValid}");

            if (isValid)
            {
                localCombat.CurrentCombatState = CombatState.MakingMove;

                Fighter initiatorFighter = localCombat.CurrentFighterTurn;
                Fighter targetedFighter = queuedMove.targetHeroIndex != -1 ? localCombat._heros[queuedMove.targetHeroIndex] : localCombat._monsters[queuedMove.targetMonsterIndex];

                SkillsCollection.SkillInfo chosenSkillInfo = skillsCollection.GetSkillByIdentifier(localCombat.CurrentFighterTurn.SkillNames[queuedMove.skillIndex]);
                List<Fighter> affectedFighters = localCombat.GetAffectedFighters(targetedFighter, chosenSkillInfo.aoe);

                await CombatField.Instance.InvokeFighterMove(initiatorFighter, chosenSkillInfo.identifier, targetedFighter, affectedFighters);

                localCombat.ApplyMove(initiatorFighter.Index, true, queuedMove);

                CombatField.Instance.UpdateFieldFightersStatusVisuals();

                localCombat.CurrentCombatState = CombatState.OnHold;
                await Awaitable.WaitForSecondsAsync(1f);
                //NextRoundRPC_Request(localCombatId);
                CombatField.Instance.CheckForDeaths();
                CombatUI.Instance.UpdateTurnsBar(localCombat.CurrentFighterTurn, localCombat.TurnList);

                bool isCombatEnded = CheckCombatEnd();
                if (isCombatEnded) return;
                CheckPassiveMoves();
            }
            else
            {
                queuedMove = new CombatMove();
                OkMessageBox.Show("Note", $"Error:\n{message}", "Ok", ResetCombat);
            }
        }

        [TargetRpc(RunLocally = false)]
        private void CheckAiMoveRPC_Response(NetworkConnection conn, bool isValid, string message = "", CombatMove? move = null)
        {
            CheckAiMove(isValid, message, move);
        }
        private async void CheckAiMove(bool isValid, string message = "", CombatMove? move = null)
        {
            // Check if Valid
            // Notify False Move
            // Apply Move Locally
            if (isDebug) Debug.Log($"Chosen Ai Move validity is: {isValid}");

            await Awaitable.WaitForSecondsAsync(1f);

            if (isValid)
            {
                localCombat.CurrentCombatState = CombatState.MakingMove;

                Fighter initiatorFighter = localCombat.CurrentFighterTurn;
                Fighter targetedFighter = move.Value.targetHeroIndex != -1 ? localCombat._heros[move.Value.targetHeroIndex] : localCombat._monsters[move.Value.targetMonsterIndex];

                SkillsCollection.SkillInfo chosenSkillInfo = skillsCollection.GetSkillByIdentifier(localCombat.CurrentFighterTurn.SkillNames[move.Value.skillIndex]);
                List<Fighter> affectedFighters = localCombat.GetAffectedFighters(targetedFighter, chosenSkillInfo.aoe);


                await CombatField.Instance.InvokeFighterMove(initiatorFighter, chosenSkillInfo.identifier, targetedFighter, affectedFighters);

                localCombat.ApplyMove(initiatorFighter.Index, false, move.Value);

                CombatField.Instance.UpdateFieldFightersStatusVisuals();

                localCombat.CurrentCombatState = CombatState.OnHold;
                await Awaitable.WaitForSecondsAsync(1f);

                CombatField.Instance.CheckForDeaths();
                CombatUI.Instance.UpdateTurnsBar(localCombat.CurrentFighterTurn, localCombat.TurnList);

                bool isCombatEnded = CheckCombatEnd();
                if (isCombatEnded) return;
                CheckPassiveMoves();
            }
            else
            {
                OkMessageBox.Show("Note", $"Error:\n{message}", "Ok", ResetCombat);
            }
        }

        [TargetRpc(RunLocally = false)]
        private void CheckQuickActionRPC_Response(NetworkConnection conn, bool isValid, string message = "")
        {
            CheckQuickAction(isValid, message);
        }
        private async void CheckQuickAction(bool isValid, string message = "")
        {
            // Check if Valid
            // Notify False Move
            // Apply Move Locally
            if (isDebug) Debug.Log($"Chosen Quick Action validity is: {isValid}");

            if (isValid)
            {
                localCombat.CurrentCombatState = CombatState.MakingMove;

                Fighter initiatorFighter = localCombat.CurrentFighterTurn;
                Fighter targetedFighter = queuedQuickActionMove.targetHeroIndex != -1 ? localCombat._heros[queuedQuickActionMove.targetHeroIndex] : localCombat._monsters[queuedQuickActionMove.targetMonsterIndex];

                QuickActionsCollection.QuickActionInfo chosenQuickActionInfo = quickActionsCollection.GetQuickActionByIdentifier(localCombat.CurrentFighterTurn.QuickActions[queuedQuickActionMove.quickActionIndex].QuickActionName);
                List<Fighter> affectedFighters = localCombat.GetAffectedFighters(targetedFighter, chosenQuickActionInfo.aoe);

                await CombatField.Instance.InvokeFighterQuickAction(initiatorFighter, chosenQuickActionInfo.identifier, targetedFighter, affectedFighters);

                //localCombat.ApplyMove(currentFighter.Index, true, queuedMove);
                localCombat.ApplyQuickAction(initiatorFighter.Index, true, queuedQuickActionMove);

                CombatField.Instance.UpdateFieldFightersStatusVisuals();

                localCombat.CurrentCombatState = CombatState.OnHold;
                await Awaitable.WaitForSecondsAsync(1f);
                //NextRoundRPC_Request(localCombatId);
                CombatField.Instance.CheckForDeaths();
                CombatUI.Instance.UpdateTurnsBar(localCombat.CurrentFighterTurn, localCombat.TurnList);

                bool isCombatEnded = CheckCombatEnd();
                if (isCombatEnded) return;
                localCombat.CurrentCombatState = CombatState.DecidingMove;
                UpdateUiForCurrentFighter();
            }
            else
            {
                queuedMove = new CombatMove();
                OkMessageBox.Show("Note", $"Error:\n{message}", "Ok", ResetCombat);
            }
        }

        [TargetRpc(RunLocally = false)]
        private void CheckPassiveInvokeRPC_Response(NetworkConnection conn, List<CombatPassiveMove> combatPassiveMoves)
        {
            CheckPassiveInvoke(combatPassiveMoves);
        }
        private async void CheckPassiveInvoke(List<CombatPassiveMove> combatPassiveMoves)
        {
            localCombat.CurrentCombatState = CombatState.InvokingPassive;

            //await combatField.InvokeFighterMove(currentFighter, affectedFighters);
            List<(Fighter, int)> targetFighters = new List<(Fighter, int)>();
            foreach (var passiveMove in combatPassiveMoves)
            {
                Fighter initiator = passiveMove.targetHeroIndex >= 0 ? localCombat.GetHero(passiveMove.targetHeroIndex) : localCombat.GetMonster(passiveMove.targetMonsterIndex);
                targetFighters.Add((initiator, passiveMove.passiveSkillIndex));
            }
            await CombatField.Instance.InvokeFightersPassiveMoves(targetFighters);

            localCombat.ApplyPassiveMoves(combatPassiveMoves);
            localCombat.CurrentCombatState = CombatState.OnHold;
            await Awaitable.WaitForSecondsAsync(1f);

            //combatField.CheckForDeaths();
            NextRoundRPC_Request(LocalConnection, localCombatId);
        }
        #endregion

        #region ClientFunctions
        [Client]
        public void TryRequestCombat(List<MonsterName> monstersName, List<CombatFieldPos> monstersPositions, BaseStats heroBaseStats, WeaponName heroWeapon, OffHandName heroOffhand)
        {
            requestedMonstersList = monstersName;
            requestedMonstersPositionsList = monstersPositions;
            requestedHeroStats = heroBaseStats;
            requestedHeroWeapon = heroWeapon;
            requestedHeroOffhand = heroOffhand;
            RequestCombat(LocalConnection, monstersName, monstersPositions, heroBaseStats, heroWeapon, heroOffhand);
            //EducationUIManager.Instance.OnQuestionAnswered += OnEducationQuistionResult_Event;
        }

        [Client]
        public void FinishGame()
        {
            //EducationUIManager.Instance.OnQuestionAnswered -= OnEducationQuistionResult_Event;
        }

        [Client]
        public void TryQuitCombatGame()
        {
            RequestFinishCombat(LocalConnection, localCombatId);
            FinishGame();
        }

        [Client]
        public void SwitchTurns()
        {
            localCombat.SwitchFighter();
            if (isDebug) Debug.Log(localCombat.CurrentFighterTurn.Name);

            CombatField.Instance.MarkCurrentFighter(localCombat.CurrentFighterTurn);
            CombatUI.Instance.UpdateTurnsBar(localCombat.CurrentFighterTurn, localCombat.TurnList);
        }

        [Client]
        public void UpdateUiForCurrentFighter()
        {
            if (localCombat.CurrentFighterTurn.IsLocalPlayer)
                CombatUI.Instance.ShowSkillTab(localCombat.CurrentFighterTurn);
            else
                CombatUI.Instance.HideSkillTab();
        }

        [Client]
        public void RequestMove(int skillIndex)
        {
            // move index is queue until targets are chosen by combat field
            queuedMove = new CombatMove(skillIndex, -1, -1);
            queuedQuickActionMove = new CombatQuickActionMove(-1, -1, -1);
            CombatField.Instance.ShowAffectedTargets(new List<Fighter> { });
            CombatUI.Instance.SwitchConfirmationButton(false);

            SkillsCollection.SkillInfo skillInfo = skillsCollection.GetSkillByIdentifier(localCombat.CurrentFighterTurn.SkillNames[skillIndex]);

            List<Fighter> targetableFighters = localCombat.GetTargetableFighters(skillInfo.targetType);
            CombatField.Instance.ShowPossibleTargets(localCombat.CurrentFighterTurn, targetableFighters);
        }

        [Client]
        public void RequestQuickAction(int QuickActionIndex)
        {
            // move index is queue until targets are chosen by combat field
            queuedQuickActionMove = new CombatQuickActionMove(QuickActionIndex, -1, -1);
            queuedMove = new CombatMove(-1, -1, -1);
            CombatField.Instance.ShowAffectedTargets(new List<Fighter> { });
            CombatUI.Instance.SwitchConfirmationButton(false);

            QuickActionsCollection.QuickActionInfo quickActionInfo = quickActionsCollection.GetQuickActionByIdentifier(localCombat.CurrentFighterTurn.QuickActions[QuickActionIndex].QuickActionName);

            List<Fighter> targetableFighters = localCombat.GetTargetableFighters(quickActionInfo.targetType);
            CombatField.Instance.ShowPossibleTargets(localCombat.CurrentFighterTurn, targetableFighters);
        }

        [Client]
        public void SelectTarget(Fighter targetFighter)
        {
            queuedMove.targetHeroIndex = -1;
            queuedMove.targetMonsterIndex = -1;
            queuedQuickActionMove.targetHeroIndex = -1;
            queuedQuickActionMove.targetMonsterIndex = -1;

            List<Fighter> affectedFighters = new();
            if (queuedMove.skillIndex >= 0)
            {
                SkillsCollection.SkillInfo chosenSkillInfo = skillsCollection.GetSkillByIdentifier(localCombat.CurrentFighterTurn.SkillNames[queuedMove.skillIndex]);
                affectedFighters = localCombat.GetAffectedFighters(targetFighter, chosenSkillInfo.aoe);

                if (localCombat._heros.Contains(targetFighter))
                    queuedMove.targetHeroIndex = localCombat._heros.IndexOf(targetFighter);
                else //if (localCombat._monsters.Contains(targetFighters))
                    queuedMove.targetMonsterIndex = localCombat._monsters.IndexOf(targetFighter);
            }
            else if (queuedQuickActionMove.quickActionIndex >= 0)
            {
                QuickActionsCollection.QuickActionInfo chosenQuickActionInfo = quickActionsCollection.GetQuickActionByIdentifier(localCombat.CurrentFighterTurn.QuickActions[queuedQuickActionMove.quickActionIndex].QuickActionName);
                affectedFighters = localCombat.GetAffectedFighters(targetFighter, chosenQuickActionInfo.aoe);

                if (localCombat._heros.Contains(targetFighter))
                    queuedQuickActionMove.targetHeroIndex = localCombat._heros.IndexOf(targetFighter);
                else //if (localCombat._monsters.Contains(targetFighters))
                    queuedQuickActionMove.targetMonsterIndex = localCombat._monsters.IndexOf(targetFighter);
            }

            CombatField.Instance.ShowAffectedTargets(affectedFighters);
            CombatUI.Instance.SwitchConfirmationButton(true);
        }

        [Client]
        public void ConfirmTarget()
        {
            CombatField.Instance.OnTargetConfirmed();
            CombatUI.Instance.HideSkillTab();
            CombatUI.Instance.SwitchConfirmationButton(false);
            if (queuedMove.skillIndex >= 0) CheckMoveRPC_Request(LocalConnection, localCombatId, localCombat.CurrentFighterTurn.Index, queuedMove);
            else if (queuedQuickActionMove.quickActionIndex >= 0) CheckQuickActionRPC_Request(LocalConnection, localCombatId, localCombat.CurrentFighterTurn.Index, queuedQuickActionMove);
        }


        [Client]
        public bool CheckCombatEnd()
        {
            CombatResult result = localCombat.CheckCombatResult();
            if (result != CombatResult.NotDecided)
            {
                CheckCombatEnd_Request(LocalConnection, localCombatId, result);
                return true;
            }
            return false;
        }

        [Client]
        public void CheckPassiveMoves()
        {
            localCombat.CurrentCombatState = CombatState.CheckingPassives;

            List<CombatPassiveMove> combatPassiveMoves = localCombat.CheckForPassiveSkills();
            if (combatPassiveMoves.Count > 0)
                CheckPassiveInvokeRPC_Request(LocalConnection, localCombatId);
            else
                NextRoundRPC_Request(LocalConnection, localCombatId);
        }

        #endregion

        public void ResetCombat()
        {
            //CombatField.Instance.ResetField();
            //combatUI.ResetUI();
            queuedMove = new CombatMove();
        }

        public void OnEducationQuistionResult_Event(bool isCorrect)
        {
            if (isCorrect)
            {
                // Show Skills UI for player
                OkMessageBox.Show("Notice", "Your Answer is correct\nPlay Turn", "Ok", UpdateUiForCurrentFighter);
                //UpdateUiForCurrentFighter();
            }
            else
            {
                // Skip turn for player
                OkMessageBox.Show("Notice", "Your Answer is incorrect\nSkip Turn", "Ok", () => NextRoundRPC_Request(LocalConnection, localCombatId));
                //NextRoundRPC_Request(LocalConnection, localCombatId);
            }
        }
    }
}
