using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using Assets._Scripts.Editor;
#endif

namespace Assets._Scripts.Visuals
{
    public class LevelFinishedVisual : GamePopupVisual
    {
        [System.Serializable]
        private sealed class ButtonIdleFloatAnimation
        {
            [SerializeField] private float _moveOffsetY = 16f;
            [SerializeField] private float _moveDuration = 1.35f;
            [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [SerializeField] private float _startDelayStep = 0.12f;

            private readonly List<Tween> _moveTweens = new();
            private readonly Dictionary<RectTransform, Vector2> _initialPositions = new();

            public void CacheTargets(IEnumerable<RectTransform> targets)
            {
                _initialPositions.Clear();
                if (targets == null)
                    return;

                foreach (var target in targets)
                {
                    if (target == null)
                        continue;

                    _initialPositions[target] = target.anchoredPosition;
                }
            }

            public void Play(IEnumerable<RectTransform> targets, GameObject owner)
            {
                Stop();
                if (targets == null)
                    return;

                var index = 0;
                foreach (var target in targets)
                {
                    if (target == null || !target.gameObject.activeInHierarchy)
                        continue;

                    if (!_initialPositions.TryGetValue(target, out var initialPosition))
                    {
                        initialPosition = target.anchoredPosition;
                        _initialPositions[target] = initialPosition;
                    }

                    target.anchoredPosition = initialPosition;
                    var tween = target.DOAnchorPosY(initialPosition.y + _moveOffsetY, _moveDuration)
                        .SetEase(_moveCurve)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetDelay(index * _startDelayStep)
                        .SetUpdate(true)
                        .SetLink(owner, LinkBehaviour.KillOnDisable);
                    _moveTweens.Add(tween);
                    index++;
                }
            }

            public void Stop()
            {
                foreach (var tween in _moveTweens)
                    tween?.Kill();

                _moveTweens.Clear();

                foreach (var pair in _initialPositions)
                {
                    if (pair.Key == null)
                        continue;

                    pair.Key.anchoredPosition = pair.Value;
                }
            }
        }

        [System.Serializable]
        private sealed class ScalePulseAnimation
        {
            [SerializeField] private float _scaleMultiplier = 1.08f;
            [SerializeField] private float _duration = 0.8f;
            [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [SerializeField] private float _startDelay = 0.1f;

            private Tween _scaleTween;
            private Vector3 _initialScale = Vector3.one;
            private bool _hasCachedScale;

            public void CacheState(RectTransform target)
            {
                if (target == null)
                    return;

                _initialScale = target.localScale;
                _hasCachedScale = true;
            }

            public void Play(RectTransform target, GameObject owner)
            {
                if (target == null || !target.gameObject.activeInHierarchy)
                    return;

                CacheState(target);
                Stop(target);
                target.localScale = _initialScale;

                _scaleTween = target.DOScale(_initialScale * _scaleMultiplier, _duration)
                    .SetEase(_scaleCurve)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(_startDelay)
                    .SetUpdate(true)
                    .SetLink(owner, LinkBehaviour.KillOnDisable);
            }

            public void Stop(RectTransform target)
            {
                _scaleTween?.Kill();
                _scaleTween = null;

                if (target == null || !_hasCachedScale)
                    return;

                target.localScale = _initialScale;
            }
        }

