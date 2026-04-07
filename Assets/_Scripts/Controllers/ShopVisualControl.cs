using System.Collections;
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

        public void InitVisual()
        {
            //TODO: Fetch and show all bundles

            var curScene = GameSceneManager.Instance.GetActiveScene();
            _settingButton.gameObject.SetActive(curScene == EGameScene.None);
            _closeButton.gameObject.SetActive(curScene == EGameScene.Ingame);
            _coinDisplay.UpdateVisual();
        }

        public override IEnumerator Show()
        {
            yield return base.Show();

            InitVisual();
        }

        protected override void Start()
        {
            _settingButton.OnClicked.AddListener(() =>
            {
                PopupManager.Instance.ShowPopup(EPopup.Setting);
            });

            base.Start();
        }
    }
}