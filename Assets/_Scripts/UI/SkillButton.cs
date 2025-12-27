using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GP7.Prodigy.Combat
{
    public class SkillButton : MonoBehaviour, ISelectHandler
    {
        [Header("References")]
        [SerializeField] private GameObject visuals;
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text referenceName;
        [SerializeField] private TMP_Text number;
        //[SerializeField] private Image boarder;
        [SerializeField] private Image interactableMark;
        [SerializeField] private Image SelectedMark;
        [SerializeField] private Image DisabledMark;


        [Header("Options")]
        [SerializeField, Range(0, 1)] private float floatScreenPercentage;
        [SerializeField] private bool isDebug;

        public SkillButtonVisualInfos VisualInfo;
        private CombatUI _reference;

        public void Initialize(CombatUI reference, SkillButtonVisualInfos visualInfo, UnityAction buttonAction = null)
        {
            _reference = reference;
            VisualInfo = visualInfo;
            MarkAsDeselected();

            button.onClick.RemoveAllListeners();
            if (buttonAction != null)
                button.onClick.AddListener(buttonAction);

            this.referenceName.text = VisualInfo.name;
            this.number.text = VisualInfo.number;
            this.icon.sprite = VisualInfo.icon;
            UpdateVisuals(buttonAction != null);
        }

        public void ResetButton()
        {
            button.onClick.RemoveAllListeners();
            this.referenceName.text = "None";
            this.number.text = "0";

            button.interactable = false;
            icon.enabled = false;
            //int shinePropertyID = Shader.PropertyToID("_ShineFade");
            //boarder.material.SetFloat(shinePropertyID, 0);
            interactableMark.enabled = false;
            SelectedMark.enabled = false;
            DisabledMark.enabled = true;
        }

        //public void Initialize(CombatUI reference, WeaponsCollection.WeaponInfo weaponInfo, UnityAction buttonAction = null)
        //{
        //    _reference = reference;
        //    MarkAsDeselected();

        //    button.onClick.RemoveAllListeners();
        //    if (buttonAction != null)
        //        button.onClick.AddListener(buttonAction);

        //    this.moveName.text = weaponInfo.name.ToString();
        //    this.manaCost.text = "0";
        //    UpdateVisuals(buttonAction != null);
        //}

        public void UpdateVisuals(bool isEnabled)
        {
            icon.enabled = true;

            if (isEnabled)
            {
                button.interactable = true;
                //icon.enabled = true;
                interactableMark.enabled = true;
                DisabledMark.enabled = false;
                //int shinePropertyID = Shader.PropertyToID("_ShineFade");
                //boarder.material.SetFloat(shinePropertyID, 1);
            }
            else
            {
                button.interactable = false;
                //icon.enabled = false;
                interactableMark.enabled = false;
                DisabledMark.enabled = true;
                //int shinePropertyID = Shader.PropertyToID("_ShineFade");
                //boarder.material.SetFloat(shinePropertyID, 0);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            _reference.InvokeButtonSelected(this);
            if(isDebug) Debug.Log("I am selected", this);
        }

        public void MarkAsSelected()
        {
            //boarder.enabled = true;
            visuals.transform.localPosition = Vector3.up * floatScreenPercentage * Screen.height;
            SelectedMark.enabled = true;
            //int enchantPropertyID = Shader.PropertyToID("_EnchantedFade");
            //boarder.material.SetFloat(enchantPropertyID, 1);

        }
        public void MarkAsDeselected()
        {
            //boarder.enabled = false;
            visuals.transform.localPosition = Vector3.zero;
            SelectedMark.enabled = false;
            //int enchantPropertyID = Shader.PropertyToID("_EnchantedFade");
            //boarder.material.SetFloat(enchantPropertyID, 0);
        }
    }
}
