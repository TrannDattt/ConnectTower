using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class ToggleButtonVisual : GameButtonVisual
    {
        [SerializeField] private Image _disableIcon;

        public UnityEvent<bool> OnToggled {get; private set;} = new();
        private bool _curState;

        public void UpdateToggle(bool isOn, bool isNotify = true)
        {
            _curState = isOn;
            if (_disableIcon != null) _disableIcon.gameObject.SetActive(!isOn);
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