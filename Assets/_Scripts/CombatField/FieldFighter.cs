using GameKit.Dependencies.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GP7.Prodigy.Combat
{
    public class FieldFighter : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private MonstersCollection monstersCollection;

        [Header("Visuals")]
        [SerializeField] Collider col;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite heroSprite;
        //[SerializeField] SpriteRenderer turnMark;

        [Header("Canvas")]
        [SerializeField] FieldFigterCanvas uiCanvas;

        [Header("Options")]
        [SerializeField] Vector3 originalVisualPos;
        
        [SerializeField] bool isDebug;

        public Fighter FighterReference { get; private set; }
        private bool isFacingRight;
        private bool isDeathChecked;

        private CombatField _combatFieldReference;

        public void Initialize(CombatField combatFieldReference, Fighter fighterReference, bool lookRight)
        {
            _combatFieldReference = combatFieldReference;
            FighterReference = fighterReference;
            if (FighterReference.IsLocalPlayer)
                spriteRenderer.sprite = heroSprite;
            else
                spriteRenderer.sprite = monstersCollection.GetMonsterInfoByName((fighterReference as MonsterFighter).MonsterNameReference).monsterSprite;

            if (!lookRight)
                spriteRenderer.transform.localScale = spriteRenderer.transform.localScale.Multiply(new Vector3(-1, 1, 1));

            uiCanvas.Initialize(fighterReference);

            FighterReference.OnHealthChanged += OnHealthChanged;
            FighterReference.OnManaChanged += OnManaChanged;
            FighterReference.OnStatusEffectApplied += OnStatusEffectApplied;
            FighterReference.OnStatusEffectInvoked += OnStatusEffectInvoked;

            isFacingRight = lookRight;

            //SwitchTargetSelectionMode(false);
            //SwitchFighterTurnMark(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //if (!selectionModeArrow.enabled) return;
            //ChooseAsTarget();
        }

        private void OnHealthChanged(float newHealth, float healthChangeAmount)
        {
            //float newFillAmount = (float)newHealth / (float)FighterReference.MaxHealth;
            uiCanvas.UpdateHealth(newHealth);

            if(isDebug) Debug.Log($"healthUpdated: {(float)newHealth / (float)FighterReference.MaxHealth}");
            QueuedNotificationManager.Instance.QueueNotification(
                "HP " + healthChangeAmount.ToString(),
                transform,
                Vector3.up * 2,
                Vector3.one * 0.3f,
                healthChangeAmount < 0 ? Color.red : Color.green
                );
        }

        private void OnManaChanged(float newMana, float manaChangeAmount)
        {
            uiCanvas.UpdateMana(newMana);

            if (isDebug) Debug.Log($"ManaUpdated: {(float)newMana / (float)FighterReference.MaxMana}");
            QueuedNotificationManager.Instance.QueueNotification(
                "MP " + manaChangeAmount.ToString(),
                transform,
                Vector3.up * 2,
                Vector3.one * 0.3f,
                Color.white
                );
        }

        private void OnStatusEffectApplied(StatusEffectName statsEffect)
        {
            Color textColor = statsEffect switch
            {
                StatusEffectName.Poison => Color.green,
                StatusEffectName.Burn => new Color(1, 0.5f, 0),
                StatusEffectName.DamageBoost => new Color(0, 0.5f, 1),
                _ => Color.white
            };
            QueuedNotificationManager.Instance.QueueNotification(
               $"{statsEffect}",
                transform,
                Vector3.up * 2,
                Vector3.one * 0.3f,
                textColor
                );
        }

        private void OnStatusEffectInvoked(StatusEffectName statsEffect, float amount)
        {
            uiCanvas.UpdateHealth(FighterReference.CurrentHealth);

            if (isDebug) Debug.Log($"statsApplied: {(float)FighterReference.CurrentHealth / (float)FighterReference.MaxHealth}");
            Color textColor = statsEffect switch
            {
                StatusEffectName.Poison => Color.green,
                StatusEffectName.Burn => new Color(1, 0.5f, 0),
                StatusEffectName.Stun => Color.gray,
                _ => Color.white
            };
            QueuedNotificationManager.Instance.QueueNotification(
                $"{statsEffect} {amount}",
                transform,
                Vector3.up * 2,
                Vector3.one * 0.3f,
                textColor
                );
        }

        public void UpdateStatsEffectVisuals()
        {
            List<StatusEffectHandler> statsEffects = FighterReference.StatusEffects;
            Dictionary<StatusEffectName, float> statusEffectsDict = new Dictionary<StatusEffectName, float>();

            for (int i = 0; i < statsEffects.Count; i++)
            {
                if (statsEffects[i].StatusEffectName == StatusEffectName.Poison)
                {
                    if (!statusEffectsDict.ContainsKey(StatusEffectName.Poison)) statusEffectsDict.Add(StatusEffectName.Poison, 0);
                    statusEffectsDict[StatusEffectName.Poison] += statsEffects[i].Value;
                }
                if (statsEffects[i].StatusEffectName == StatusEffectName.Burn)
                {
                    if (!statusEffectsDict.ContainsKey(StatusEffectName.Burn)) statusEffectsDict.Add(StatusEffectName.Burn, 0);
                    statusEffectsDict[StatusEffectName.Burn] += statsEffects[i].Value;
                }
                if (statsEffects[i].StatusEffectName == StatusEffectName.DamageBoost)
                {
                    if (!statusEffectsDict.ContainsKey(StatusEffectName.DamageBoost)) statusEffectsDict.Add(StatusEffectName.DamageBoost, 1);
                    statusEffectsDict[StatusEffectName.DamageBoost] += statsEffects[i].Value;
                }
                if (statsEffects[i].StatusEffectName == StatusEffectName.Stun)
                {
                    if (!statusEffectsDict.ContainsKey(StatusEffectName.Stun)) statusEffectsDict.Add(StatusEffectName.Stun, 0);
                    statusEffectsDict[StatusEffectName.Stun] += statsEffects[i].TurnCounter;
                }
            }

            uiCanvas.UpdateStats(statusEffectsDict);
        }

        public async Task MakeMove()
        {
            Vector3 newPos = spriteRenderer.transform.localPosition;
            //Debug.Log(newPos);
            if (isFacingRight)
                newPos.x += 1;
            else
                newPos.x -= 1;
            //newPos.x += isFacingRight ? 1 : -1;
            //Debug.Log(newPos);
            spriteRenderer.transform.DOLocalMoveX(newPos.x, 0.5f).SetEase(Ease.OutCirc)
                .OnComplete(() =>
                {
                    _combatFieldReference.InvokeFightersHitBySkill();
                    spriteRenderer.transform.DOLocalMoveX(originalVisualPos.x, 0.2f).SetEase(Ease.OutCirc);
                });
            await Awaitable.WaitForSecondsAsync(2f); // Animation Time

            return;
        }

        public async Task MakeQuickAction()
        {
            Vector3 newPos = spriteRenderer.transform.localPosition;
            //Debug.Log(newPos);
            if (isFacingRight)
                newPos.x += 1;
            else
                newPos.x -= 1;
            //newPos.x += isFacingRight ? 1 : -1;
            //Debug.Log(newPos);
            spriteRenderer.transform.DOLocalMoveX(newPos.x, 0.5f).SetEase(Ease.OutCirc)
                .OnComplete(() =>
                {
                    _combatFieldReference.InvokeFightersHitByQuickAction();
                    spriteRenderer.transform.DOLocalMoveX(originalVisualPos.x, 0.2f).SetEase(Ease.OutCirc);
                });
            await Awaitable.WaitForSecondsAsync(2f); // Animation Time

            return;
        }

        public void InvokePassive()
        {
            Vector3 newPos = spriteRenderer.transform.localPosition;
            newPos.y += 1f;
            spriteRenderer.transform.DOLocalMoveY(newPos.y, 0.5f).SetEase(Ease.OutCirc)
                .OnComplete(() =>
                {
                    spriteRenderer.transform.DOLocalMoveY(originalVisualPos.y, 0.2f).SetEase(Ease.OutCirc);
                });
        }

        public void TakeHit()
        {
            // Play hit animation
            spriteRenderer.transform.DOShakePosition(0.5f, 0.2f, 10);
        }

        public void CheckDeathCondition()
        {
            if (isDeathChecked) return;
            if (FighterReference.IsDead)
            {
                isDeathChecked = true;
                spriteRenderer.color = Color.red;
                uiCanvas.Hide();
            }
        }
    }
}
