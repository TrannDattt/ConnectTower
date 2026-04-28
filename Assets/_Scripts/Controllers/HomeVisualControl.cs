using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using UnityEngine;
using UnityEngine.UI;

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

#if UNITY_EDITOR
        [SerializeField] private InputField _indexInput;
        [SerializeField] private Button _setLevelBtn;
#endif

        public void InitVisual()
        {
            _levelHolder.InitVisual(-1);
            _coinDisplay.UpdateVisual();
            _heartDisplay.UpdateVisual();
            _playButton.UpdateVisual();
        }

        void Start()
        {
#if UNITY_EDITOR
            _setLevelBtn.onClick.AddListener(() =>
            {
                if (!int.TryParse(_indexInput.text, out var index)) return;
                UserManager.UpdateProgress(index, true);
            });
#endif
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
                if (UserManager.CurUser.HeartCount == 0)
                {
                    PopupManager.Instance.ShowBundlePopup(EPopup.GetLife, BundleManager.Instance.GetLifeBundle());
                    return;
                }

                GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                {
                    var curLevel = LevelManager.Instance.GetLatestNotClearedLevel();
                    GameManager.Instance.StartLevel(curLevel);
                });
            });
        }
    }
}