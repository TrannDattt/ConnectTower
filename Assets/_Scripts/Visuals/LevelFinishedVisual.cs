using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Editor;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class LevelFinishedVisual : GamePopupVisual
    {
        [Header("Base")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private GameButtonVisual _continueButton;
        [SerializeField] private GameButtonVisual _normalRewardButton;
        [SerializeField] private Text _normalRewardText;
        [SerializeField] private GameButtonVisual _adsRewardButton;
        [SerializeField] private Text _adsRewardText;


        [Header("Master Anim")]
        [SerializeField] private float _textDelay;
        [SerializeField] private float _starDelay;
        [SerializeField] private float _hornDelay;
        [SerializeField] private float _blockDelay;
        [SerializeField] private float _particleDelay;

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
        [SerializeField] private AnimationCurve _blockMoveCurve;
        [SerializeField] private AnimationCurve _blockScaleCurve;

#if UNITY_EDITOR
        [SerializeField] private Button _restartBtn;
#endif

        private LevelRuntimeData _curLevelData => LevelManager.PlayingLevel;

        public override IEnumerator Show()
        {
            var clearedState = _curLevelData.IsCleared;
            _continueButton.gameObject.SetActive(clearedState);
            _normalRewardText.text = _curLevelData.CoinReward.ToString();
            _normalRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardText.text = (_curLevelData.CoinReward * 2).ToString();

            PrepareBlockImage();

            yield return base.Show();

            SoundManager.Instance.PlayRandomSFX(ESfx.Win);
            yield return DoWinPopupAnim().WaitForCompletion();
        }

        private Sequence DoWinPopupAnim()
        {
            StartCoroutine(PlayBottomParticle());
            return DOTween.Sequence().SetTarget(this).SetLink(gameObject, LinkBehaviour.KillOnDisable)
            .Append(DoTextAnim())
            .Insert(_hornDelay, DoHornAnim())
            .Insert(_starDelay, DoStarAnim())
            .Insert(_blockDelay, DoBlockImageAnim());
        }

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

        protected override void Start()
        {
#if UNITY_EDITOR
            if (_restartBtn != null)
            {
                _restartBtn.onClick.AddListener(() => 
                {
                    StartCoroutine(Hide());
                    GameManager.Instance.StartLevel(_curLevelData);
                });
            }
#endif
            _initStarScale = _star.transform.localScale;
            _initStarRotation = _star.transform.localRotation;
            CacheBlockTargets();
            CacheHornTargets();

            _continueButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Continue next level");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu();
            });
            _normalRewardButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Gained level reward");
                StartCoroutine(Hide());

#if UNITY_EDITOR
                if (DebugFlagToggle.Instance.SkipFirstLevel)
                {
#endif
                    if (UserManager.CurUser.CurrentLevelIndex <= 5)
                    {
                        GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                        {
                            UserManager.GainCoin(_curLevelData.CoinReward);
                            GameManager.Instance.StartLevel(LevelManager.Instance.GetLevel(_curLevelData.Index + 1), false, EBooster.ExtraMove, EBooster.Shuffle, EBooster.Hint);
                        });
                    }
                    else
                        GameManager.Instance.GoToMenu(() => UserManager.GainCoin(_curLevelData.CoinReward));
#if UNITY_EDITOR
                }
#endif
            });
            _adsRewardButton.OnClicked.AddListener(() => 
            {
                //TODO: Ads Service
                Debug.Log("Gained double reward via ads");
                StartCoroutine(Hide());

#if UNITY_EDITOR
                if (DebugFlagToggle.Instance.SkipFirstLevel)
                {
#endif
                    if (_curLevelData.Index + 1 <= 5)
                    {
                        GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                        {
                            UserManager.GainCoin(_curLevelData.CoinReward * 2);
                            GameManager.Instance.StartLevel(LevelManager.Instance.GetLevel(_curLevelData.Index + 1), false, EBooster.ExtraMove, EBooster.Shuffle, EBooster.Hint);
                        });
                    }
                    else
                        GameManager.Instance.GoToMenu(() => UserManager.GainCoin(_curLevelData.CoinReward * 2));
#if UNITY_EDITOR
                }
#endif
            });

            base.Start();
        }
    }
}
