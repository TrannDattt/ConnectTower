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
        [SerializeField] protected RectTransform _buttonRt;
        [SerializeField] protected Text _content;
        
        [Header("Settings")]
        [SerializeField] private float _pressedScale = 0.8f;
        [SerializeField] private float _duration = 0.1f;

        public bool IsEnabled {get; protected set;} = true;
        
        public UnityEvent OnClicked = new();

        private Vector3 _originalScale;
        protected Canvas _parentCanvas;

        public void SetEnable(bool isEnabled) => IsEnabled = isEnabled;

        protected virtual void Awake()
        {
            _originalScale = _buttonRt.localScale;
        }

        protected virtual void Start()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
            if (_button != null)
            {
                _button.onClick.AddListener(() => 
                {
                    OnClicked.Invoke();
                    HapticManager.DoLightFeedback();
                    SoundManager.Instance.PlayRandomSFX(IsEnabled ? ESfx.ButtonClicked : ESfx.DisabledButtonClicked);
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
            DOTween.Kill(this, "Click");
            _buttonRt.DOScale(targetScale, _duration)
                     .SetEase(Ease.OutSine)
                     .SetTarget(this)
                     .SetId("Click")
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

        public Vector3 GetCenterPosition()
        {
            return _buttonRt != null ? _buttonRt.TransformPoint(_buttonRt.rect.center) : transform.position;
        }

        public void SetContent(string content)
        {
            if (_content == null) return;
            _content.text = content;
        }

        public void ShowPopupText(string message)
        {
            PopupManager.Instance.ShowPopupText(message, _buttonRt, _parentCanvas);
        }
    }
}