using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GP7.Prodigy.Combat
{
    public class PauseGameUI : MonoBehaviour
    {
        [SerializeField] Button yesButton;
        [SerializeField] Button noButton;

        public void Initialize(UnityAction buttonAction)
        {
            yesButton.onClick.AddListener(buttonAction);
            noButton.onClick.AddListener(Hide);
        }

        public void Show() => GetComponent<Canvas>().enabled = true;
        public void Hide() => GetComponent<Canvas>().enabled = false;
    }
}
