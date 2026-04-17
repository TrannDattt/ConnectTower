using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TextPopupVisual : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _offsetY;
        [SerializeField] private float _duration;
        [SerializeField] private Text _content;

        public void Pop(string content, Vector2 screenPos, UnityAction onComplete = null)
        {
            float inserOffset = .4f;

            _content.text = content;

            // Lấy canvas gốc để ánh xạ screen position vào đúng plane của Canvas này
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            var canvasRt = canvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasRt, screenPos, canvas.worldCamera, out Vector3 worldPos);

            transform.position = worldPos;
            _canvasGroup.alpha = 1;

            var sequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject, LinkBehaviour.CompleteOnDisable).SetUpdate(true);
            sequence.Append(transform.DOMoveY(worldPos.y + _offsetY, _duration).SetEase(Ease.OutQuad))
                    .Insert(inserOffset, DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0, _duration * (1 - inserOffset)).SetEase(Ease.OutQuad))
                    .OnComplete(() =>
                    {
                        onComplete?.Invoke();
                    });
        }
    }
}