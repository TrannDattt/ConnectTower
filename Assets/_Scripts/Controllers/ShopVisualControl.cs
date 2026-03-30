using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using TMPro;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class ShopVisualControl : GamePopupVisual
    {
        [SerializeField] private CoinDisplayVisual _coinDisplay;
        [SerializeField] private RectTransform _bundleContainer;
        [SerializeField] private GameButtonVisual _settingButton;
        [SerializeField] private SettingPopupVisual _settingPopup;

        public void InitVisual()
        {
            //TODO: Fetch and show all bundles

            var curScene = GameSceneManager.Instance.GetActiveScene();
            _settingButton.gameObject.SetActive(curScene == EGameScene.Menu);
            _closeButton.gameObject.SetActive(curScene == EGameScene.Ingame);
        }

        public override void Show()
        {
            base.Show();

            InitVisual();
        }

        protected override void Start()
        {
            _settingButton?.OnClicked.AddListener(() =>
            {
                _settingPopup?.Show();
            });

            base.Start();
        }
    }
}