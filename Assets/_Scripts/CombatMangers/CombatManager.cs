//using FishNet.Object;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Rendering;

//namespace GP7.Prodigy.Combat
//{
//    public abstract class CombatManager : MonoBehaviour
//    {
//        public static CombatManager Instance { get; protected set; }

//        [Header("Resources")]
//        public WeaponsCollection weaponsCollection;
//        public SkillsCollection skillsCollection;
//        public MonstersCollection monstersCollection;
//        public PassiveSkillsCollection passiveSkillsCollection;
//        public OffHandsCollection offHandsCollection;
//        public QuickActionsCollection quickActionsCollection;

//        [Header("References")]
//        [SerializeField] protected CombatField combatField;
//        [SerializeField] protected CombatUI combatUI;

//        [Header("Testing")]
//        [SerializeField] protected bool isDebug;

//        // Server Variables
//        protected SerializedDictionary<int, Combat> combatsDictionary = new SerializedDictionary<int, Combat>();
//        protected int lastCombatId = 0;

//        // Client Variables
//        protected Combat localCombat;
//        protected int localCombatId;
//        protected CombatMove queuedMove;
//        protected CombatQuickActionMove queuedQuickActionMove;

//        protected List<MonsterName> requestedMonstersList;
//        protected List<CombatFieldPos> requestedMonstersPositionsList;
//        protected WeaponName requestedHeroWeapon;
//        protected OffHandName requestedHeroOffhand;

//        // Properties
//        public CombatState CurrentCombatState => localCombat != null ? localCombat.CurrentCombatState : CombatState.NotStarted;

//        //private void Update()
//        //{
//        //    if (Input.GetKeyDown(KeyCode.Space))
//        //        RequestCombat(LocalConnection, new List<MonsterName>() { MonsterName.Death, MonsterName.Death, MonsterName.Witch });
//        //}

//        public abstract void TryRequestCombat(List<MonsterName> monstersName, List<CombatFieldPos> monstersPositions, WeaponName heroWeapon, OffHandName heroOffhand);
//        public Combat InitializeCombat(List<MonsterName> monstersNames, List<CombatFieldPos> monstersPositions, WeaponName heroWeapon, OffHandName heroOffhand)
//        {
//            Fighter hero;
//            {
//                float maxHP = 300;
//                float maxMP = 7;

//                WeaponsCollection.WeaponInfo heroWeaponInfo = weaponsCollection.GetWeaponByIdentifier(heroWeapon);
//                List<SkillName> skillNames = new List<SkillName>(heroWeaponInfo.skillNames);

//                List<PassiveSkillsCollection.PassiveSkillInfo> passiveSkills = new List<PassiveSkillsCollection.PassiveSkillInfo>();
//                if (heroOffhand != OffHandName.None)
//                {
//                    OffHandsCollection.OffhandInfo offHandInfo = offHandsCollection.GetOffHandByIdentifier(heroOffhand);
//                    maxHP += offHandInfo.extraHealth;
//                    maxMP += offHandInfo.extraMana;

//                    if (offHandInfo.skillName != SkillName.None)
//                    {
//                        skillNames.Add(offHandInfo.skillName);
//                    }
//                    if (offHandInfo.passiveSkillName != PassiveSkillName.None)
//                    {
//                        PassiveSkillsCollection.PassiveSkillInfo passiveSkillInfo = passiveSkillsCollection.GetPassiveSkillByIdentifier(offHandInfo.passiveSkillName);
//                        passiveSkills.Add(passiveSkillInfo);
//                    }
//                }

//                QuickActionsCollection.QuickActionInfo rock = quickActionsCollection.GetQuickActionByIdentifier(QuickActionName.Rock);
//                QuickActionsCollection.QuickActionInfo healPotion = quickActionsCollection.GetQuickActionByIdentifier(QuickActionName.healingPotion);
//                QuickActionsCollection.QuickActionInfo damageBoost = quickActionsCollection.GetQuickActionByIdentifier(QuickActionName.DamgeBoostPotion);