        [Header("Base")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private GameButtonVisual _continueButton;
        [SerializeField] private GameButtonVisual _normalRewardButton;
        [SerializeField] private Text _normalRewardText;
        [SerializeField] private GameButtonVisual _adsRewardButton;
        [SerializeField] private Text _adsRewardText;

        [Header("Idle Button Anim")]
        [SerializeField] private RectTransform _buttonsRoot;
        [SerializeField] private ButtonIdleFloatAnimation _buttonIdleFloat = new();
        [SerializeField] private ScalePulseAnimation _adsRewardButtonPulse = new();

        [Header("Master Anim")]
        // [SerializeField] private float _textDelay;
        [SerializeField] private float _starDelay;
        [SerializeField] private float _hornDelay;
        [SerializeField] private float _blockDelay;
        // [SerializeField] private float _particleDelay;

        [Header("Particle")]
        [SerializeField] private float _hornScaleYTime;
        [SerializeField] private AnimationCurve _hornScaleYCurve;
        [SerializeField] private float _confettiDelayTime;
        [SerializeField] private ParticleSystem _topConfetti1;
        [SerializeField] private ParticleSystem _topConfetti2;
        [SerializeField] private ParticleSystem _bottomConfetti1;
        [SerializeField] private ParticleSystem _bottomConfetti2;

        [Header("Star Anim")]
        [SerializeField] private Image _star;
        [SerializeField] private float _startAnimDur;
        [SerializeField] private float _startRotateAngle;
        [SerializeField] private AnimationCurve _starRotateCurve;
        [SerializeField] private float _starScaleFactor;
        [SerializeField] private AnimationCurve _starScaleCurve;

        [Header("Horn Anim")]
        [SerializeField] private Image[] _hornImages;
        [SerializeField] private Vector3 _hornRotationOffset;
        [SerializeField] private float _hornAnimTime;
        [SerializeField] private float _hornDelayTime;
        [SerializeField] private AnimationCurve _hornMoveCurve;
        [SerializeField] private AnimationCurve _hornScaleCurve;
        [SerializeField] private AnimationCurve _hornRotationCurve;

        [Header("Block Anim")]
        [SerializeField] private Image[] _blockImages;
        [SerializeField] private Vector2 _blockStartPos;
        [SerializeField] private float _blockDelayTime;
        // [SerializeField] private AnimationCurve _blockMoveCurve;
        // [SerializeField] private AnimationCurve _blockScaleCurve;

        [Header("Score Anim")]
        [SerializeField] private TextMeshProUGUI _scoreNumText;
        [SerializeField] private RectTransform _scoreText;
        [SerializeField] private RectTransform _newRecord;
        [SerializeField] private float _scoreAnimDelay;
        [SerializeField] private Vector2 _scoreTextStartOffset;
        [SerializeField] private Vector2 _scoreNumTextStartOffset;
        [SerializeField] private float _scoreElementDelay;
        [SerializeField] private float _scoreRevealDuration = 0.35f;
        [SerializeField] private Ease _scoreRevealMoveEase = Ease.OutBack;
        [SerializeField] private Ease _scoreRevealFadeEase = Ease.OutQuad;
        [SerializeField] private Vector3 _scoreRevealStartScale = new(.8f, .8f, 1f);
        [SerializeField] private float _scoreWaveScaleFactor = 1.2f;
        [SerializeField] private float _scoreWaveDuration = 0.3f;
        [SerializeField] private AnimationCurve _scoreWaveScaleCurve;
        [SerializeField] private float _scoreWaveStepDelay = 0.05f;
        // [SerializeField] private Vector3 _scorePunchScale = new(.08f, .08f, 0f);
        // [SerializeField] private float _scorePunchDuration = 0.25f;
        // [SerializeField] private int _scorePunchVibrato = 8;
        // [SerializeField] private float _scorePunchElasticity = 0.5f;
        [SerializeField] private float _newRecordDelay;
        [SerializeField] private float _newRecordDuration = 0.3f;
        [SerializeField] private Ease _newRecordFadeEase = Ease.OutQuad;
        [SerializeField] private Ease _newRecordEase = Ease.OutBack;

#if UNITY_EDITOR
        [SerializeField] private Button _restartBtn;
#endif

        private LevelRuntimeData _curLevelData => LevelManager.PlayingLevel;

        public override IEnumerator Show()
        {
            if (_curLevelData == null)
            {
                Debug.LogWarning($"{nameof(LevelFinishedVisual)} tried to show before a level was ready.", this);
                yield break;
            }

            Debug.Log($"Showing {nameof(LevelFinishedVisual)} for level {_curLevelData.Index}", this);

            var clearedState = _curLevelData.IsCleared;
            _continueButton.gameObject.SetActive(clearedState);
            _normalRewardText.text = _curLevelData.CoinReward.ToString();
            _normalRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardText.text = (_curLevelData.CoinReward * 2).ToString();

            PrepareBlockImage();

            yield return base.Show();

            StartIdleButtonEffects();
            SoundManager.Instance.PlayRandomSFX(ESfx.Win);
            yield return DoWinPopupAnim().WaitForCompletion();
        }

        public override IEnumerator Hide()
        {
            StopIdleButtonEffects();
            yield return base.Hide();
        }

        private Sequence DoWinPopupAnim()
        {
            StartCoroutine(PlayBottomParticle());
            return DOTween.Sequence().SetTarget(this).SetLink(gameObject, LinkBehaviour.KillOnDisable)
            .Append(DoTextAnim())
            .Insert(_scoreAnimDelay, DoScoreAnim())
            .Insert(_hornDelay, DoHornAnim())
            .Insert(_starDelay, DoStarAnim())
            .Insert(_blockDelay, DoBlockImageAnim());
        }

#region SCORE ANIM
        private Vector2 _scoreTextTargetPos;
        private Vector2 _scoreNumTextTargetPos;
        private Vector3 _scoreTextTargetScale;
        private Vector3 _scoreNumTextTargetScale;
        private Vector3 _newRecordStartScale;
        private CanvasGroup _scoreTextCanvasGroup;
        private CanvasGroup _scoreNumTextCanvasGroup;
        private CanvasGroup _newRecordCanvasGroup;

        private Sequence DoScoreAnim()
        {
            PrepareScoreVisuals();

            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            if (_scoreText != null)
                sequence.Append(CreateScoreRevealSequence(_scoreText, _scoreTextTargetPos, _scoreTextTargetScale, _scoreTextStartOffset, _scoreTextCanvasGroup, GetScoreLabelText()));

            if (_scoreText != null && _scoreNumText != null)
                sequence.AppendInterval(_scoreElementDelay);

            if (_scoreNumText != null)
                sequence.Append(CreateScoreRevealSequence(_scoreNumText.rectTransform, _scoreNumTextTargetPos, _scoreNumTextTargetScale, _scoreNumTextStartOffset, _scoreNumTextCanvasGroup, _scoreNumText));

            if (_curLevelData.HasNewHighScore && _newRecord != null)
            {
                sequence.AppendInterval(_newRecordDelay);
                sequence.Append(CreateNewRecordSequence());
            }

            return sequence;
        }

        private void PrepareScoreVisuals()
        {
            if (_scoreNumText != null)
                _scoreNumText.SetText(_curLevelData.CurrentScore.ToString());

            if (_scoreText != null)
            {
                _scoreText.DOKill(true);
                _scoreText.anchoredPosition = _scoreTextTargetPos + _scoreTextStartOffset;
                _scoreText.localScale = Vector3.Scale(_scoreTextTargetScale, _scoreRevealStartScale);
                _scoreTextCanvasGroup.alpha = 0f;
            }

            if (_scoreNumText != null)
            {
                var scoreNumRect = _scoreNumText.rectTransform;
                scoreNumRect.DOKill(true);
                scoreNumRect.anchoredPosition = _scoreNumTextTargetPos + _scoreNumTextStartOffset;
                scoreNumRect.localScale = Vector3.Scale(_scoreNumTextTargetScale, _scoreRevealStartScale);
                _scoreNumTextCanvasGroup.alpha = 0f;
            }

            if (_newRecord != null)
            {
                _newRecord.DOKill(true);
                _newRecordCanvasGroup.alpha = 0f;
                _newRecord.localScale = _newRecordStartScale;
                _newRecord.gameObject.SetActive(_curLevelData.HasNewHighScore);
            }
        }

        private Sequence CreateScoreRevealSequence(RectTransform target, Vector2 targetPosition, Vector3 targetScale, Vector2 startOffset, CanvasGroup canvasGroup, TextMeshProUGUI text)
        {
            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            if (target == null || canvasGroup == null)
                return sequence;

            target.anchoredPosition = targetPosition + startOffset;
            target.localScale = Vector3.Scale(targetScale, _scoreRevealStartScale);
            canvasGroup.alpha = 0f;

            sequence.Append(target.DOAnchorPos(targetPosition, _scoreRevealDuration).SetEase(_scoreRevealMoveEase));
            sequence.Join(target.DOScale(targetScale, _scoreRevealDuration).SetEase(_scoreRevealMoveEase));
            sequence.Join(canvasGroup.DOFade(1f, _scoreRevealDuration).SetEase(_scoreRevealFadeEase));

            var waveTween = CreateScoreWaveTween(text);
            if (waveTween != null)
                sequence.Join(waveTween);

            // if (_scorePunchDuration > 0f)
            //     sequence.Append(target.DOPunchScale(_scorePunchScale, _scorePunchDuration, _scorePunchVibrato, _scorePunchElasticity));

            return sequence;
        }

        private Tween CreateScoreWaveTween(TextMeshProUGUI text)
        {
            if (text == null)
                return null;

            text.ForceMeshUpdate();
            var animator = new DOTweenTMPAnimator(text);
            animator.Refresh();

            var waveSequence = DOTween.Sequence().SetTarget(text).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            var visibleCharIndex = 0;
            for (var i = 0; i < animator.textInfo.characterCount; i++)
            {
                if (!animator.textInfo.characterInfo[i].isVisible) continue;

                animator.SetCharScale(i, Vector3.zero);
                waveSequence.Insert(visibleCharIndex * _scoreWaveStepDelay, animator
                    .DOScaleChar(i, _scoreWaveScaleFactor, _scoreWaveDuration)
                    .SetEase(_scoreWaveScaleCurve));
                // waveSequence.Join(animator
                //     .DOScaleChar(i, _scoreWaveScaleFactor, _scoreWaveDuration + visibleCharIndex * _scoreWaveStepDelay)
                //     .SetEase(_scoreWaveScaleCurve));
                visibleCharIndex++;
            }

            waveSequence.OnKill(animator.Dispose);
            waveSequence.OnComplete(animator.Dispose);
            return waveSequence;
        }

        private Sequence CreateNewRecordSequence()
        {
            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            if (_newRecord == null || _newRecordCanvasGroup == null)
                return sequence;

            _newRecord.gameObject.SetActive(true);
            _newRecord.localScale = _newRecordStartScale;
            _newRecordCanvasGroup.alpha = 0f;

            sequence.Append(_newRecordCanvasGroup.DOFade(1f, _newRecordDuration).SetEase(_newRecordFadeEase));
            sequence.Join(_newRecord.DOScale(Vector3.one, _newRecordDuration).SetEase(_newRecordEase));
            return sequence;
        }

        private TextMeshProUGUI GetScoreLabelText()
        {
            if (_scoreText == null)
                return null;

            return _scoreText.GetComponent<TextMeshProUGUI>() ?? _scoreText.GetComponentInChildren<TextMeshProUGUI>(true);
        }

        private CanvasGroup GetOrAddCanvasGroup(Component target)
        {
            if (target == null)
                return null;

            if (target.TryGetComponent<CanvasGroup>(out var canvasGroup))
                return canvasGroup;

            return target.gameObject.AddComponent<CanvasGroup>();
        }
#endregion

#region STAR ANIM
        private Vector3 _initStarScale;
        private Quaternion _initStarRotation;

        private Sequence DoStarAnim()
        {
            _star.transform.localRotation = Quaternion.Euler(0, 0, _startRotateAngle);

            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);
            sequence.Append(_star.transform.DOLocalRotate(_initStarRotation.eulerAngles, _startAnimDur).SetEase(_starRotateCurve));
            sequence.Join(_star.transform.DOScale(_initStarScale * _starScaleFactor, _startAnimDur).SetEase(_starScaleCurve));
            sequence.OnComplete(reset).OnKill(reset);

            void reset()
            {
                _star.rectTransform.localScale = _initStarScale;
                _star.transform.localRotation = _initStarRotation;
            }

            return sequence;
        }
#endregion

#region HORN ANIM
        private Vector2[] _hornInitPos;
        private Vector3[] _hornInitScale;
        private Quaternion[] _hornInitRotations;
        private bool _hornCached;
        
