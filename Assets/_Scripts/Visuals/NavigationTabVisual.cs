using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class NavigationTabVisual : GameButtonVisual
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _name;

        [SerializeField] private float _animDuration = 0.25f;
        [SerializeField] private float _offsetY;
        private Vector2 _offsetPos;
        private Vector2 _originPos;
        private RectTransform _iconRt;

        private bool _isSelected = false;

        public void DoOnSelectedAnim()
        {
            if (_isSelected) return;

            _isSelected = true;
            _iconRt.DOKill();
            
            _offsetPos = _originPos + new Vector2(0, _offsetY);
            _iconRt.DOAnchorPos(_offsetPos, _animDuration)
                .SetEase(Ease.InOutQuad)
                .SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable)
                .OnComplete(() =>
                {
                    _name.gameObject.SetActive(true);
                });
            // Debug.Log($"Tab {name} selected");
        }

        public void DoOnDeselectedAnim()
        {   
            if (!_isSelected) return;

            _isSelected = false;
            _iconRt.DOKill();
            _name.gameObject.SetActive(false);
            _iconRt.DOAnchorPos(_originPos, _animDuration).SetEase(Ease.InOutQuad).SetLink(gameObject, LinkBehaviour.CompleteAndKillOnDisable);
            // Debug.Log($"Tab {name} deselected");
        }

        protected override void Awake()
        {
            base.Awake();
            _iconRt = _icon.GetComponent<RectTransform>();
            _originPos = _iconRt.anchoredPosition;
        }
    }
}