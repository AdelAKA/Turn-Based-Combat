using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;
using UnityEngine.UI;

namespace GP7.Prodigy.Combat
{
    public class StartScreen : MonoBehaviour
    {
        [SerializeField] List<MonsterSlot> slots;

        [Header("Moidifiers")]
        [SerializeField] SliderWithText healthSliderWithText;
        [SerializeField] SliderWithText manaSliderWithText;
        [SerializeField] SliderWithText speedSliderWithText;
        [SerializeField] SliderWithText damageSliderWithText;
        [SerializeField] SliderWithText offHandSliderWithText;
        [SerializeField] TMP_Dropdown weaponsDropdown;
        [SerializeField] TMP_Dropdown offHandsDropdown;

        [Space(10)]
        [SerializeField] Button confirmButton;

        public int HealthValue => (int)healthSliderWithText.Value;
        public int ManaValue => (int)manaSliderWithText.Value;
        public int SpeedValue => (int)speedSliderWithText.Value;
        public int DamageValue => (int)damageSliderWithText.Value;
        public int OffHandValue => (int)offHandSliderWithText.Value;
        public WeaponName SelectedWeapon => (WeaponName)weaponsDropdown.value;
        public OffHandName SelectedOffHand => (OffHandName)offHandsDropdown.value;
        public List<MonsterName> MonsersNames => slots.Where(s => s.SelectedMonster != MonsterName.None).Select(s => s.SelectedMonster).ToList();
        public List<CombatFieldPos> MonsersPositions => slots.Where(s => s.SelectedMonster != MonsterName.None).Select(s => s.FieldPos).ToList();

        private void Start()
        {
            healthSliderWithText.Initialize(50, 300, 300, true);
            manaSliderWithText.Initialize(0, 20, 8, true);
            speedSliderWithText.Initialize(1, 20, 10, true);
            damageSliderWithText.Initialize(5, 100, 50, true);
            offHandSliderWithText.Initialize(5, 100, 50, true);

            weaponsDropdown.ClearOptions();
            weaponsDropdown.AddOptions(Enum.GetNames(typeof(WeaponName)).ToList());

            offHandsDropdown.ClearOptions();
            offHandsDropdown.AddOptions(Enum.GetNames(typeof(OffHandName)).ToList());

            confirmButton.onClick.AddListener(ConfirmButton_Aciton);
        }

        private void ConfirmButton_Aciton()
        {
            if (MonsersNames.All(m => m == MonsterName.None)) return;
            BaseStats SelectedHeroStats = new BaseStats(HealthValue, ManaValue, SpeedValue, DamageValue, OffHandValue);
            OfflineCombatManager.Instance.TryRequestCombat(MonsersNames, MonsersPositions, SelectedHeroStats, SelectedWeapon, SelectedOffHand);
            GetComponent<Canvas>().enabled = false;
        }
    }
}
