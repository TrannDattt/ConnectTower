using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    [RequireComponent(typeof(RectTransform))]
    public class GameButtonVisual : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] protected Button _button;
        [SerializeField] private RectTransform _buttonRt;
        
        [Header("Settings")]
        [SerializeField] private float _pressedScale = 0.8f;
        [SerializeField] private float _duration = 0.1f;

        private bool _isEnabled = true;
        
        public UnityEvent OnClicked = new();

        private Vector3 _originalScale;

        public void SetEnable(bool isEnabled) => _isEnabled = isEnabled;

        protected virtual void Awake()
        {
            _originalScale = _buttonRt.localScale;
        }

        protected virtual void Start()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(() => 
                {
                    OnClicked.Invoke();
                    SoundManager.Instance.PlayRandomSFX(_isEnabled ? ESfx.ButtonClicked : ESfx.DisabledButtonClicked);
                });
            }
        }

        protected virtual void OnDestroy()
        {
            OnClicked.RemoveAllListeners();
            if (_button != null) _button.onClick.RemoveAllListeners();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            Scale(_originalScale * _pressedScale);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            ResetScale();
        }

        private void Scale(Vector3 targetScale)
        {
            _buttonRt.DOKill();
            _buttonRt.DOScale(targetScale, _duration)
                     .SetEase(Ease.OutSine)
                     .SetUpdate(true);
        }

        private void ResetScale()
        {
           Scale(_originalScale);
        }

        void OnDisable()
        {
            _buttonRt.DOKill();
            _buttonRt.localScale = _originalScale;
        }
    }
}