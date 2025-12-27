using UnityEngine;

namespace GP7.Prodigy.Combat
{
    public class TextPopupInitializer : MonoBehaviour
    {
        public static TextPopupInitializer Instance { get; set; }

        [SerializeField] TextPopup stylizedPerfectTextPopupPrefab;
        [SerializeField] TextPopup editableTextPopupPrefab;

        private void Awake()
        {
            Instance = this;
        }

        public void CreateEditable(string text, Vector3 textPosition, Vector3 textScale, Camera relatedCamera, Color textColor)
        {
            TextPopup textPopup = Instantiate(editableTextPopupPrefab, textPosition, Quaternion.identity);
            textPopup.SetUp(text, textScale, relatedCamera, textColor);
        }

        public void CreateStylizedPerfect(Vector3 textPosition)
        {
            TextPopup textPopup = Instantiate(stylizedPerfectTextPopupPrefab, textPosition, Quaternion.identity);
        }
    }
}