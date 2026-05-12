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
        [SerializeField] private RectTransform _popupRt;

        void Awake()
        {
            if (_popupRt == null) _popupRt = transform as RectTransform;
        }

        public void Pop(string content, Vector2 anchoredPos, UnityAction onComplete = null)
        {
            float inserOffset = .4f;

            _content.text = content;
            _popupRt.anchoredPosition = anchoredPos;
            _canvasGroup.alpha = 1;

            var sequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject, LinkBehaviour.CompleteOnDisable).SetUpdate(true);
            sequence.Append(_popupRt.DOAnchorPosY(anchoredPos.y + _offsetY, _duration).SetEase(Ease.OutQuad))
                    .Insert(inserOffset, DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0, _duration * (1 - inserOffset)).SetEase(Ease.OutQuad))
                    .OnComplete(() =>
                    {
                        onComplete?.Invoke();
                    });
        }
    }
}
