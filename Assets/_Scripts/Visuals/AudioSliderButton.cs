using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class AudioSliderButton : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private ToggleButtonVisual _toggleButton;

        public UnityEvent<float> OnValueChanged => _slider.onValueChanged;
        public UnityEvent<bool> OnToggled => _toggleButton.OnToggled;

        public void UpdateToggle(bool isOn, bool isNotify = true) => _toggleButton.UpdateToggle(isOn, isNotify);
        public void UpdateSlider(float value, bool isActive = true)
        {
            _slider.interactable = isActive;
            _slider.SetValueWithoutNotify(value);
        }

        void Awake()
        {
            OnToggled.AddListener((isOn) => _slider.interactable = isOn);
        }
    }
}