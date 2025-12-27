using DG.Tweening;
using TMPro;
using UnityEngine;

namespace GP7.Prodigy.Combat
{
    public class TextPopup : MonoBehaviour
    {
        private TextMeshPro textMesh;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        public void SetUp(string _text, Vector3 textScale, Camera relatedCamera, Color textColor)
        {
            textMesh.SetText(_text);
            textMesh.color = textColor;
            transform.localScale = textScale;
            transform.rotation = relatedCamera.transform.rotation;
            transform.Rotate(0f, 0f, Random.Range(-30, 30));
            transform.DOLocalMove(transform.position + (transform.up * Random.Range(1f, 2f) * transform.localScale.x), 1f)
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    //textMesh.DOFade(0, 0.5f).OnComplete(() => Destroy(gameObject));
                });
        }
    }
}