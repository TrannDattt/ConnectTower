using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Assets._Scripts.Managers
{
    public class PopupManager : Singleton<PopupManager>
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _holder;
        [SerializeField] private Image _ovelayPanel;
        [SerializeField] private float _overlayFadeDur = .1f;

        [Header("Game Popup")]
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
        [SerializeField] private BoosterSelectPopupVisual _boosterSelectPopup;

        [Header("Text Popup")]
        [SerializeField] private TextPopupVisual _textPopupPrefab;
        [SerializeField] private int _initAmount;

        private Pooling<TextPopupVisual> _textPopupPool = new();
        private Dictionary<EPopup, GamePopupVisual> _popupDict = new();
        private EventBinding<PopupHiddenEvent> _popupHiddenBinding;

        private GamePopupVisual GetPopup(EPopup key) => _popupDict.TryGetValue(key, out var popup) ? popup : null;

        private Tween ShowOverlay()
        {
            _ovelayPanel.gameObject.SetActive(true);
            return _ovelayPanel.DOFade(.8f, _overlayFadeDur)
                               .SetEase(Ease.OutQuad)
                               .SetTarget(_ovelayPanel)
                               .SetUpdate(true);
        }

        private Tween HideOverlay()
        {
            return _ovelayPanel.DOFade(0f, _overlayFadeDur)
                               .SetEase(Ease.InQuad)
                               .SetTarget(_ovelayPanel)
                               .SetUpdate(true)
                               .OnComplete(() => _ovelayPanel.gameObject.SetActive(false));
        }

        public IEnumerator ShowPopup(EPopup key)
        {
            var popup = GetPopup(key);
            if (popup == null) yield break;
            ShowOverlay().Play();
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
            ShowOverlay().Play();
            StartCoroutine(bundlePopup.ShowBundle(bundle));
        }

        public IEnumerator ShowTutorial(ETutorial type)
        {
            if (_tutorialPopup == null) yield break;
            ShowOverlay().Play();
            yield return _tutorialPopup.ShowTutorial(type);
        }

        public bool IsFinishedTutorial() => _tutorialPopup == null || _tutorialPopup.IsFinished;

        private Vector2 MapUIPosition(RectTransform sourceUI, RectTransform targetParent, Canvas canvasA, Canvas canvasB)
        {
            // 1. Lấy camera tương ứng với từng Canvas (Nếu là Overlay thì camera sẽ là null)
            Camera camA = canvasA.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvasA.worldCamera;
            Camera camB = canvasB.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvasB.worldCamera;

            // 2. Chuyển vị trí của sourceUI sang tọa độ màn hình (Screen Point)
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(camA, sourceUI.position);

            // 3. Chuyển tọa độ màn hình đó về tọa độ Local của targetParent (Object cha ở Canvas B)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent, screenPos, camB, out Vector2 localPos);

            return localPos;
        }

        public void ShowPopupText(string content, RectTransform target, Canvas fromCanvas)
        {
            Vector2 screenPos = MapUIPosition(target, _holder, fromCanvas, _canvas);
            var popup = _textPopupPool.GetItem();
            popup.Pop(content, screenPos, () => _textPopupPool.ReturnItem(popup));
        }

        public IEnumerator ShowConfirmPopup(string content, string confirmContent = "", UnityAction onConfirmed = null, string declineContent = "", UnityAction onDeclined = null)
        {
            if (_confirmPopup == null) yield break;
            _confirmPopup.SetContent(content, confirmContent, declineContent);
            _confirmPopup.SetActions(onConfirmed, onDeclined);
            ShowOverlay().Play();
            yield return _confirmPopup.Show();
        }

        public IEnumerator ShowBoosterSelectPopup(LevelRuntimeData levelData)
        {
            if (_boosterSelectPopup == null) yield break;
            ShowOverlay().Play();
            yield return _boosterSelectPopup.ShowSelector(levelData);
        }

        public IEnumerator HidePopup(EPopup key)
        {
            var popup = GetPopup(key);
            if (popup == null) yield break;
            yield return popup.Hide();
        }

        public Tween ChangeOverlayOpacity(float value, float duration, Ease ease)
        {
            return _ovelayPanel.DOFade(value, duration).SetEase(ease);
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
            _popupDict[EPopup.BoosterSelect] = _boosterSelectPopup;

            _textPopupPool = new(_textPopupPrefab, _initAmount, transform);

            _popupHiddenBinding = new(() =>
            {
                if (_popupDict.Values.All(p => !p.IsActive))
                {
                    HideOverlay().Play();
                    if (GameManager.Instance.CurState == EGameState.Pause) GameManager.Instance.ResumeGame();
                }
            });
            EventBus<PopupHiddenEvent>.Subscribe(_popupHiddenBinding);
        }

        void OnDestroy()
        {
            EventBus<PopupHiddenEvent>.Unsubscribe(_popupHiddenBinding);
        }
    }

    public struct PopupHiddenEvent : IEvent
    {
    }
}