        private bool CacheHornTargets()
        {
            if (_hornCached) return true;

            if (_hornImages == null || _hornImages.Length == 0)
            {
                Debug.LogWarning($"{nameof(LevelFinishedVisual)} on {name} has no horn images assigned.", this);
                return false;
            }

            if (_hornImages.Any(horn => horn == null))
            {
                Debug.LogWarning($"{nameof(LevelFinishedVisual)} on {name} contains a null horn image reference.", this);
                return false;
            }

            _hornInitPos = _hornImages.Select(horn => horn.rectTransform.anchoredPosition).ToArray();
            _hornInitScale = _hornImages.Select(horn => horn.transform.localScale).ToArray();
            _hornInitRotations = _hornImages.Select(horn => horn.transform.localRotation).ToArray();
            _hornCached = true;
            return true;
        }

        private void PrepareHorns()
        {
            if (!CacheHornTargets()) return;

            for (int i = 0; i < _hornImages.Length; i++)
            {
                _hornImages[i].rectTransform.DOKill(true);
                _hornImages[i].transform.DOKill(true);
                _hornImages[i].rectTransform.anchoredPosition = Vector2.zero;
                _hornImages[i].transform.localScale = Vector3.zero;
                _hornImages[i].transform.localRotation = _hornInitRotations[i];
            }
        }

