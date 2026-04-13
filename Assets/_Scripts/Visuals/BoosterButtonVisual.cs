using System.Collections;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class BoosterButtonVisual : GameButtonVisual
    {
        [SerializeField] private EBooster Key;
        [SerializeField] private Image _lockImage;
        [SerializeField] private Image _lockBackground;
        [SerializeField] private GameObject _baseContent;
        [SerializeField] private Text _countText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _getMoreImage;
        [SerializeField] private BoosterButtonEffectVisual _effectVisual;

        public bool IsLocked {get; private set;}
        private bool _inAnim = false;
        public bool IsInAnim => _inAnim;

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

        private Vector3 _originalIconLocalPos, _originalIconScale;
        public IEnumerator DoOnUseBoosterAnim(BoosterRuntimeData data, Vector3 gatherPoint)
        {
            _inAnim = true;
            _button.interactable = false;
            float buttonMoveDuration = .5f;
            float effectDuration = _effectVisual != null ? _effectVisual.GetTotalDuration() : 0;
            float scaleTime = 2.5f;

            void reset()
            {
                _iconImage.transform.localPosition = _originalIconLocalPos;
                _iconImage.transform.localScale = _originalIconScale;
                _button.interactable = true;
                _inAnim = false;
            }

            Sequence beginSequence = DOTween.Sequence()
                                            .Append(_iconImage.transform.DOMove(gatherPoint, buttonMoveDuration).SetEase(Ease.OutSine))
                                            .Join(_iconImage.transform.DOScale(_originalIconScale * scaleTime, buttonMoveDuration).SetEase(Ease.OutSine));

            Sequence endSequence = DOTween.Sequence()
                                          .Append(_iconImage.transform.DOLocalMove(_originalIconLocalPos, buttonMoveDuration).SetEase(Ease.OutSine))
                                          .Join(_iconImage.transform.DOScale(_originalIconScale, buttonMoveDuration).SetEase(Ease.OutSine));

            Sequence mainSequence = DoBoosterAnim(data);

            Sequence masterSequence = DOTween.Sequence().SetTarget(gameObject).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            masterSequence.Append(beginSequence).Append(mainSequence)
            .SetDelay(.5f).Append(endSequence)
            .OnKill(() =>
            {
                reset();
            })
            .OnComplete(() => 
            {
                reset();
            });
            
            yield return masterSequence.WaitForCompletion();
        }

        private Sequence DoBoosterAnim(BoosterRuntimeData data)
        {
            var boosterSFX = data.Key switch
            {
                EBooster.ExtraMove => ESfx.ExtraMove,
                EBooster.Shuffle => ESfx.Shuffle,
                EBooster.Hint => ESfx.Hint,
                _ => ESfx.None
            };
            SoundManager.Instance.PlayRandomSFX(boosterSFX);

            return DOTween.Sequence()
                          .Append(data.DoBoosterAnim())
                          .Join(data.DoBoosterButtonAnim(_iconImage));
        }

        public void ShowPopupText()
        {
            PopupManager.Instance.ShowPopupText("Locked", _originalIconLocalPos);
        }

        protected override void Start()
        {
            base.Start();

            _originalIconLocalPos = _iconImage.transform.localPosition;
            _originalIconScale = _iconImage.transform.localScale;
        }
    }

    [RequireComponent(typeof(BoosterButtonVisual))]
    public abstract class BoosterButtonEffectVisual : MonoBehaviour
    {
        public abstract Sequence DoEffectAnim(Image target);
        public abstract float GetTotalDuration();
    }
}