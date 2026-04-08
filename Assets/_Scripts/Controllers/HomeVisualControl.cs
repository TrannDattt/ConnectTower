using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class HomeVisualControl : MonoBehaviour
    {
        [SerializeField] private CoinDisplayVisual _coinDisplay;
        [SerializeField] private HeartDisplayVisual _heartDisplay;
        [SerializeField] private GameButtonVisual _settingButton;
        [SerializeField] private GameButtonVisual _noAdsButton;
        [SerializeField] private LevelPlayButton _playButton;
        [SerializeField] private LevelHolderVisual _levelHolder;

        public void InitVisual()
        {
            var allLevels = LevelManager.Instance.GetAllLevels();
            _levelHolder.InitVisual(allLevels);
            _coinDisplay.UpdateVisual();
            _heartDisplay.UpdateVisual();
            _playButton.UpdateVisual();
        }

        void Start()
        {
            _settingButton.OnClicked.AddListener(() =>
            {
                StartCoroutine(PopupManager.Instance.ShowPopup(EPopup.Setting));
            });

            _noAdsButton.OnClicked.AddListener(() =>
            {
                PopupManager.Instance.ShowBundlePopup(EPopup.NoAds, BundleManager.Instance.GetNoAdsBundle());
                // _noAdsPopup.ShowBundle(BundleManager.Instance.GetNoAdsBundle());
            });

            _playButton.OnClicked.AddListener(() => 
            {
                GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                {
                    var curLevel = LevelManager.Instance.GetLatestNotClearedLevel();
                    GameManager.Instance.StartLevel(curLevel);
                });
            });
        }
    }
}