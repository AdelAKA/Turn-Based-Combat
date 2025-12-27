using UnityEngine;
using TMPro;

namespace GP7.Prodigy.Combat
{
    public class EndGameUI : MonoBehaviour
    {
        [SerializeField] TMP_Text endGameText;

        public void Show(string text)
        {
            GetComponent<Canvas>().enabled = true;
            endGameText.text = text;
        }
        public void Hide()
        {
            GetComponent<Canvas>().enabled = false;
        }
    }
}