        private Sequence DoHornAnim()
        {
            PrepareHorns();
            var sequence = DOTween.Sequence();

            for (int i = 0; i < _hornImages.Length; i++)
            {
                var horn = _hornImages[i];
                var baseEuler = _hornInitRotations[i].eulerAngles;
                var offsetEuler = baseEuler + _hornRotationOffset * Mathf.Pow(-1, i);

                var hornSequence = DOTween.Sequence();
                hornSequence.Append(horn.rectTransform.DOAnchorPos(_hornInitPos[i], _hornAnimTime).SetEase(_hornMoveCurve));
                hornSequence.Join(horn.transform.DOScale(_hornInitScale[i], _hornAnimTime).SetEase(_hornScaleCurve));
                hornSequence.Join(horn.transform.DOLocalRotate(offsetEuler, _hornAnimTime).SetEase(_hornRotationCurve));
                hornSequence.Append(_hornImages[i].transform.DOScaleY(2, _hornScaleYTime).SetEase(_hornScaleYCurve));
                // hornSequence.Join(DOTween.Sequence()
                //     .Append(horn.transform.DOLocalRotate(offsetEuler, rotateOutDuration).SetEase(_hornRotationCurve))
                //     .Append(horn.transform.DOLocalRotate(baseEuler, rotateBackDuration).SetEase(_hornRotationCurve)));

                sequence.Insert(i * _hornDelayTime, hornSequence);
            }

            // for (int i = 0; i < _hornImages.Length; i++)
            // {
            //     if (i == 0)
            //         sequence.Append(_hornImages[i].transform.DOScaleY(2, _hornScaleYTime).SetEase(_hornScaleYCurve));
            //     else
            //         sequence.Join(_hornImages[i].transform.DOScaleY(2, _hornScaleYTime).SetEase(_hornScaleYCurve));
            // }

            var confettiDelayTime = (_hornImages.Length - 1) * _hornDelayTime + _hornAnimTime + _confettiDelayTime;
            sequence.InsertCallback(confettiDelayTime, () => StartCoroutine(PlayTopParticles()));

            return sequence;
        }
#endregion

#region TEXT ANIM
        private Sequence DoTextAnim()
        {
            if (!_titleText.gameObject.TryGetComponent<WarpTextExample>(out var wrapper)) return null;

            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);

