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

        public void Pop(string content, Vector3 pos, UnityAction onComplete = null)
        {
            float inserOffset = .4f;

            _content.text = content;
            transform.position = pos;
            _canvasGroup.alpha = 1;

            var sequence = DOTween.Sequence().SetTarget(this);
            sequence.Append(transform.DOMoveY(pos.y + _offsetY, _duration).SetEase(Ease.OutQuad))
                    .Insert(inserOffset, DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0, _duration * (1 - inserOffset)).SetEase(Ease.OutQuad))
                    .OnComplete(() =>
                    {
                        onComplete?.Invoke();
                    });
        }
    }
}