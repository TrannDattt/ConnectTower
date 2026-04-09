using Assets._Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class BoosterButtonVisual : GameButtonVisual
    {
        [SerializeField] private Image _lockImage;
        [SerializeField] private Image _lockBackground;
        [SerializeField] private GameObject _baseContent;
        [SerializeField] private Text _countText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _getMoreImage;

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
            _lockImage.gameObject.SetActive(true);
            _lockBackground.gameObject.SetActive(true);
            _baseContent.SetActive(false);
        }

        public void Unlock()
        {
            IsLocked = false;
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
        public Tween DoOnUseBoosterAnim(Vector3 gatherPoint, System.Action onReachedCenter)
        {
            if (_inAnim) return null;

            _inAnim = true;
            _button.interactable = false;
            float duration = .5f;
            float stayDuration = .5f;
            float scaleTime = 1.2f;
            
            DOTween.Kill(gameObject);

            void reset()
            {
                _iconImage.transform.localPosition = _originalIconLocalPos;
                _iconImage.transform.localScale = _originalIconScale;
                _button.interactable = true;
            }

            Sequence sequence = DOTween.Sequence().SetTarget(gameObject).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            sequence.Append(_iconImage.transform.DOMove(gatherPoint, duration).SetEase(Ease.OutSine));
            sequence.Join(_iconImage.transform.DOScale(_originalIconScale * scaleTime, duration).SetEase(Ease.OutSine));
            sequence.AppendInterval(stayDuration);
            sequence.OnKill(() => 
            {
                reset();
                _inAnim = false;
            });
            sequence.OnComplete(() => 
            {
                onReachedCenter?.Invoke();
            });
            
            return sequence;
        }

        public void ShowPopupText()
        {
            PopupManager.Instance.ShowPopupText("Locked", transform.position);
        }

        protected override void Start()
        {
            base.Start();

            _originalIconLocalPos = _iconImage.transform.localPosition;
            _originalIconScale = _iconImage.transform.localScale;
        }
    }
}