            var targetPos = _titleText.transform.localPosition;
            var targetColor = _titleText.color;
            var targetScale = Vector3.one;
            var targetCurveScale = 23f;

            float overshoot = .2f;

            var startPos = targetPos - Vector3.up * 250f;
            var startScale = targetScale * 0.4f;
            var startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0);
            var startCurveScale = 150f;

            // Manual Overshoot Values
            var overshootPos = targetPos - (startPos - targetPos) * overshoot;
            var overshootScale = targetScale - (startScale - targetScale) * overshoot;
            var overshootCurve = targetCurveScale - (startCurveScale - targetCurveScale) * overshoot;

            float popDuration = 0.45f;
            float settleDuration = 0.25f;

            // Initial State setup
            _titleText.transform.localPosition = startPos;
            _titleText.transform.localScale = startScale;
            _titleText.color = startColor;
            wrapper.CurveScale = startCurveScale;

            // --- Pop Sequence (Moves to overshoot position/scale) ---
            var popSeq = DOTween.Sequence();
            popSeq.Append(_titleText.transform.DOLocalMoveY(overshootPos.y, popDuration).SetEase(Ease.OutQuad));
            popSeq.Join(_titleText.transform.DOScale(overshootScale, popDuration).SetEase(Ease.OutQuad));
            popSeq.Join(_titleText.DOFade(1f, popDuration * 0.3f).SetEase(Ease.OutCubic));
            popSeq.Join(DOTween.To(() => wrapper.CurveScale, x => wrapper.CurveScale = x, overshootCurve, popDuration).SetEase(Ease.OutQuad));

