using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GP7.Prodigy.Combat
{
    public class CombatUI : MonoBehaviour
    {
        public static CombatUI Instance { get; private set; }

        [Header("Resources")]
        [SerializeField] MonstersCollection monstersCollection;
        [SerializeField] SkillsCollection skillsCollection;

        [Header("Info")]
        [SerializeField] private TMP_Text combatState;

        [Header("Skills Tab")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private GameObject skillsTab;
        [SerializeField] private List<SkillButton> skillsButtons;
        [SerializeField] private List<SkillButton> quickActionsButtons;

        [Header("Skill Info Tab")]
        [SerializeField] private GameObject skillinfoContainer;
        [SerializeField] private Image skillicon;
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text skillInfoText;

        [Header("Turns Tab")]
        [SerializeField] private CharacterPortrait currentFighterIconHolder;
        [SerializeField] private Transform iconsContainer;
        [SerializeField] private CharacterPortrait iconPrefab;

        [Header("Canvases")]
        [SerializeField] private EndGameUI endScreenUI;
        [SerializeField] private PauseGameUI pauseGameUI;
        [SerializeField] private Button pauseGameButton;

        [Header("Testing")]
        [SerializeField] private Sprite heroSprite;

        [SerializeField] private bool isDebug;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            ResetUI();
            endScreenUI.Hide();
            pauseGameUI.Initialize(() => OfflineCombatManager.Instance.TryQuitCombatGame());
            pauseGameUI.Hide();
            pauseGameButton.onClick.AddListener(PauseGame);
            iconsContainer.DestroyAllChildren();
            currentFighterIconHolder.enabled = false;
            confirmButton.onClick.AddListener(() => OfflineCombatManager.Instance.ConfirmTarget());
        }

        private void Update()
        {
            combatState.text = OfflineCombatManager.Instance.CurrentCombatState.ToString();
        }

        public void ToggleUI(bool isEnabled) => GetComponent<Canvas>().enabled = isEnabled;

        public void ResetUI()
        {
            ToggleUI(false);
            ResetSkillButtons();
            SwitchConfirmationButton(false);
            HideSkillTab();
            HideFinishCombatScreen();
            ResetTurnsBar();
        }

        public void ResetSkillButtons()
        {
            for (int i = 0; i < skillsButtons.Count; i++)
            {
                skillsButtons[i].ResetButton();
            }
            for (int i = 0; i < quickActionsButtons.Count; i++)
            {
                quickActionsButtons[i].ResetButton();
            }
        }

        public void InvokeButtonSelected(SkillButton selectedButton)
        {
            for (int i = 0; i < skillsButtons.Count; i++)
            {
                if (skillsButtons[i] == selectedButton)
                {
                    skillsButtons[i].MarkAsSelected();
                    if (selectedButton.VisualInfo != null)
                    {
                        skillicon.sprite = selectedButton.VisualInfo.icon;
                        skillNameText.text = selectedButton.VisualInfo.name;
                        skillInfoText.text = selectedButton.VisualInfo.description;
                        skillinfoContainer.SetActive(true);
                    }
                }
                else skillsButtons[i].MarkAsDeselected();
            }
            for (int i = 0; i < quickActionsButtons.Count; i++)
            {
                if (quickActionsButtons[i] == selectedButton)
                {
                    quickActionsButtons[i].MarkAsSelected();
                    if (selectedButton.VisualInfo != null)
                    {
                        skillicon.sprite = selectedButton.VisualInfo.icon;
                        skillNameText.text = selectedButton.VisualInfo.name;
                        skillInfoText.text = selectedButton.VisualInfo.description;
                        //skillinfoContainer.SetActive(true);
                        //skillinfoContainer.SetActive(false);
                        skillinfoContainer.SetActive(true);
                    }
                }
                else quickActionsButtons[i].MarkAsDeselected();
            }
        }

        public void UpdateTurnsBar(Fighter currentFighter, List<Fighter> fighters)
        {
            iconsContainer.DestroyAllChildren();

            currentFighterIconHolder.enabled = true;
            if (currentFighter.IsLocalPlayer)
                currentFighterIconHolder.SetSprite(heroSprite);
            else
                currentFighterIconHolder.SetSprite(monstersCollection.GetMonsterInfoByName((currentFighter as MonsterFighter).MonsterNameReference).monsterSprite);

            for (int i = 0; i < fighters.Count - 1; i++) // -1 because the current player is dequeued at the end
            {
                if (fighters[i].IsDead) continue;
                CharacterPortrait newIcon = Instantiate(iconPrefab, iconsContainer);
                if (fighters[i].IsLocalPlayer)
                    newIcon.SetSprite(heroSprite);
                else
                    newIcon.SetSprite(monstersCollection.GetMonsterInfoByName((fighters[i] as MonsterFighter).MonsterNameReference).monsterSprite);
            }
        }

        public void ResetTurnsBar()
        {
            iconsContainer.DestroyAllChildren();
            currentFighterIconHolder.SetSprite(null);
        }

        public void ShowSkillTab(Fighter targetFighter)
        {
            SwitchConfirmationButton(false);
            ResetSkillButtons();
            skillinfoContainer.SetActive(false);

            UnityAction action = null;
            for (int i = 0; i < targetFighter.SkillNames.Count; i++)
            {
                SkillsCollection.SkillInfo skillInfo = OfflineCombatManager.Instance.skillsCollection.GetSkillByIdentifier(targetFighter.SkillNames[i]);
                int skillIndex = i;
                action = null;
                if (targetFighter.CurrentMana >= skillInfo.requiredMP)
                    action = () =>
                    {
                        if (isDebug) Debug.Log(skillIndex);
                        OfflineCombatManager.Instance.RequestMove(skillIndex);
                    };
                SkillButtonVisualInfos visualInfo = new SkillButtonVisualInfos()
                {
                    icon = skillInfo.icon,
                    name = skillInfo.name,
                    number = skillInfo.requiredMP.ToString(),
                    description = skillInfo.SummarizeSkillInfo()
                };
                skillsButtons[i].Initialize(this, visualInfo, action);
            }
            for (int i = 0; i < targetFighter.QuickActions.Count; i++)
            {
                QuickActionsCollection.QuickActionInfo quickActionInfo = OfflineCombatManager.Instance.quickActionsCollection.GetQuickActionByIdentifier(targetFighter.QuickActions[i].QuickActionName);
                int skillIndex = i;
                action = null;
                if (!targetFighter.IsQuickActionUsed && targetFighter.QuickActions[i].ItemCount > 0)
                    action = () =>
                    {
                        if (isDebug) Debug.Log(skillIndex);
                        OfflineCombatManager.Instance.RequestQuickAction(skillIndex);
                    };
                SkillButtonVisualInfos visualInfo = new SkillButtonVisualInfos()
                {
                    icon = quickActionInfo.icon,
                    name = quickActionInfo.name,
                    number = targetFighter.QuickActions[i].ItemCount.ToString(),
                    description = quickActionInfo.SummarizeSkillInfo()
                };
                quickActionsButtons[i].Initialize(this, visualInfo, action);
            }
            skillsTab.SetActive(true);
        }

        public void HideSkillTab()
        {
            skillsTab.SetActive(false);
        }

        public void SwitchConfirmationButton(bool isEnable)
        {
            confirmButton.gameObject.SetActive(isEnable);
        }

        public void ShowFinishCombatScreen(CombatResult result)
        {
            endScreenUI.Show($"You {result}!");
        }
        public void HideFinishCombatScreen() => endScreenUI.Hide();

        public void PauseGame()
        {
            if (OfflineCombatManager.Instance.CurrentCombatResult == CombatResult.NotDecided)
            {
                pauseGameUI.Show();
            }
        }
    }
}
