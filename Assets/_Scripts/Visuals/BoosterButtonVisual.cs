using System.Collections;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using TMPro;
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
        [SerializeField] private float _scaleFactor;
        [SerializeField] private AnimationCurve _scaleCurve;
        [SerializeField] private float _posXOffset;
        [SerializeField] private float _lockOpenDur;
        [SerializeField] private float _lockOpenAngle;
        [SerializeField] private float _fadeDur;

        //UNLOCK2
        [SerializeField] private Image _glowImage;
        [SerializeField] private float _glowDur;
        [SerializeField] private AnimationCurve _glowCurve;
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
            void reset()
            {
                _lockImage.gameObject.SetActive(false);
                _lockBackground.gameObject.SetActive(false);
                _lockImage.alpha = 1f;
                _lockBackground.color = Color.white;
                //TEST
                _lockImageUp.transform.localRotation = Quaternion.identity;
                _lockImageDown.transform.localRotation = Quaternion.identity;
                _lockImage.transform.localPosition = Vector3.zero;
                _lockImage.transform.localScale = Vector3.one;
                _lockImage.gameObject.SetActive(true);
                _lockBackground.gameObject.SetActive(true);
            }

            _glowImage.gameObject.SetActive(true);
            _glowImage.color = new Color(_glowImage.color.r, _glowImage.color.g, _glowImage.color.b, 0f);

            var sequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            
            // sequence.Append(_lockImage.transform.DOShakePosition(_shakeDur, new Vector3(_posXOffset, 0f, 0f), vibrato: 10, randomness: 90, snapping: false, fadeOut: true))
            //         .Join(_lockImage.transform.DOScaleX(_scaleFactor, _shakeDur).SetEase(_scaleCurve));
            // sequence.Append(_lockImageUp.transform.DOLocalRotate(new Vector3(0f, 0f, _lockOpenAngle), _lockOpenDur).SetEase(Ease.OutSine));
            // sequence.Join(_lockImageDown.transform.DOLocalRotate(new Vector3(0f, 0f, -_lockOpenAngle), _lockOpenDur).SetEase(Ease.OutSine));
            // sequence.Append(_lockImage.DOFade(0f, _fadeDur).SetEase(Ease.OutSine));
            // sequence.Join(_lockBackground.DOFade(0f, _fadeDur).SetEase(Ease.OutSine));
            // sequence.OnComplete(() => reset()).OnKill(() => reset());

            sequence.Append(_lockImage.transform.DOScale(_scaleFactor, _glowDur).SetEase(_scaleCurve).SetRelative());
            sequence.Join(_glowImage.DOFade(1f, _glowDur).SetEase(_glowCurve));
            sequence.JoinCallback(() =>
            {
                _lockImage.transform.DOShakePosition(_shakeDur, new Vector3(_posXOffset, _posXOffset, 0f), vibrato: 10, randomness: 90, snapping: false)
                                    .SetLoops(-1, LoopType.Restart)
                                    .SetTarget(this)
                                    .SetId("Shake");
            });
            sequence.AppendInterval(_shakeDur);
            sequence.Append(_lockImage.DOFade(0f, _fadeDur).SetEase(Ease.OutSine));
            sequence.JoinCallback(() =>
            {
                // _lockImage.gameObject.SetActive(false);
                _unlockParticle.Play();
            });
            
            sequence.OnComplete(() => reset()).OnKill(() => reset());

            return sequence;
        }

        void Update()
        {
            if (_lockImage.gameObject.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
            {
                DoUnlockAnim().Play();
            }
        }
    }
}