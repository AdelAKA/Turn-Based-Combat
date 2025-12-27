using UnityEngine;
using UnityEngine.UI;

namespace GP7.Prodigy.Combat
{
    public class CombatFieldCell : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] CombatFieldPos combatFieldPos;

        [Header("References")]
        [SerializeField] Transform standTransform;
        [SerializeField] Canvas worldCanvas;
        [SerializeField] Button selectButton;

        [Header("Visuals")]
        [SerializeField] SpriteRenderer standSprite;
        [SerializeField] SpriteRenderer currentTurnMark;
        [SerializeField] Color turnMarkColor;
        [SerializeField] SpriteRenderer targetMark;
        [SerializeField] Color targetMarkColor;

        public Transform StandTransform => standTransform;
        public CombatFieldPos ReservedPos => combatFieldPos;

        public FieldFighter FieldFighterReference { get; private set; }

        private CombatField _combatFieldReference;

        private void Start()
        {
            UnMark();
            selectButton.onClick.AddListener(SelectAsTarget_Action);
            SwitchTargetSelectionMode(false);
            currentTurnMark.color = turnMarkColor;
            targetMark.color = targetMarkColor;
        }

        public void Initialize(Camera referencedCamera, CombatField combatFieldReference, FieldFighter fieldFighterReference)
        {
            _combatFieldReference = combatFieldReference;
            FieldFighterReference = fieldFighterReference;
            worldCanvas.worldCamera = referencedCamera;
        }

        public void SwitchCurrentTurnMark(bool isCurrentTurn)
        {
            //int currentTurnShaderEffectId = Shader.PropertyToID("_InnerOutlineFade");
            //standSprite.material.SetFloat(currentTurnShaderEffectId, isCurrentTurn ? 1 : 0);
            currentTurnMark.enabled = isCurrentTurn;
        }

        public void SwitchAffectedMark(bool isAffected)
        {
            //int targetShaderEffectId = Shader.PropertyToID("_PingPongGlowFade");
            //standSprite.material.SetFloat(targetShaderEffectId, isAffected ? 1 : 0);
            targetMark.enabled = isAffected;
        }

        public void SwitchTargetSelectionMode(bool isSelectable)
        {
            //col.enabled = isSelectable;
            selectButton.gameObject.SetActive(isSelectable);
            //selectionModeArrow.enabled = isSelectable;
        }

        public void UnMark()
        {
            SwitchCurrentTurnMark(false);
            SwitchAffectedMark(false);
            SwitchTargetSelectionMode(false);
            //int currentTurnShaderEffectId = Shader.PropertyToID("_InnerOutlineFade");
            //standSprite.material.SetFloat(currentTurnShaderEffectId, 0);
            //int targetShaderEffectId = Shader.PropertyToID("_PingPongGlowFade");
            //standSprite.material.SetFloat(targetShaderEffectId, 0);
        }

        public void SelectAsTarget_Action()
        {
            _combatFieldReference.SelectTarget(FieldFighterReference);
        }
    }
}
