using System.Collections;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public partial class BoosterButtonVisual : GameButtonVisual
    {
        [field: SerializeField] public EBooster BoosterKey {get; private set;}
        [SerializeField] private CanvasGroup _lockImage;
        [SerializeField] private Image _lockImageUp;
        [SerializeField] private Image _lockImageDown;
        [SerializeField] private Image _lockBackground;
        [SerializeField] private GameObject _baseContent;
        [SerializeField] private Text _countText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _getMoreImage;
        [SerializeField] private BoosterButtonEffectVisual _effectVisual;

        // UNLOCK
        [SerializeField] private float _shakeDur;
        [SerializeField] private float _posXOffset;
        [SerializeField] private int _vibrateVibrato = 30;
        [SerializeField] private float _vibrateRandomness = 20f;
        [SerializeField] private bool _vibrateFadeOut = true;
        [SerializeField] private float _lockOpenDur;
        [SerializeField] private float _lockOpenAngle;
        [SerializeField] private Vector2 _lockImageUpOffset = new(-28f, 36f);
        [SerializeField] private Vector2 _lockImageDownOffset = new(28f, -24f);
        [SerializeField] private AnimationCurve _lockSplitMoveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _lockSplitRotateCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _launchDur = 0.24f;
        [SerializeField] private Vector2 _launchOffset = new(80f, 540f);
        [SerializeField] private float _launchRotateAngle = 18f;
        [SerializeField] private float _launchScaleMultiplier = 1.08f;
        [SerializeField] private AnimationCurve _launchMoveXCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _launchMoveYCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _launchRotateCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _launchScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _fallDur = 0.42f;
        [SerializeField] private Vector2 _fallOffset = new(140f, -980f);
        [SerializeField] private float _fallRotateAngle = -24f;
        [SerializeField] private float _fallScaleMultiplier = 0.92f;
        [SerializeField] private AnimationCurve _fallMoveXCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _fallMoveYCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _fallRotateCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _fallScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _fadeDur;
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField] private ParticleSystem _unlockParticle;

        public bool IsLocked {get; private set;}

        public void ChangeLockStatus(bool isLock)
        {
            if (isLock) Lock();
            else Unlock();
        }

        public void Lock()
        {
            IsLocked = true;
            SetEnable(false);
            _lockImage.gameObject.SetActive(true);
            _lockBackground.gameObject.SetActive(true);
            _baseContent.SetActive(false);
        }

        public void Unlock()
        {
            IsLocked = false;
            SetEnable(true);
            _lockImage.gameObject.SetActive(false);
            _lockBackground.gameObject.SetActive(false);
            _baseContent.SetActive(true);
        }

        public void SetCount(int count)
        {
            if (count > 0)
            {
                _countText.text = $"{count}";
                _countText.gameObject.SetActive(true);
                _getMoreImage.gameObject.SetActive(false);
            }
            else
            {
                _getMoreImage.gameObject.SetActive(true);
            }
        }

        public IEnumerator DoOnUseBoosterAnim(BoosterRuntimeData data, Vector3 gatherPoint) => _effectVisual?.DoOnUseBoosterAnim(data, gatherPoint);

        public Sequence DoUnlockAnim()
        {
            var lockRoot = _lockImage != null ? _lockImage.transform as RectTransform : null;
            var lockImageUpRt = _lockImageUp != null ? _lockImageUp.rectTransform : null;
            var lockImageDownRt = _lockImageDown != null ? _lockImageDown.rectTransform : null;
            if (lockRoot == null || _lockBackground == null)
                return DOTween.Sequence();

            var initialLockAnchoredPos = lockRoot.anchoredPosition;
            var initialLockScale = lockRoot.localScale;
            var initialLockRotation = lockRoot.localRotation;
            var initialLockImageUpPos = lockImageUpRt != null ? lockImageUpRt.anchoredPosition : Vector2.zero;
            var initialLockImageDownPos = lockImageDownRt != null ? lockImageDownRt.anchoredPosition : Vector2.zero;
            var initialLockImageUpRotation = lockImageUpRt != null ? lockImageUpRt.localRotation : Quaternion.identity;
            var initialLockImageDownRotation = lockImageDownRt != null ? lockImageDownRt.localRotation : Quaternion.identity;
            var initialLockBackgroundColor = _lockBackground.color;
            bool isUnlocked = false;

            void ResetLockVisualState()
            {
                lockRoot.DOKill();
                lockImageUpRt?.DOKill();
                lockImageDownRt?.DOKill();
                _lockBackground.DOKill();
                _lockImage.DOKill();

                lockRoot.anchoredPosition = initialLockAnchoredPos;
                lockRoot.localScale = initialLockScale;
                lockRoot.localRotation = initialLockRotation;

                if (lockImageUpRt != null)
                {
                    lockImageUpRt.anchoredPosition = initialLockImageUpPos;
                    lockImageUpRt.localRotation = initialLockImageUpRotation;
                }

                if (lockImageDownRt != null)
                {
                    lockImageDownRt.anchoredPosition = initialLockImageDownPos;
                    lockImageDownRt.localRotation = initialLockImageDownRotation;
                }

                _lockImage.alpha = 1f;
                _lockBackground.color = initialLockBackgroundColor;
            }

            void ApplyLockedPresentation()
            {
                ResetLockVisualState();
                _lockImage.gameObject.SetActive(true);
                _lockBackground.gameObject.SetActive(true);
                _baseContent.SetActive(false);
            }

            void ApplyUnlockedPresentation()
            {
                ResetLockVisualState();
                Unlock();
            }

            ApplyLockedPresentation();
            _unlockParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var sequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            sequence.Append(lockRoot.DOShakeAnchorPos(
                    _shakeDur,
                    _posXOffset,
                    _vibrateVibrato,
                    _vibrateRandomness,
                    false,
                    _vibrateFadeOut)
                .SetEase(Ease.Linear));

            sequence.AppendCallback(() =>
            {
                _baseContent.SetActive(true);
                _unlockParticle?.Play();
            });

            Tween launchXTween = lockRoot.DOAnchorPosX(initialLockAnchoredPos.x + _launchOffset.x, _launchDur).SetEase(_launchMoveXCurve);
            Tween launchYTween = lockRoot.DOAnchorPosY(initialLockAnchoredPos.y + _launchOffset.y, _launchDur).SetEase(_launchMoveYCurve);
            Tween fallXTween = lockRoot.DOAnchorPosX(initialLockAnchoredPos.x + _fallOffset.x, _fallDur).SetEase(_fallMoveXCurve);
            Tween fallYTween = lockRoot.DOAnchorPosY(initialLockAnchoredPos.y + _fallOffset.y, _fallDur).SetEase(_fallMoveYCurve);
            Tween launchRotateTween = lockRoot.DOLocalRotate(new Vector3(0f, 0f, _launchRotateAngle), _launchDur).SetEase(_launchRotateCurve);
            Tween fallRotateTween = lockRoot.DOLocalRotate(new Vector3(0f, 0f, _fallRotateAngle), _fallDur).SetEase(_fallRotateCurve);
            Tween launchScaleTween = lockRoot.DOScale(initialLockScale * _launchScaleMultiplier, _launchDur).SetEase(_launchScaleCurve);
            Tween fallScaleTween = lockRoot.DOScale(initialLockScale * _fallScaleMultiplier, _fallDur).SetEase(_fallScaleCurve);

            sequence.Append(launchXTween);
            sequence.Join(launchYTween);
            sequence.Join(launchRotateTween);
            sequence.Join(launchScaleTween);

            if (lockImageUpRt != null)
            {
                sequence.Join(lockImageUpRt.DOAnchorPos(initialLockImageUpPos + _lockImageUpOffset, _lockOpenDur).SetEase(_lockSplitMoveCurve));
                sequence.Join(lockImageUpRt.DOLocalRotate(new Vector3(0f, 0f, _lockOpenAngle), _lockOpenDur).SetEase(_lockSplitRotateCurve));
            }

            if (lockImageDownRt != null)
            {
                sequence.Join(lockImageDownRt.DOAnchorPos(initialLockImageDownPos + _lockImageDownOffset, _lockOpenDur).SetEase(_lockSplitMoveCurve));
                sequence.Join(lockImageDownRt.DOLocalRotate(new Vector3(0f, 0f, -_lockOpenAngle), _lockOpenDur).SetEase(_lockSplitRotateCurve));
            }

            sequence.Append(fallXTween);
            sequence.Join(fallYTween);
            sequence.Join(fallRotateTween);
            sequence.Join(fallScaleTween);
            sequence.Join(_lockImage.DOFade(0f, _fadeDur).SetEase(_fadeCurve));
            sequence.Join(_lockBackground.DOFade(0f, _fadeDur).SetEase(_fadeCurve));

            sequence.OnComplete(() =>
            {
                isUnlocked = true;
                ApplyUnlockedPresentation();
            });
            sequence.OnKill(() =>
            {
                if (!isUnlocked)
                    ApplyLockedPresentation();
            });

            return sequence;
        }

        void Update()
        {
#if UNITY_EDITOR
            if (_lockImage.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
            {
                DoUnlockAnim().Play();
            }
#endif
        }
    }
}