            // --- Settle Sequence (Returns to target position/scale) ---
            var settleSeq = DOTween.Sequence();
            settleSeq.Append(_titleText.transform.DOLocalMoveY(targetPos.y, settleDuration).SetEase(Ease.OutSine));
            settleSeq.Join(_titleText.transform.DOScale(targetScale, settleDuration).SetEase(Ease.OutSine));
            settleSeq.Join(DOTween.To(() => wrapper.CurveScale, x => wrapper.CurveScale = x, targetCurveScale, settleDuration).SetEase(Ease.OutSine));

            // Assemble Main Sequence
            sequence.Append(popSeq);
            sequence.Append(settleSeq);
            sequence.Append(_titleText.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 10, 0.5f));

            void reset()
            {
                _titleText.transform.localPosition = targetPos;
                _titleText.transform.localScale = targetScale;
                _titleText.color = targetColor;
                wrapper.CurveScale = targetCurveScale;
            }

            sequence.OnComplete(reset).OnKill(reset);

            return sequence.Play();
        }
#endregion

#region BLOCK ANIM        
        private Vector2[] _blockTargetPositions;
        private Vector3[] _blockTargetScales;
        private Vector3[] _blockTargetLocalPositions;
        private Quaternion[] _blockTargetLocalRotations;
        private bool _blockTargetsCached;