//                hero = new HeroFighter(
//                        index: 0,
//                        new CombatFieldPos() { lineIndex = 1, isFront = true, isLocal = true },
//                        isLocalPlayer: true,
//                        maxHP: maxHP,
//                        maxMP: maxMP,
//                        speed: 10,
//                        damage: 50 + heroWeaponInfo.damage,
//                        offHandStats: 50,
//                        heldWeapon: heroWeapon,
//                        heldOffHand: heroOffhand,
//                        skillNames: skillNames,
//                        passiveSkillInfoList: passiveSkills,
//                        quickActionInfoList: new() { rock, healPotion, damageBoost }
//                    );
//            }

//            List<Fighter> monsters = new List<Fighter>();
//            for (int i = 0; i < monstersNames.Count; i++)
//            {
//                MonstersCollection.MonsterInfo monsterInfo = monstersCollection.GetMonsterInfoByName(monstersNames[i]);
//                if (monsterInfo == null) continue;
//                WeaponsCollection.WeaponInfo monsterWeaponInfo = weaponsCollection.GetWeaponByIdentifier(monsterInfo.heldWeapon);
//                PassiveSkillsCollection.PassiveSkillInfo passiveSkillInfo = passiveSkillsCollection.GetPassiveSkillByIdentifier(PassiveSkillName.healPercentage);

//                Fighter monster = new MonsterFighter(
//                    monsterName: monsterInfo.name,
//                    index: i,
//                    monstersPositions[i],
//                    isLocalPlayer: false,
//                    maxHP: monsterInfo.hP,
//                    maxMP: monsterInfo.mP,
//                    speed: monsterInfo.speed,
//                    damage: monsterWeaponInfo.damage,
//                    offHandStats: 0,
//                    heldWeapon: monsterInfo.heldWeapon,
//                    heldOffHand: OffHandName.None,
//                    skillNames: monsterWeaponInfo.skillNames,
//                    passiveSkillInfoList: new() { passiveSkillInfo },
//                    quickActionInfoList: new() { }
//                );
//                monsters.Add(monster);
//            }
//            Combat newCombat = new Combat(new List<Fighter>() { hero }, monsters);
//            return newCombat;
//        }

//        public void SwitchTurns()
//        {
//            localCombat.SwitchFighter();
//            if (isDebug) Debug.Log(localCombat.CurrentFighterTurn.Name);

//            combatField.MarkCurrentFighter(localCombat.CurrentFighterTurn);
//            combatUI.UpdateTurnsBar(localCombat.CurrentFighterTurn, localCombat.TurnList);
//        }

//        public void UpdateUiForCurrentFighter()
//        {
//            if (localCombat.CurrentFighterTurn.IsLocalPlayer)
//                combatUI.ShowSkillTab(localCombat.CurrentFighterTurn);
//            else
//                combatUI.HideSkillTab();
//        }

//        public void RequestMove(int skillIndex)
//        {
//            // move index is queue until targets are chosen by combat field
//            queuedMove = new CombatMove(skillIndex, -1, -1);
//            queuedQuickActionMove = new CombatQuickActionMove(-1, -1, -1);
//            combatField.ShowAffectedTargets(new List<Fighter> { });
//            combatUI.SwitchConfirmationButton(false);

//            SkillsCollection.SkillInfo skillInfo = skillsCollection.GetSkillByIdentifier(localCombat.CurrentFighterTurn.SkillNames[skillIndex]);

//            List<Fighter> targetableFighters = localCombat.GetTargetableFighters(skillInfo.targetType);
//            combatField.ShowPossibleTargets(localCombat.CurrentFighterTurn, targetableFighters);
//        }

//        public void RequestQuickAction(int QuickActionIndex)
//        {
//            // move index is queue until targets are chosen by combat field
//            queuedQuickActionMove = new CombatQuickActionMove(QuickActionIndex, -1, -1);
//            queuedMove = new CombatMove(-1, -1, -1);
//            combatField.ShowAffectedTargets(new List<Fighter> { });
//            combatUI.SwitchConfirmationButton(false);

