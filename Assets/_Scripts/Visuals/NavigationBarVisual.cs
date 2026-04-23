using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class NavigationBarVisual : MonoBehaviour
    {
        [SerializeField] private RectTransform _tabContainer;
        [SerializeField] private RectTransform _selectedBackground;
        [SerializeField] private float _animDuration = 0.25f;
        [SerializeField] private Ease _easeType = Ease.OutBack;

        private NavigationTabVisual _selectedTab;

        public void DoChangeTabAnim(NavigationTabVisual tab)
        {
            if (_selectedTab == tab) return;

            if (tab == null)
            {
                return;
            }

            bool isFirstTime = _selectedTab == null;
            _selectedTab?.DoOnDeselectedAnim();
            _selectedTab = tab;

            _selectedBackground.DOKill(true);

            // Nếu là lần đầu (khi Start), ta cho nó nhảy thẳng tới (0s) thay vì chạy animation
            float duration = isFirstTime ? 0 : _animDuration;

            // Cập nhật kích thước (nếu bạn muốn)
            // var tabRt = tab.GetComponent<RectTransform>();
            // _selectedBackground.DOSizeDelta(new Vector2(tabRt.rect.width, tabRt.rect.height), duration);

            // Di chuyển tới vị trí Tab
            _selectedBackground.DOMove(tab.transform.position, duration)
                               .SetEase(_easeType)
                               .SetLink(_selectedBackground.gameObject, LinkBehaviour.CompleteAndKillOnDisable);
            
            _selectedTab.DoOnSelectedAnim();
        }

        void Start()
        {
            // Ép Layout Group tính toán ngay lập tức vị trí các con
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tabContainer);            
        }
    }
}