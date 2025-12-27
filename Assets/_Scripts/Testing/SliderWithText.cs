using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GP7.Prodigy.Combat
{
    public class SliderWithText : MonoBehaviour
    {
        [SerializeField] TMP_Text text;
        [SerializeField] Slider slider;

        public float Value => slider.value;

        private void Start()
        {
            slider.onValueChanged.AddListener((newValue) => text.text = newValue.ToString());
        }

        public void Initialize(int minValue, int maxValue, int currentValue, bool isWholeNumbers)
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = Mathf.Clamp(currentValue, minValue, maxValue);
            slider.wholeNumbers = isWholeNumbers;
            text.text = currentValue.ToString();
        }
    }
}
