using System.Collections.Generic;
using Assets._Scripts.Enums;
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
            _homeControl.InitVisual();
            _shopControl.InitVisual();
        }

        public void ChangeTab(EMenuTab tab)
        {
            _tabSM.ChangeState(tab);
        }

        public void OpenShop() => ChangeTab(EMenuTab.Shop);

        void Start()
        {
            HomeState home = new(EMenuTab.Home, _homeTab, _homeControl, _homeControl.InitVisual);
            ShopState shop = new(EMenuTab.Shop, _shopTab, _shopControl, _shopControl.InitVisual);
            RankingState ranking = new(EMenuTab.Scoreboard, null, null, null);

            _tabSM.AddStates(home, shop, ranking);
            _tabSM.SetDefaultState(EMenuTab.Home);
            _tabSM.ChangeToDefault();

            _homeTab.OnClicked.AddListener(() => ChangeTab(EMenuTab.Home));
            _shopTab.OnClicked.AddListener(() => ChangeTab(EMenuTab.Shop));
        }

        public abstract class NavTabState : AState<EMenuTab>
        {
            protected NavigationTabVisual _tabVisual;
            protected MonoBehaviour _tabControl;
            protected UnityAction _onChanged;

            protected NavTabState(EMenuTab key, NavigationTabVisual tabVisual, MonoBehaviour tabControl, UnityAction onChanged) : base(key)
            {
                _tabVisual = tabVisual;
                _tabControl = tabControl;
                _onChanged = onChanged;
            }

            public override void Enter()
            {
                base.Enter();

                _onChanged?.Invoke();
                _tabControl.gameObject.SetActive(true);
                Instance._navBar.DoChangeTabAnim(_tabVisual);
            }

            public override void Exit()
            {
                base.Exit();

                _tabControl.gameObject.SetActive(false);
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