using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace GP7.Prodigy.Combat
{
    public class CombatField : MonoBehaviour
    {
        public static CombatField Instance { get; private set; }

        [Header("Resources")]
        [SerializeField] SkillsCollection skillsCollection;
        [SerializeField] PassiveSkillsCollection passiveSkillsCollection;
        [SerializeField] QuickActionsCollection quickActionsCollection;

        [Header("References")]
        [SerializeField] Camera referencedCamera;
        [SerializeField] FieldFighter fighterPrefab;
        [SerializeField] Transform heroHolder;
        [SerializeField] Transform monsterHolder;
        [SerializeField] List<CombatFieldCell> heroFieldCells;
        [SerializeField] List<CombatFieldCell> monsterFieldCells;

        private List<FieldFighter> fieldFighters = new List<FieldFighter>();

        public CombatFieldCell GetCellOfPos(CombatFieldPos targetPos)
        {
            return targetPos.isLocal switch
            {
                true => heroFieldCells.FirstOrDefault(c => c.ReservedPos.Equals(targetPos)),
                false => monsterFieldCells.FirstOrDefault(c => c.ReservedPos.Equals(targetPos))
            };
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            ResetField();
        }

        public void InitializeField(Combat combat)
        {
            ResetField();

            for (int i = 0; i < combat._heros.Count; i++)
            {
                FieldFighter fieldHero = Instantiate(fighterPrefab, heroHolder);
                fieldHero.Initialize(this, combat._heros[i], true);

                CombatFieldCell targetFieldCell = GetCellOfPos(combat._heros[i].Position);
                fieldHero.transform.position = targetFieldCell.StandTransform.position;
                //targetFieldCell.FighterReference = fieldHero.FighterReference;
                targetFieldCell.Initialize(referencedCamera, this, fieldHero);

                fieldFighters.Add(fieldHero);
            }

            for (int i = 0; i < combat._monsters.Count; i++)
            {
                FieldFighter fieldMonster = Instantiate(fighterPrefab, monsterHolder);
                fieldMonster.Initialize(this, combat._monsters[i], false);

                CombatFieldCell targetFieldCell = GetCellOfPos(combat._monsters[i].Position);
                fieldMonster.transform.position = targetFieldCell.StandTransform.position;
                //targetFieldCell.FighterReference = fieldMonster.FighterReference;
                targetFieldCell.Initialize(referencedCamera, this, fieldMonster);

                fieldFighters.Add(fieldMonster);
            }
        }

        public void ResetField()
        {
            heroHolder.DestroyAllChildren();
            monsterHolder.DestroyAllChildren();
            fieldFighters.Clear();
        }

        public FieldFighter GetFieldFighterByReference(Fighter fighter) => fieldFighters.FirstOrDefault(f => f.FighterReference == fighter);

        public void MarkCurrentFighter(Fighter currentFighter)
        {
            foreach (var heroFieldCell in heroFieldCells)
            {
                bool condition = heroFieldCell.FieldFighterReference != null
                    && heroFieldCell.FieldFighterReference.FighterReference == currentFighter;
                heroFieldCell.SwitchCurrentTurnMark(condition);
            }
            foreach (var monsterFieldCell in monsterFieldCells)
            {
                bool condition = monsterFieldCell.FieldFighterReference != null
                    && monsterFieldCell.FieldFighterReference.FighterReference == currentFighter;
                monsterFieldCell.SwitchCurrentTurnMark(condition);
            }
        }

        public void ShowPossibleTargets(Fighter initiator, List<Fighter> possibleTargets)
        {
            List<CombatFieldCell> allFieldCells = new List<CombatFieldCell>(monsterFieldCells);
            allFieldCells.AddRange(heroFieldCells);

            //// TODO: check skill target type
            foreach (CombatFieldCell fieldCell in allFieldCells)
            {
                bool condition = fieldCell.FieldFighterReference != null
                    && !fieldCell.FieldFighterReference.FighterReference.IsDead
                    && possibleTargets.Contains(fieldCell.FieldFighterReference.FighterReference);
                fieldCell.SwitchTargetSelectionMode(condition);
            }
        }

        public void ShowAffectedTargets(List<Fighter> affectedTargets)
        {
            List<CombatFieldCell> allFieldCells = new List<CombatFieldCell>(monsterFieldCells);
            allFieldCells.AddRange(heroFieldCells);

            //// TODO: check skill target type
            foreach (CombatFieldCell fieldCell in allFieldCells)
            {
                bool condition = fieldCell.FieldFighterReference != null
                    && !fieldCell.FieldFighterReference.FighterReference.IsDead
                    && affectedTargets.Contains(fieldCell.FieldFighterReference.FighterReference);
                fieldCell.SwitchAffectedMark(condition);
                //fieldCell.SwitchTargetSelectionMode(condition);
            }
        }

        public void SelectTarget(FieldFighter target)
        {
            OfflineCombatManager.Instance.SelectTarget(target.FighterReference);
        }

        public void OnTargetConfirmed()
        {
            heroFieldCells.ForEach(c => c.UnMark());
            monsterFieldCells.ForEach(c => c.UnMark());
        }
        private SkillName tempSelectedSkill;
        private QuickActionName tempSelectedQuickAction;
        private Fighter tempTargetedFighter;
        private List<Fighter> tempAffectedFighters;

        public async Task InvokeFighterMove(Fighter initiatorFighter, SkillName selectedSkill, Fighter targetedFighter, List<Fighter> affectedFighters)
        {
            FieldFighter initiator = GetFieldFighterByReference(initiatorFighter);
            this.tempSelectedSkill = selectedSkill;
            this.tempTargetedFighter = targetedFighter;
            this.tempAffectedFighters = affectedFighters;
            await initiator.MakeMove();
        }

        public async Task InvokeFighterQuickAction(Fighter initiatorFighter, QuickActionName selectedQuickAction, Fighter targetedFighter, List<Fighter> affectedFighters)
        {
            FieldFighter initiator = GetFieldFighterByReference(initiatorFighter);
            this.tempSelectedQuickAction = selectedQuickAction;
            this.tempTargetedFighter = targetedFighter;
            this.tempAffectedFighters = affectedFighters;
            await initiator.MakeQuickAction();
        }

        public async Task InvokeFightersPassiveMoves(List<(Fighter, int)> invokedFighters)
        {
            foreach (var invokedFighter in invokedFighters)
            {
                Fighter fighter = invokedFighter.Item1;
                int passiveSkillIndex = invokedFighter.Item2;
                PassiveSkillsCollection.PassiveSkillInfo passiveSkillInfo = passiveSkillsCollection.GetPassiveSkillByIdentifier(fighter.PassiveSkills[passiveSkillIndex].PassiveSkillName);

                FieldFighter initiator = GetFieldFighterByReference(invokedFighter.Item1);
                if (passiveSkillInfo.skillEffect != null)
                {
                    ParticleSystem instantiatedParticle = Instantiate(passiveSkillInfo.skillEffect, initiator.transform.position, Quaternion.identity);
                    if (initiator.FighterReference.IsLocalPlayer) instantiatedParticle.transform.localScale = new Vector3(-1, 1, 1);
                }
                initiator.InvokePassive();
            }
            await Awaitable.WaitForSecondsAsync(2);
        }

        public void InvokeFightersHitBySkill()
        {
            ParticleSystem skillEffectParticle = skillsCollection.GetSkillByIdentifier(tempSelectedSkill).skillEffect;
            for (int i = 0; i < tempAffectedFighters.Count; i++)
            {
                FieldFighter target = GetFieldFighterByReference(tempAffectedFighters[i]);
                if (skillEffectParticle != null)
                {
                    ParticleSystem instantiatedParticle = Instantiate(skillEffectParticle, target.transform.position, Quaternion.identity);
                    if (target.FighterReference.IsLocalPlayer) instantiatedParticle.transform.localScale = new Vector3(-1, 1, 1);
                }

                if (skillsCollection.GetSkillByIdentifier(tempSelectedSkill).IsDamageType()) target.TakeHit();
            }
            tempSelectedSkill = SkillName.None;
            tempTargetedFighter = null;
            tempAffectedFighters.Clear();
        }

        public void InvokeFightersHitByQuickAction()
        {
            QuickActionsCollection.QuickActionInfo quickActionInfo = quickActionsCollection.GetQuickActionByIdentifier(tempSelectedQuickAction);
            ParticleSystem quickActionEffectParticle = quickActionInfo.skillEffect;
            for (int i = 0; i < tempAffectedFighters.Count; i++)
            {
                FieldFighter target = GetFieldFighterByReference(tempAffectedFighters[i]);
                if (quickActionEffectParticle != null)
                {
                    ParticleSystem instantiatedParticle = Instantiate(quickActionEffectParticle, target.transform.position, Quaternion.identity);
                    if (target.FighterReference.IsLocalPlayer) instantiatedParticle.transform.localScale = new Vector3(-1, 1, 1);
                }

                if (quickActionInfo.IsDamageType()) target.TakeHit();
            }
            tempSelectedQuickAction = QuickActionName.None;
            tempTargetedFighter = null;
            tempAffectedFighters.Clear();
        }

        public void UpdateFieldFightersStatusVisuals()
        {
            for (int i = 0; i < fieldFighters.Count; i++)
            {
                fieldFighters[i].UpdateStatsEffectVisuals();
            }
        }

        public void CheckForDeaths()
        {
            foreach (var fighter in fieldFighters)
            {
                fighter.CheckDeathCondition();
            }
        }
    }
}