        private bool CacheBlockTargets()
        {
            if (_blockTargetsCached) return true;

            if (_blockImages == null || _blockImages.Length == 0)
            {
                Debug.LogWarning($"{nameof(LevelFinishedVisual)} on {name} has no block images assigned.", this);
                return false;
            }

            if (_blockImages.Any(block => block == null))
            {
                Debug.LogWarning($"{nameof(LevelFinishedVisual)} on {name} contains a null block image reference.", this);
                return false;
            }

            _blockTargetPositions = _blockImages.Select(block => block.rectTransform.anchoredPosition).ToArray();
            _blockTargetScales = _blockImages.Select(block => block.transform.localScale).ToArray();
            _blockTargetLocalPositions = _blockImages.Select(block => block.transform.localPosition).ToArray();
            _blockTargetLocalRotations = _blockImages.Select(block => block.transform.localRotation).ToArray();
            _blockTargetsCached = true;
            return true;
        }

        private void PrepareBlockImage()
        {
            if (!CacheBlockTargets()) return;

            for(int i = 0; i < _blockImages.Length; i++)
            {
                var block = _blockImages[i];
                block.DOKill(true);
                block.rectTransform.DOKill(true);
                block.transform.DOKill(true);
                block.transform.SetLocalPositionAndRotation(_blockTargetLocalPositions[i], _blockTargetLocalRotations[i]);
                block.rectTransform.anchoredPosition = _blockStartPos;
                block.transform.localScale = _blockTargetScales[i] * .2f;
                block.gameObject.SetActive(false);
            }
        }

