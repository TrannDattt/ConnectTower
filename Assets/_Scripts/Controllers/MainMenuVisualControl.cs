using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Controllers
{
    public class MainMenuVisualControl : Singleton<MainMenuVisualControl>
    {
        [SerializeField] private HomeVisualControl _homeControl;
        [SerializeField] private ShopVisualControl _shopControl;
        [SerializeField] private NavigationBarVisual _navBar;
        [SerializeField] private NavigationTabVisual _homeTab;
        [SerializeField] private NavigationTabVisual _shopTab;
        [SerializeField] private NavigationTabVisual _rankTab;

        [SerializeField] private float _changeTabDur;
        [SerializeField] private float _tabControlOffsetX;

        private StateMachine<EMenuTab> _tabSM = new();

        public void InitVisual()
        {
            Debug.Log($"Init main menu");
            _homeControl.InitVisual();
            _shopControl.InitVisual();
        }

        public void ChangeTab(EMenuTab tab)
        {
            _tabSM.TryGetState(tab, out var toChange);
            if (toChange == null) return;

            var nextTab = toChange as NavTabState;
            if (nextTab.TabControl == null)
            {
                nextTab.TabVisual.ShowPopupText("Coming soon");
                return;
            }
            
            var preTab = _tabSM.CurrentState as NavTabState;
            _tabSM.ChangeState(tab);

            DoChangeTabAnim(preTab, nextTab);
        }

        private void DoChangeTabAnim(NavTabState from, NavTabState to)
        {
            if (to == null) return;
            Debug.Log("Pass 1");

            _navBar.DoChangeTabAnim(to.TabVisual);
            to.TabControl.gameObject.SetActive(true);

            if (from == null || from == to) return;
            Debug.Log("Pass 2");

            DOTween.Kill(this);

            bool slideToRight = (_tabSM.GetStateIndex(from.Key) - _tabSM.GetStateIndex(to.Key)) > 0;
            to.TabControl.GetComponent<RectTransform>().anchoredPosition = new Vector3(_tabControlOffsetX * (slideToRight ? -1 : 1), 0, 0);
            from.TabControl.transform.SetAsFirstSibling();

            var sequence = DOTween.Sequence().SetTarget(this);
            sequence.Append(from.TabControl.GetComponent<RectTransform>().DOAnchorPosX(_tabControlOffsetX * (slideToRight ? 1 : -1), _changeTabDur).SetEase(Ease.OutQuad));
            sequence.Join(to.TabControl.GetComponent<RectTransform>().DOAnchorPosX(0, _changeTabDur).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() => from.TabControl.gameObject.SetActive(false));

            sequence.Play();
        }

        public void OpenShop() => ChangeTab(EMenuTab.Shop);

        void Start()
        {
            HomeState home = new(EMenuTab.Home, _homeTab, _homeControl, _homeControl.InitVisual);
            ShopState shop = new(EMenuTab.Shop, _shopTab, _shopControl, _shopControl.InitVisual);
            RankingState ranking = new(EMenuTab.Ranking, _rankTab, null, null);

            _homeTab.OnClicked.AddListener(() => ChangeTab(EMenuTab.Home));
            _shopTab.OnClicked.AddListener(() => ChangeTab(EMenuTab.Shop));
            _rankTab.OnClicked.AddListener(() => ChangeTab(EMenuTab.Ranking));

            _tabSM.AddStates(shop, home, ranking);
            _tabSM.SetDefaultState(EMenuTab.Home);
            _tabSM.ChangeToDefault();
        }

        public abstract class NavTabState : AState<EMenuTab>
        {
            public NavigationTabVisual TabVisual {get; private set;}
            public MonoBehaviour TabControl {get; private set;}
            protected UnityAction _onChanged;

            protected NavTabState(EMenuTab key, NavigationTabVisual tabVisual, MonoBehaviour tabControl, UnityAction onChanged) : base(key)
            {
                TabVisual = tabVisual;
                TabControl = tabControl;
                _onChanged = onChanged;

                TabVisual.SetEnable(TabControl != null);
                TabControl?.gameObject.SetActive(false);
            }

            public override void Enter()
            {
                base.Enter();

                _onChanged?.Invoke();
            }

            public override void Exit()
            {
                base.Exit();
            }
        }

        public class HomeState : NavTabState
        {
            public HomeState(EMenuTab key, NavigationTabVisual tabVisual, MonoBehaviour tabControl, UnityAction onChanged) : base(key, tabVisual, tabControl, onChanged)
            {
            }
        }

        public class ShopState : NavTabState
        {
            public ShopState(EMenuTab key, NavigationTabVisual tabVisual, MonoBehaviour tabControl, UnityAction onChanged) : base(key, tabVisual, tabControl, onChanged)
            {
            }
        }

        public class RankingState : NavTabState
        {
            public RankingState(EMenuTab key, NavigationTabVisual tabVisual, MonoBehaviour tabControl, UnityAction onChanged) : base(key, tabVisual, tabControl, onChanged)
            {
            }
        }
    }
}