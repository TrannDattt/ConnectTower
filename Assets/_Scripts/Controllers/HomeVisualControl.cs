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
        [SerializeField] private SettingPopupVisual _settingPopup;
        [SerializeField] private GameButtonVisual _noAdsButton;
        [SerializeField] private NoAdsPopupVisual _noAdsPopup;
        [SerializeField] private GameButtonVisual _playButton;
        [SerializeField] private LevelHolderVisual _levelHolder;

        public void InitVisual()
        {
            //TODO: Fetch and show heart amount and time counter
            var allLevels = LevelManager.Instance.GetAllLevel();
            _levelHolder.InitVisual(allLevels);
        }

        void Start()
        {
            _settingButton.OnClicked.AddListener(() =>
            {
                _settingPopup.Show();
            });

            _noAdsButton.OnClicked.AddListener(() =>
            {
                _noAdsPopup.ShowBundle(BundleManager.Instance.GetNoAdsBundle());
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