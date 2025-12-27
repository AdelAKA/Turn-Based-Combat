using UnityEngine;
using UnityEngine.UI;

namespace GP7.Prodigy.Combat
{
    public class CharacterPortrait : MonoBehaviour
    {
        [SerializeField] Image portrait;

        public void SetSprite(Sprite sprite)
        {
            portrait.sprite = sprite;
        }
    }
}
