using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class ToggleButtonVisual : GameButtonVisual
    {
        [SerializeField] private Image _disableIcon;
        [SerializeField] private Sprite _onIcon;
        [SerializeField] private Sprite _offIcon;
        [SerializeField] private bool _changeIcon = false;

        public UnityEvent<bool> OnToggled {get; private set;} = new();
        private bool _curState;

        public void UpdateToggle(bool isOn, bool isNotify = true)
        {
            _curState = isOn;
            if (_disableIcon && !_changeIcon) _disableIcon.gameObject.SetActive(!isOn);
            if (_changeIcon && _onIcon && _offIcon && _buttonIcon) _buttonIcon.sprite = isOn ? _onIcon : _offIcon;
            if (isNotify) OnToggled?.Invoke(_curState);
        }

        protected override void Awake()
        {
            base.Awake();

            OnClicked.AddListener(() => UpdateToggle(!_curState));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnToggled.RemoveAllListeners();
        }
    }
}