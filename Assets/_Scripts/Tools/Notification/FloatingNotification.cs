using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace GP7.Prodigy.Combat
{
    public class FloatingNotification : MonoBehaviour
    {
        private TextMeshPro textMesh;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        public void SetUp(string _text, Vector3 textScale, Color textColor)
        {
            textMesh.SetText(_text);
            textMesh.color = textColor;
            transform.localScale = textScale;
            transform.DOLocalMove(transform.position + (transform.up * transform.localScale.x * 2), 1f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    Sequence s = DOTween.Sequence();
                    s.AppendCallback(() => GetComponent<SortingGroup>().sortingOrder--);
                    //s.Append(textMesh.DOFade(0, 0.5f));
                    s.Join(textMesh.transform.DOLocalMove(transform.position + (transform.up * transform.localScale.x * 2), 1f));
                    s.OnComplete(() => Destroy(gameObject));
                });
        }
    }
}
