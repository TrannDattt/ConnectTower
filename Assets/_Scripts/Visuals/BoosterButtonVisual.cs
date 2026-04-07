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
        //TODO: Add popup text if button is locked and player click on it

        public bool IsLocked {get; private set;}

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

        private Vector3 _originalIconPos, _originalIconScale;
        public Tween DoOnUseBoosterAnim(Vector3 gatherPoint, System.Action onReachedCenter)
        {
            float duration = .5f;
            float stayDuration = .5f;
            float scaleTime = 1.5f;
            
            _iconImage.transform.DOKill();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_iconImage.transform.DOMove(gatherPoint, duration).SetEase(Ease.OutSine));
            sequence.Join(_iconImage.transform.DOScale(_originalIconScale * scaleTime, duration).SetEase(Ease.OutSine));
            sequence.AppendInterval(stayDuration);
            sequence.OnComplete(() => 
            {
                _iconImage.transform.position = _originalIconPos;
                _iconImage.transform.localScale = _originalIconScale;
                onReachedCenter?.Invoke();
            });
            sequence.OnKill(() =>
            {
                _iconImage.transform.position = _originalIconPos;
                _iconImage.transform.localScale = _originalIconScale;
            });
            
            return sequence;
        }

        public void ShowPopupText()
        {
            //TODO: Show popup text if click to lock button
        }

        protected override void Start()
        {
            base.Start();

            _originalIconPos = _iconImage.transform.position;
            _originalIconScale = _iconImage.transform.localScale;
        }
    }
}