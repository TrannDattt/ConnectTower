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
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _buttonRt;
        
        [Header("Settings")]
        [SerializeField] private float _pressedScale = 0.8f;
        [SerializeField] private float _duration = 0.1f;
        
        public UnityEvent OnClicked = new();

        private Vector3 _originalScale;

        protected virtual void Awake()
        {
            _originalScale = _buttonRt.localScale;
            if (_button == null) _button = GetComponent<Button>();
        }

        protected virtual void Start()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(() => OnClicked.Invoke());
            }
        }

        protected virtual void OnDestroy()
        {
            OnClicked.RemoveAllListeners();
            if (_button != null) _button.onClick.RemoveAllListeners();
            _buttonRt.DOKill();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;

            Scale(_originalScale * _pressedScale);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            ResetScale();
        }

        private void Scale(Vector3 targetScale)
        {
            _buttonRt.DOKill();
            _buttonRt.DOScale(targetScale, _duration).SetEase(Ease.OutSine).SetUpdate(true);
        }

        private void ResetScale()
        {
           Scale(_originalScale);
        }
    }
}