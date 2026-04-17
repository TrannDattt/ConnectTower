using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class LevelIndexVisual : MonoBehaviour
    {
        [SerializeField] private Text _levelIndexText;
        [SerializeField] private Outline _textOutline;
        [SerializeField] private float _animDuration = 0.5f;

        private Vector3 _initialPos;
        private int _initialFontSize;
        private Vector2 _initialOutline;

        public void SetLevelIndex(int index)
        {
            if (_levelIndexText != null)
            {
                _levelIndexText.text = $"Level {index}";
            }
        }

        public void PrepareAnim()
        {
            gameObject.SetActive(false);
            _initialPos = transform.position;
            _initialFontSize = _levelIndexText.fontSize;
            if (_textOutline != null) _initialOutline = _textOutline.effectDistance;
            Debug.Log($"Initial pos: {_initialPos}");
        }

        public IEnumerator DoLevelIndexAnim(Vector3 pos)
        {
            int scaleFactor = 5;
            int startSize = _initialFontSize * scaleFactor;
            int overshootSize = startSize + _initialFontSize;
            float scaleDuration = .7f;
            float stayDuration = .5f;

            transform.position = pos;
            _levelIndexText.fontSize = startSize;
            if (_textOutline != null)
            {
                _textOutline.effectDistance = _initialOutline * ((float)startSize /_initialFontSize);
            }
            gameObject.SetActive(true);

            yield return DOTween.Sequence()
                                .Append(DOTween.To(() => _levelIndexText.fontSize, x => ScaleText(x), overshootSize, scaleDuration * 0.5f).SetEase(Ease.InQuad))
                                .Append(DOTween.To(() => _levelIndexText.fontSize, x => ScaleText(x), startSize, scaleDuration * 0.5f).SetEase(Ease.OutQuad))
                                .AppendInterval(stayDuration)
                                .Append(transform.DOMove(_initialPos, _animDuration).SetEase(Ease.OutCubic))
                                .Join(DOTween.To(() => _levelIndexText.fontSize, x => ScaleText(x), _initialFontSize, scaleDuration).SetEase(Ease.OutCubic))
                                .SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable)
                                .WaitForCompletion();
        }

        private void ScaleText(int newSize)
        {
            float factor = (float)newSize /_initialFontSize;
            _levelIndexText.fontSize = newSize;
            if (_textOutline != null)
            {
                _textOutline.effectDistance = _initialOutline * factor;
            }
        }
    }
}
