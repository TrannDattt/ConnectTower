using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using Assets._Scripts.Visuals;
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

        private StateMachine<EMenuTab> _tabSM = new();

        // TODO: Provide user data
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

            var navState = toChange as NavTabState;
            if (navState.TabControl == null)
            {
                PopupManager.Instance.ShowPopupText("Coming soon", navState.TabVisual.transform.position);
                return;
            }
            
            _tabSM.ChangeState(tab);
        }

        public void OpenShop() => ChangeTab(EMenuTab.Shop);

        void Start()
        {
            HomeState home = new(EMenuTab.Home, _homeTab, _homeControl, _homeControl.InitVisual);
            ShopState shop = new(EMenuTab.Shop, _shopTab, _shopControl, _shopControl.InitVisual);
            RankingState ranking = new(EMenuTab.Ranking, _rankTab, null, null);

            _tabSM.AddStates(home, shop, ranking);
            _tabSM.SetDefaultState(EMenuTab.Home);
            _tabSM.ChangeToDefault();

            _homeTab.OnClicked.AddListener(() => ChangeTab(EMenuTab.Home));
            _shopTab.OnClicked.AddListener(() => ChangeTab(EMenuTab.Shop));
            _rankTab.OnClicked.AddListener(() => ChangeTab(EMenuTab.Ranking));
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
            }

            public override void Enter()
            {
                base.Enter();

                TabControl.gameObject.SetActive(true);
                Instance._navBar.DoChangeTabAnim(TabVisual);
                _onChanged?.Invoke();
            }

            public override void Exit()
            {
                base.Exit();

                TabControl.gameObject.SetActive(false);
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