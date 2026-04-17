using System.Collections;
using Assets._Scripts.Enums;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class DifficultyTagVisual : MonoBehaviour
    {
        [SerializeField] private GameObject _transition;
        [SerializeField] private Image _transitionMask;

        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite _hardBackground;
        [SerializeField] private Sprite _superHardBackground;

        [SerializeField] private Image _difficultyTag;
        [SerializeField] private Sprite _hardTag;
        [SerializeField] private Sprite _superHardTag;

        [SerializeField] private Text _difficultyText;
        [SerializeField] private Outline _textOutline;

        [SerializeField] private float _animDuration = 0.5f;

        private Vector3 _textInitialPos;
        private int _initialFontSize;
        private Vector2 _initialOutline;

        public void SetDifficulty(EDifficulty difficulty)
        {
            _transition.gameObject.SetActive(difficulty != EDifficulty.Normal);
            _transitionMask.fillAmount = 0;

            switch (difficulty)
            {
                case EDifficulty.Normal:
                    _difficultyText.gameObject.SetActive(false);
                    break;
                case EDifficulty.Hard:
                    _backgroundImage.sprite = _hardBackground;
                    _difficultyTag.sprite = _hardTag;
                    _difficultyText.text = "Hard";
                    _difficultyText.gameObject.SetActive(true);
                    break;
                case EDifficulty.SuperHard:
                    _backgroundImage.sprite = _superHardBackground;
                    _difficultyTag.sprite = _superHardTag;
                    _difficultyText.text = "Super Hard";
                    _difficultyText.gameObject.SetActive(true);
                    break;
            }
        }

        public void PrepareAnim()
        {
            _difficultyText.gameObject.SetActive(false);
            _textInitialPos = _difficultyText.transform.position;
            _initialFontSize = _difficultyText.fontSize;
            if (_textOutline != null) _initialOutline = _textOutline.effectDistance;
        }

        public IEnumerator DoDifficultyAnim(Vector3 pos)
        {
            int scaleFactor = 5;
            int startSize = _initialFontSize * scaleFactor;
            int overshootSize = startSize + _initialFontSize;
            float scaleDuration = .7f;
            float stayDuration = .5f;

            _difficultyText.transform.position = pos;
            _difficultyText.fontSize = startSize;
            if (_textOutline != null)
            {
                _textOutline.effectDistance = _initialOutline * ((float)startSize /_initialFontSize);
            }
            _difficultyText.gameObject.SetActive(true);
            var sequence = DOTween.Sequence().SetTarget(gameObject).SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable);


            // Scale and move text
            sequence.Append(DOTween.To(() => _difficultyText.fontSize, x => ScaleText(x), overshootSize, scaleDuration * 0.5f).SetEase(Ease.InQuad))
                    .Append(DOTween.To(() => _difficultyText.fontSize, x => ScaleText(x), startSize, scaleDuration * 0.5f).SetEase(Ease.OutQuad))
                    .AppendInterval(stayDuration)
                    .Append(_difficultyText.transform.DOMove(_textInitialPos, _animDuration).SetEase(Ease.OutCubic))
                    .Join(DOTween.To(() => _difficultyText.fontSize, x => ScaleText(x), _initialFontSize, scaleDuration).SetEase(Ease.OutCubic));
                    
            // Show background
            sequence.Append(DOTween.To(() => _transitionMask.fillAmount, x => _transitionMask.fillAmount = x, 1, 1f));
                    
            yield return sequence.WaitForCompletion();
        }

        private void ScaleText(int newSize)
        {
            float factor = (float)newSize /_initialFontSize;
            _difficultyText.fontSize = newSize;
            if (_textOutline != null)
            {
                _textOutline.effectDistance = _initialOutline * factor;
            }
        }
    }
}