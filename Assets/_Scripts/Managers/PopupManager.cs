using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using Assets._Scripts.Visuals;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace Assets._Scripts.Managers
{
    public class PopupManager : Singleton<PopupManager>
    {
        [SerializeField] private Canvas _canvas;

        [Header("Game Popup")]
        [SerializeField] private GameObject _ovelayPanel;
        [SerializeField] private ShopVisualControl _shopPopup;
        [SerializeField] private NoAdsPopupVisual _noAdsPopup;
        [SerializeField] private BundlePurchasePopupVisual _getLifeBundle;
        [SerializeField] private BoosterPurchasePopupVisual _boosterPopup;
        [SerializeField] private LevelFailedVisual _losePopup;
        [SerializeField] private LevelFinishedVisual _winPopup;
        [SerializeField] private LoadingPopupVisual _loadingPopup;
        [SerializeField] private RevivePopupVisual _revivePopup;
        [SerializeField] private SettingPopupVisual _settingPopup;
        [SerializeField] private TutorialPopupVisual _tutorialPopup;
        [SerializeField] private ConfirmationPopup _confirmPopup;

        [Header("Text Popup")]
        [SerializeField] private TextPopupVisual _textPopupPrefab;
        [SerializeField] private int _initAmount;

        private Pooling<TextPopupVisual> _textPopupPool = new();
        private Dictionary<EPopup, GamePopupVisual> _popupDict = new();

        private GamePopupVisual GetPopup(EPopup key) => _popupDict.TryGetValue(key, out var popup) ? popup : null;

        public UnityEvent OnPopupHidden;

        public IEnumerator ShowPopup(EPopup key)
        {
            var popup = GetPopup(key);
            if (popup == null) yield break;
            _ovelayPanel.SetActive(true);
            yield return popup.Show();
        }

        public void ShowBundlePopup(EPopup key, BundleSO bundle)
        {
            var popup = GetPopup(key);
            if (popup == null || popup is not BundlePurchasePopupVisual bundlePopup) 
            {
                Debug.Log("Wrong type of popup");
                return;
            }
            _ovelayPanel.SetActive(true);
            StartCoroutine(bundlePopup.ShowBundle(bundle));
        }

        public IEnumerator ShowTutorial(ETutorial type)
        {
            if (_tutorialPopup == null) yield break;
            _ovelayPanel.SetActive(true);
            yield return _tutorialPopup.ShowTutorial(type);
        }

        public void ShowPopupText(string content, Vector3 worldPos)
        {
            // Chuyển worldPos từ Canvas nguồn sang toạ độ màn hình (screen space) 
            // để popup có thể tự định vị đúng trong Canvas của chính nó
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, worldPos);
            var popup = _textPopupPool.GetItem();
            popup.Pop(content, screenPos, () => _textPopupPool.ReturnItem(popup));
        }

        public IEnumerator ShowConfirmPopup(string content, string confirmContent = "", UnityAction onConfirmed = null, string declineContent = "", UnityAction onDeclined = null)
        {
            if (_confirmPopup == null) yield break;
            _confirmPopup.SetContent(content, confirmContent, declineContent);
            _confirmPopup.SetActions(onConfirmed, onDeclined);
            _ovelayPanel.SetActive(true);
            yield return _confirmPopup.Show();
        }

        public IEnumerator HidePopup(EPopup key)
        {
            var popup = GetPopup(key);
            if (popup == null) yield break;
            yield return popup.Hide();
        }

        protected override void Awake()
        {
            base.Awake();

            _popupDict[EPopup.Shop] = _shopPopup;
            _popupDict[EPopup.Setting] = _settingPopup;
            _popupDict[EPopup.Revive] = _revivePopup;
            _popupDict[EPopup.NoAds] = _noAdsPopup;
            _popupDict[EPopup.GetLife] = _getLifeBundle;
            _popupDict[EPopup.Win] = _winPopup;
            _popupDict[EPopup.Lose] = _losePopup;
            _popupDict[EPopup.Loading] = _loadingPopup;
            _popupDict[EPopup.Booster] = _boosterPopup;
            _popupDict[EPopup.Tutorial] = _tutorialPopup;
            _popupDict[EPopup.Confirmation] = _confirmPopup;

            _textPopupPool = new(_textPopupPrefab, _initAmount, transform);
            
            OnPopupHidden.AddListener(() =>
            {
                if (_popupDict.Values.All(p => !p.IsActive))
                {
                    _ovelayPanel.SetActive(false);
                    if (GameManager.Instance.CurState == EGameState.Pause) GameManager.Instance.ResumeGame();
                }
            });
        }

        void OnDestroy()
        {
            OnPopupHidden.RemoveAllListeners();
        }
    }
}
