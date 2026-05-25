using Assets._Scripts.Datas;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class IngameScoreVisual : MonoBehaviour
    {
        private const string ScoreShowTweenId = "ScoreShow";
        private const string ScoreHideTweenId = "ScoreHide";
        private const string ScorePopTweenId = "ScorePop";
        private const string ScoreIdleTweenId = "ScoreIdle";
        private const string ScoreAutoHideTweenId = "ScoreAutoHide";

        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Vector2 _scoreTextOffset;
        [SerializeField] private float _moveDur = 0.5f;
        [SerializeField] private AnimationCurve _moveCurve;
        [SerializeField] private float _textScaleFactor = 1.2f;
        [SerializeField] private float _textScaleDur = 0.3f;
        [SerializeField] private AnimationCurve _textScaleCurve;
        [SerializeField] private float _charWaveStepDelay = 0.05f;
        [SerializeField] private float _idleFloatAmplitude = 6f;
        [SerializeField] private float _idleFloatAmplitudeVariance = 2f;
        [SerializeField] private float _idleFloatDuration = 1.8f;
        [SerializeField] private float _idleFloatDurationVariance = 0.45f;
        [SerializeField] private float _idleRotateAngle = 6f;
        [SerializeField] private float _idleRotateAngleVariance = 2f;
        [SerializeField] private float _idleRotateDuration = 2.1f;
        [SerializeField] private float _idleRotateDurationVariance = 0.5f;
        [SerializeField] private Vector3 _textRotateAngle;
        [SerializeField] private float _textRotateDur = 0.3f;
        [SerializeField] private float _autoHideDelay = 5f;

        private Vector2 _originPos;
        private DOTweenTMPAnimator _scoreAnimator;

        private EventBinding<UpdateScoreEvent> _updateScoreBinding;

        public void InitScore()
        {
            KillScoreTweens();
            _scoreText.SetText("0");
            (transform as RectTransform).anchoredPosition = _originPos;
            _scoreText.rectTransform.localScale = Vector3.one;
            _scoreText.rectTransform.localRotation = Quaternion.identity;
            StartIdleFloatTween();
        }

        public void ShowScore()
        {
            (transform as RectTransform).anchoredPosition = _originPos;
        }

         public void HideScore()
        {
            (transform as RectTransform).anchoredPosition = _originPos;
        }

        public void PopScore(int score)
        {
            DOTween.Kill(ScorePopTweenId, true);
            _scoreText.SetText(score.ToString());
            StartIdleFloatTween();
            var textSeq = DOTween.Sequence().SetTarget(_scoreText).SetId(ScorePopTweenId).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            textSeq.Append(_scoreText.rectTransform.DOPunchRotation(_textRotateAngle, _textRotateDur).SetEase(_textScaleCurve));
            textSeq.Join(CreateScoreWaveTween());
        }

        public float GetPopDuration()
        {
            EnsureScoreAnimator();
            _scoreAnimator?.Refresh();

            var visibleCharCount = 0;
            var characterCount = _scoreAnimator != null ? _scoreAnimator.textInfo.characterCount : _scoreText.textInfo.characterCount;
            var textInfo = _scoreAnimator != null ? _scoreAnimator.textInfo : _scoreText.textInfo;
            for (var i = 0; i < characterCount; i++)
            {
                if (textInfo.characterInfo[i].isVisible)
                    visibleCharCount++;
            }

            var waveDuration = visibleCharCount <= 0
                ? 0f
                : _textScaleDur + Mathf.Max(0, visibleCharCount - 1) * _charWaveStepDelay;
            return Mathf.Max(_textRotateDur, waveDuration);
        }

        private Tween CreateScoreWaveTween()
        {
            EnsureScoreAnimator();
            _scoreAnimator?.Refresh();

            if (_scoreAnimator == null)
                return DOVirtual.DelayedCall(0f, () => { });

            var waveSeq = DOTween.Sequence().SetTarget(_scoreText).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            var visibleCharIndex = 0;

            for (var i = 0; i < _scoreAnimator.textInfo.characterCount; i++)
            {
                if (!_scoreAnimator.textInfo.characterInfo[i].isVisible) continue;

                _scoreAnimator.SetCharScale(i, Vector3.one);
                waveSeq.Join(_scoreAnimator.DOScaleChar(i, _textScaleFactor, _textScaleDur + visibleCharIndex * _charWaveStepDelay).SetEase(_textScaleCurve));
                // waveSeq.Insert(
                //     visibleCharIndex * _charWaveStepDelay,
                //     _scoreAnimator.DOScaleChar(i, _textScaleFactor, _textScaleDur).SetEase(_textScaleCurve)
                // );
                visibleCharIndex++;
            }

            return waveSeq;
        }

        private void StartIdleFloatTween()
        {
            EnsureScoreAnimator();
            _scoreAnimator?.Refresh();
            DOTween.Kill(ScoreIdleTweenId);

            if (_scoreAnimator == null)
                return;

            var visibleCharIndex = 0;
            for (var i = 0; i < _scoreAnimator.textInfo.characterCount; i++)
            {
                if (!_scoreAnimator.textInfo.characterInfo[i].isVisible) continue;

                var charIndex = i;
                var amplitude = _idleFloatAmplitude + GetIdleVariance(visibleCharIndex, _idleFloatAmplitudeVariance);
                var floatDuration = Mathf.Max(0.1f, _idleFloatDuration + GetIdleVariance(visibleCharIndex + 17, _idleFloatDurationVariance));
                var floatPhase = Mathf.Repeat((visibleCharIndex * 0.37f) + 0.19f, 1f);
                var floatStartAngle = floatPhase * Mathf.PI * 2f;
                var rotateAmplitude = _idleRotateAngle + GetIdleVariance(visibleCharIndex + 31, _idleRotateAngleVariance);
                var rotateDuration = Mathf.Max(0.1f, _idleRotateDuration + GetIdleVariance(visibleCharIndex + 47, _idleRotateDurationVariance));
                var rotatePhase = Mathf.Repeat((visibleCharIndex * 0.29f) + 0.43f, 1f);
                var rotateStartAngle = rotatePhase * Mathf.PI * 2f;

                DOVirtual.Float(floatStartAngle, floatStartAngle + Mathf.PI * 2f, floatDuration, angle =>
                {
                    if (_scoreAnimator == null) return;
                    _scoreAnimator.SetCharOffset(charIndex, new Vector3(0f, Mathf.Sin(angle) * amplitude, 0f));
                })
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetId(ScoreIdleTweenId)
                .SetTarget(_scoreText)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);

                DOVirtual.Float(rotateStartAngle, rotateStartAngle + Mathf.PI * 2f, rotateDuration, angle =>
                {
                    if (_scoreAnimator == null) return;
                    _scoreAnimator.SetCharRotation(charIndex, new Vector3(0f, 0f, Mathf.Sin(angle) * rotateAmplitude));
                })
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetId(ScoreIdleTweenId)
                .SetTarget(_scoreText)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);

                visibleCharIndex++;
            }
        }

        private float GetIdleVariance(int index, float variance)
        {
            return Mathf.Sin(index * 1.618f) * variance;
        }

        private void EnsureScoreAnimator()
        {
            if (_scoreAnimator != null || _scoreText == null || !_scoreText.gameObject.activeInHierarchy)
                return;

            _scoreAnimator = new DOTweenTMPAnimator(_scoreText);
        }

        private void DisposeScoreAnimator()
        {
            _scoreAnimator?.Dispose();
            _scoreAnimator = null;
        }

        private void RestartAutoHideTimer()
        {
            DOTween.Kill(ScoreAutoHideTweenId);
            DOVirtual.DelayedCall(_autoHideDelay, HideScore)
                     .SetId(ScoreAutoHideTweenId)
                     .SetTarget(this)
                     .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private void KillScoreTweens()
        {
            DOTween.Kill(ScoreShowTweenId);
            DOTween.Kill(ScoreHideTweenId);
            DOTween.Kill(ScorePopTweenId);
            DOTween.Kill(ScoreIdleTweenId);
            DOTween.Kill(ScoreAutoHideTweenId);
        }

        private void Awake()
        {
            _originPos = (transform as RectTransform).anchoredPosition;
            _updateScoreBinding = new(() =>
            {
                if (gameObject.activeInHierarchy)
                {
                    var score = LevelManager.PlayingLevel.CurrentScore;
                    PopScore(score);
                }
            });
        }

        void OnEnable()
        {
            EnsureScoreAnimator();
            StartIdleFloatTween();
            EventBus<UpdateScoreEvent>.Subscribe(_updateScoreBinding);
        }

        private void OnDisable()
        {
            KillScoreTweens();
            DisposeScoreAnimator();
            EventBus<UpdateScoreEvent>.Unsubscribe(_updateScoreBinding);
        }
    }
}
