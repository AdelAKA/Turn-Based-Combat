using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GP7.Prodigy.Combat
{
    public class FieldFigterCanvas : MonoBehaviour
    {
        [Serializable]
        struct StatsIconTypePair
        {
            public StatusEffectName statsEffectName;
            public GameObject iconGameObject;
        }

        [SerializeField] Canvas uiCanvas;
        [Header("Info")]
        [SerializeField] TMP_Text nameTag;
        [SerializeField] Image portrait;
        [Header("Health Bar")]
        [SerializeField] Image healthBar;
        [SerializeField] TMP_Text healthText;
        [Header("Mana Bar")]
        [SerializeField] Image manaBar;
        [SerializeField] TMP_Text manaText;
        [Header("Mana Bar")]
        [SerializeField] List<StatsIconTypePair> statsIcons;
        [Header("Options")]
        //[SerializeField] Sprite heroHealthBarSprite;
        //[SerializeField] Sprite enemyHealthBarSprite;

        [SerializeField] Color heroHealthBarColor;
        [SerializeField] Color enemyHealthBarColor;

        private float _maxHealth;
        private float _maxMana;

        public void Initialize(Fighter fighterReference)
        {
            nameTag.text = fighterReference.Name;

            _maxHealth = fighterReference.MaxHealth;
            _maxMana = fighterReference.MaxMana;

            healthBar.color = fighterReference.IsLocalPlayer ? heroHealthBarColor : enemyHealthBarColor;

            healthBar.fillAmount = fighterReference.CurrentHealth;
            manaBar.fillAmount = fighterReference.CurrentMana;
            healthText.text = $"{(int)fighterReference.MaxHealth}/ {(int)fighterReference.MaxHealth}";
            manaText.text = $"{(int)fighterReference.MaxMana}/ {(int)fighterReference.MaxMana}";

            ResetStatusIcons();
        }

        public void UpdateHealth(float newHealth)
        {
            float newFillAmount = (float)newHealth / (float)_maxHealth;
            //healthBar.DOFillAmount(newFillAmount, 0.5f).SetEase(Ease.OutCirc);
            healthBar.fillAmount = newFillAmount;
            healthText.text = $"{(int)newHealth}/ {(int)_maxHealth}";

        }

        public void UpdateMana(float newMana)
        {
            float newFillAmount = (float)newMana / (float)_maxMana;
            //manaBar.DOFillAmount(newFillAmount, 0.5f).SetEase(Ease.OutCirc);
            manaBar.fillAmount = newFillAmount;
            manaText.text = $"{(int)newMana}/ {(int)_maxMana}";
        }

        private void ResetStatusIcons()
        {
            foreach (var statsIcon in statsIcons)
            {
                statsIcon.iconGameObject.SetActive(false);
            }
        }

        public void UpdateStats(Dictionary<StatusEffectName, float> appliedStatsDict)
        {
            ResetStatusIcons();

            foreach (var appliedStats in appliedStatsDict)
            {
                StatsIconTypePair target = statsIcons.FirstOrDefault(s => s.statsEffectName == appliedStats.Key);
                target.iconGameObject.SetActive(true);
                target.iconGameObject.GetComponentInChildren<TMP_Text>().text = appliedStats.Value.ToString();
            }
        }

        public void Hide()
        {
            uiCanvas.enabled = false;
        }
    }
}