//            QuickActionsCollection.QuickActionInfo quickActionInfo = quickActionsCollection.GetQuickActionByIdentifier(localCombat.CurrentFighterTurn.QuickActions[QuickActionIndex].QuickActionName);

//            List<Fighter> targetableFighters = localCombat.GetTargetableFighters(quickActionInfo.targetType);
//            combatField.ShowPossibleTargets(localCombat.CurrentFighterTurn, targetableFighters);
//        }

//        public void SelectTarget(Fighter targetFighter)
//        {
//            queuedMove.targetHeroIndex = -1;
//            queuedMove.targetMonsterIndex = -1;
//            queuedQuickActionMove.targetHeroIndex = -1;
//            queuedQuickActionMove.targetMonsterIndex = -1;

//            List<Fighter> affectedFighters = new();
//            if (queuedMove.skillIndex >= 0)
//            {
//                SkillsCollection.SkillInfo chosenSkillInfo = skillsCollection.GetSkillByIdentifier(localCombat.CurrentFighterTurn.SkillNames[queuedMove.skillIndex]);
//                affectedFighters = localCombat.GetAffectedFighters(targetFighter, chosenSkillInfo.aoe);

//                if (localCombat._heros.Contains(targetFighter))
//                    queuedMove.targetHeroIndex = localCombat._heros.IndexOf(targetFighter);
//                else //if (localCombat._monsters.Contains(targetFighters))
//                    queuedMove.targetMonsterIndex = localCombat._monsters.IndexOf(targetFighter);
//            }
//            else if (queuedQuickActionMove.quickActionIndex >= 0)
//            {
//                QuickActionsCollection.QuickActionInfo chosenQuickActionInfo = quickActionsCollection.GetQuickActionByIdentifier(localCombat.CurrentFighterTurn.QuickActions[queuedQuickActionMove.quickActionIndex].QuickActionName);
//                affectedFighters = localCombat.GetAffectedFighters(targetFighter, chosenQuickActionInfo.aoe);

//                if (localCombat._heros.Contains(targetFighter))
//                    queuedQuickActionMove.targetHeroIndex = localCombat._heros.IndexOf(targetFighter);
//                else //if (localCombat._monsters.Contains(targetFighters))
//                    queuedQuickActionMove.targetMonsterIndex = localCombat._monsters.IndexOf(targetFighter);
//            }

//            combatField.ShowAffectedTargets(affectedFighters);
//            combatUI.SwitchConfirmationButton(true);
//        }

//        public abstract void ConfirmTarget();

//        public abstract bool CheckCombatEnd();

//        public abstract void CheckPassiveMoves();

//        public void ResetCombat()
//        {
//            combatField.ResetField();
//            combatUI.ResetUI();
//            queuedMove = new CombatMove();
//        }

//        //[ServerRpc(RequireOwnership = false)]
//        //public void RequestCombat(NetworkConnection requester, List<MonsterName> monstersName)
//        //{
//        //    lastCombatId++;
//        //    InitializeCombat(requester, monstersName);
//        //    ReturnBattleInfo(requester, lastCombatId);
//        //}

//        //[TargetRpc(RunLocally = false)]
//        //public void ReturnBattleInfo(NetworkConnection conn, int combatId)
//        //{
//        //    Debug.Log($"Starting Battle of Index {combatId}");
//        //}

//        //[ServerRpc(RequireOwnership = false)]
//        //public void RequestMove(NetworkConnection requester, int fighterIndex)
//        //{
//        //    // Try Make Move
//        //    //ConfirmMove(requester);
//        //}

//        //[TargetRpc(RunLocally = false)]
//        //public void ConfirmMove(NetworkConnection conn, bool isValid)
//        //{
//        //    // Check Move
//        //}
//    }
//}
