using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class AudioSliderButton : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private CanvasGroup _sliderCanvasGroup;
        [SerializeField] private ToggleButtonVisual _toggleButton;

        public UnityEvent<float> OnValueChanged => _slider.onValueChanged;
        public UnityEvent<bool> OnToggled => _toggleButton.OnToggled;

        public void UpdateToggle(bool isOn, bool isNotify = true) => _toggleButton.UpdateToggle(isOn, isNotify);
        
        public void UpdateSlider(float value, bool isActive = true)
        {
            SetSliderState(isActive);
            _slider.SetValueWithoutNotify(value);
        }

        private void SetSliderState(bool isActive)
        {
            _slider.interactable = isActive;
            _sliderCanvasGroup.alpha = isActive ? 1f : 0.4f;
        }

        void Awake()
        {
            OnToggled.AddListener((isOn) => SetSliderState(isOn));
        }
    }
}