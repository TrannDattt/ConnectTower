using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    [RequireComponent(typeof(RectTransform))]
    public class AvatarSelectorButton : GameButtonVisual
    {
        private Action<Sprite> _onSelected;

        public void Init(Sprite avatarSprite, Action<Sprite> onSelected)
        {
            _onSelected = onSelected;

            if (_buttonIcon != null)
            {
                _buttonIcon.sprite = avatarSprite;
                _buttonIcon.preserveAspect = true;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            OnClicked.AddListener(HandleClicked);
        }

        protected override void OnDestroy()
        {
            OnClicked.RemoveListener(HandleClicked);
            base.OnDestroy();
        }

        private void HandleClicked()
        {
            if (_buttonIcon.sprite == null) return;
            _onSelected?.Invoke(_buttonIcon.sprite);
        }
    }
}