        private Sequence DoBlockImageAnim()
        {
            if (!CacheBlockTargets()) return DOTween.Sequence();

            float popDuration = 0.45f;
            float settleDuration = 0.25f;
            float overshootFactor = 0.2f;

            var sequence = DOTween.Sequence().SetLink(gameObject, LinkBehaviour.KillOnDisable);

            for (int i = 0; i < _blockImages.Length; i++)
            {
                var block = _blockImages[i];
                var tPos = _blockTargetPositions[i];
                var tScale = _blockTargetScales[i];

                var oPos = tPos - (_blockImages[i].rectTransform.anchoredPosition - tPos) * overshootFactor;
                var oScale = tScale - (_blockImages[i].transform.localScale - tScale) * overshootFactor;

                var blockSequence = DOTween.Sequence();
                blockSequence.AppendCallback(() => block.gameObject.SetActive(true));
                blockSequence.Append(block.rectTransform.DOAnchorPos(oPos, popDuration).SetEase(Ease.OutQuad));
                blockSequence.Join(block.transform.DOScale(oScale, popDuration).SetEase(Ease.OutQuad));
                blockSequence.Append(block.rectTransform.DOAnchorPos(tPos, settleDuration).SetEase(Ease.OutSine));
                blockSequence.Join(block.transform.DOScale(tScale, settleDuration).SetEase(Ease.OutSine));

                sequence.Insert(i * _blockDelayTime, blockSequence);
            }

            sequence.OnComplete(() =>
            {
                foreach (var block in _blockImages)
                {
                    float randomDelay = UnityEngine.Random.Range(0f, 0.8f);
                    
                    // Hiệu ứng phập phồng (float) lên xuống
                    block.transform.DOLocalMoveY(block.transform.localPosition.y + 15f, 1.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetDelay(randomDelay)
                        .SetLink(block.gameObject);

                    // Hiệu ứng xoay nhẹ quanh trục Z
                    block.transform.DOLocalRotate(new Vector3(0, 0, 15f), 2f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetDelay(randomDelay)
                        .SetLink(block.gameObject);
                }
            });

            return sequence.Play();
        }
#endregion

        private readonly List<GameButtonVisual> _idleButtons = new();
        private readonly List<RectTransform> _buttonIdleTargets = new();

#region PLAY PARTICLE
        private IEnumerator PlayTopParticles()
        {
            _topConfetti1.Play();
            yield return new WaitForSeconds(_hornDelayTime);
            _topConfetti2.Play();
        }

        private IEnumerator PlayBottomParticle()
        {
            _bottomConfetti1.Play();
            _bottomConfetti2.Play();
            yield return null;
        }
#endregion

        private void CacheIdleButtonTargets()
        {
            _idleButtons.Clear();
            _buttonIdleTargets.Clear();
            if (_buttonsRoot == null)
            {
                var buttonsTransform = transform.Find("Buttons") ?? transform.Find("Button");
                if (buttonsTransform != null)
                    _buttonsRoot = buttonsTransform as RectTransform;
            }

            if (_buttonsRoot == null)
                return;

            _buttonsRoot.GetComponentsInChildren(true, _idleButtons);
            foreach (var button in _idleButtons)
            {
                if (button == null || button.ButtonRt == null)
                    continue;

                _buttonIdleTargets.Add(button.ButtonRt);
            }

            _buttonIdleFloat.CacheTargets(_buttonIdleTargets);
        }

        private void StartIdleButtonEffects()
        {
            StopIdleButtonEffects();
            _buttonIdleFloat.Play(_buttonIdleTargets, gameObject);
            _adsRewardButtonPulse.Play(_adsRewardButton != null ? _adsRewardButton.ButtonRt : null, gameObject);
        }

        private void StopIdleButtonEffects()
        {
            _buttonIdleFloat.Stop();
            _adsRewardButtonPulse.Stop(_adsRewardButton != null ? _adsRewardButton.ButtonRt : null);
        }

        protected override void Start()
        {
#if UNITY_EDITOR
            if (_restartBtn != null)
            {
                _restartBtn.gameObject.SetActive(true);
                _restartBtn.onClick.AddListener(() => 
                {
                    StartCoroutine(Hide());
                    GameManager.Instance.RestartLevel();
                });
            }
#endif
            _initStarScale = _star.transform.localScale;
            _initStarRotation = _star.transform.localRotation;
            CacheBlockTargets();
            CacheHornTargets();
            CacheIdleButtonTargets();
            _scoreTextCanvasGroup = GetOrAddCanvasGroup(_scoreText);
            _scoreNumTextCanvasGroup = GetOrAddCanvasGroup(_scoreNumText);
            _newRecordCanvasGroup = GetOrAddCanvasGroup(_newRecord);
            if (_scoreText != null)
            {
                _scoreTextTargetPos = _scoreText.anchoredPosition;
                _scoreTextTargetScale = _scoreText.localScale;
            }
            if (_scoreNumText != null)
            {
                _scoreNumTextTargetPos = _scoreNumText.rectTransform.anchoredPosition;
                _scoreNumTextTargetScale = _scoreNumText.rectTransform.localScale;
            }
            if (_newRecord != null)
            {
                _newRecordStartScale = _newRecord.localScale;
                _newRecord.gameObject.SetActive(false);
            }
            if (_adsRewardButton != null && _adsRewardButton.ButtonRt != null)
                _adsRewardButtonPulse.CacheState(_adsRewardButton.ButtonRt);

            _continueButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Continue next level");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu();
            });
            _normalRewardButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Gained level reward");
                StartCoroutine(ClaimRewardAndContinue(_curLevelData.CoinReward));
            });
            _adsRewardButton.OnClicked.AddListener(() => 
            {
                //TODO: Ads Service
                Debug.Log("Gained double reward via ads");
                StartCoroutine(ClaimRewardAndContinue(_curLevelData.CoinReward * 2));
            });

            base.Start();
        }

        private void OnDisable()
        {
            StopIdleButtonEffects();
        }

        private IEnumerator ClaimRewardAndContinue(int rewardAmount)
        {
            yield return Hide();

            if (_curLevelData != null
                && _curLevelData.Index <= 5
                && GameManager.Instance.TryStartLevelIngame(_curLevelData.Index + 1, false, EBooster.ExtraMove, EBooster.Shuffle, EBooster.Hint))
            {
                UserManager.GainCoin(rewardAmount);
                yield break;
            }

            GameManager.Instance.GoToMenu(() => UserManager.GainCoin(rewardAmount));
        }
    }
}
