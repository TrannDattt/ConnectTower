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
        [SerializeField] private float _moveDuration = 0.5f;
        [SerializeField] private int _scaleFactor = 5;
        [SerializeField] private float _scaleDuration = .5f;
        [SerializeField] private float _stayDuration = .7f;

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
            Debug.Log($"Initial pos: {_initialPos}");
        }

        public IEnumerator DoLevelIndexAnim(Vector3 pos)
        {
            int startSize = _initialFontSize * _scaleFactor;
            int overshootSize = startSize + _initialFontSize;

            transform.position = pos;
            _levelIndexText.fontSize = startSize;
            if (_textOutline != null)
            {
                _textOutline.effectDistance = _initialOutline * ((float)startSize /_initialFontSize);
            }
            gameObject.SetActive(true);

            yield return DOTween.Sequence()
                                .Append(DOTween.To(() => _levelIndexText.fontSize, x => ScaleText(x), overshootSize, _scaleDuration * 0.5f).SetEase(Ease.InQuad))
                                .Append(DOTween.To(() => _levelIndexText.fontSize, x => ScaleText(x), startSize, _scaleDuration * 0.5f).SetEase(Ease.OutQuad))
                                .AppendInterval(_stayDuration)
                                .Append(transform.DOMove(_initialPos, _moveDuration).SetEase(Ease.OutCubic))
                                .Join(DOTween.To(() => _levelIndexText.fontSize, x => ScaleText(x), _initialFontSize, _scaleDuration).SetEase(Ease.OutCubic))
                                .SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable)
                                .OnKill(() => ScaleText(_initialFontSize))
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

        void Awake()
        {
            _initialFontSize = _levelIndexText.fontSize;
            if (_textOutline != null) _initialOutline = _textOutline.effectDistance;
            _initialPos = transform.position;
        }
    }
}
