using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    public class MonsterSlot : MonoBehaviour
    {
        [SerializeField] Vector2Int coordinats;
        [SerializeField] TMP_Text coordinatesText;
        [SerializeField] TMP_Dropdown dropdown;

        public CombatFieldPos FieldPos => new CombatFieldPos() { rowIndex = coordinats.x, columnIndex = coordinats.y };
        public MonsterName SelectedMonster => (MonsterName)dropdown.value;

        private void Start()
        {
            coordinatesText.text = coordinats.ToString();
            dropdown.ClearOptions();
            dropdown.AddOptions(Enum.GetNames(typeof(MonsterName)).ToList());
        }
    }
}
