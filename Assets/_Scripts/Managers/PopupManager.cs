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
        private enum PopupPresentationMode
        {
            None,
            Popup,
            Tutorial
        }

        [SerializeField] private GameObject _popupParent;
        [SerializeField] private Canvas _popupCanvas;
        [SerializeField] private Camera _popupCamera;
        [SerializeField] private Image _popupOverlay;
        [SerializeField] private Canvas _tutorialCanvas;
        [SerializeField] private Camera _tutorialCamera;
        [SerializeField] private Image _tutorialOverlay;
        [SerializeField] private RectTransform _holder;
        [SerializeField] private float _overlayFadeDur = .1f;
        [SerializeField] private float _tutorialOverlayBehindPillarDistance = 100f;
        [SerializeField] private float _loadingPopupAutoHideDelay = 2f;

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
        private Tween _overlayTween;
        private bool _isDetachedFromSceneRoot;
        private Coroutine _loadingAutoHideCoroutine;
        private bool _startupLoadingCompleted;
        private float _startupLoadingShownAt = -1f;

        private GamePopupVisual GetPopup(EPopup key) => _popupDict.TryGetValue(key, out var popup) ? popup : null;

        private TutorialPopupVisual GetTutorialPopup()
        {
            if (_tutorialPopup != null)
            {
                return _tutorialPopup;
            }

            if (_popupParent != null)
            {
                _tutorialPopup = _popupParent.GetComponentInChildren<TutorialPopupVisual>(true);
            }

            return _tutorialPopup;
        }

        private void DetachPopupRoots()
        {
            if (_isDetachedFromSceneRoot)
            {
                return;
            }

            if (transform.parent != null)
            {
                transform.SetParent(null, false);
            }

            if (_popupParent != null && _popupParent.transform.parent != null)
            {
                _popupParent.transform.SetParent(null, false);
            }

            _isDetachedFromSceneRoot = true;
        }

        private void SetPresentationMode(PopupPresentationMode mode)
        {
            SetCanvasState(_popupCanvas, mode == PopupPresentationMode.Popup);
            SetCameraState(_popupCamera, mode == PopupPresentationMode.Popup);
            SetCanvasState(_tutorialCanvas, mode == PopupPresentationMode.Tutorial);
            SetCameraState(_tutorialCamera, mode == PopupPresentationMode.Tutorial);
        }

        private static void SetCanvasState(Canvas canvas, bool isActive)
        {
            if (canvas == null)
            {
                return;
            }

            if (canvas.gameObject.activeSelf != isActive)
            {
                canvas.gameObject.SetActive(isActive);
            }

            canvas.enabled = isActive;
        }

        private static void SetCameraState(Camera camera, bool isActive)
        {
            if (camera == null)
            {
                return;
            }

            if (camera.gameObject.activeSelf != isActive)
            {
                camera.gameObject.SetActive(isActive);
            }

            camera.enabled = isActive;
        }

        private Image GetOverlay(PopupPresentationMode mode)
        {
            return mode == PopupPresentationMode.Tutorial && _tutorialOverlay != null
                ? _tutorialOverlay
                : _popupOverlay;
        }

        private void SetOverlayAlpha(Image overlay, float alpha)
        {
            if (overlay == null)
                return;

            var color = overlay.color;
            color.a = alpha;
            overlay.color = color;
        }

        private void ResetOverlay(Image overlay)
        {
            if (overlay == null)
                return;

            overlay.DOKill(false);
            SetOverlayAlpha(overlay, 0f);
            overlay.gameObject.SetActive(false);
        }

        private Tween ShowOverlay(PopupPresentationMode mode)
        {
            _overlayTween?.Kill(false);
            var overlay = GetOverlay(mode);
            SetPresentationMode(mode);
            _popupParent.SetActive(true);
            ResetOverlay(_popupOverlay == overlay ? _tutorialOverlay : _popupOverlay);
            if (overlay == null)
            {
                _overlayTween = DOVirtual.DelayedCall(0f, () => { }).SetUpdate(true);
                return _overlayTween;
            }

            overlay.DOKill(false);
            overlay.gameObject.SetActive(true);
            SetOverlayAlpha(overlay, 0f);
            _overlayTween = overlay.DOFade(.8f, _overlayFadeDur)
                                   .SetEase(Ease.OutQuad)
                                   .SetTarget(overlay)
                                   .SetUpdate(true);
            return _overlayTween;
        }

        private void HideOverlayInstant()
        {
            _overlayTween?.Kill(false);
            _overlayTween = null;
            ResetOverlay(_popupOverlay);
            ResetOverlay(_tutorialOverlay);
            _popupParent.SetActive(false);
            SetPresentationMode(PopupPresentationMode.None);
        }

        private Tween HideOverlay()
        {
            _overlayTween?.Kill(false);
            var activeOverlay = _tutorialCanvas != null && _tutorialCanvas.enabled ? _tutorialOverlay : _popupOverlay;
            if (activeOverlay == null)
            {
                HideOverlayInstant();
                _overlayTween = DOVirtual.DelayedCall(0f, () => { }).SetUpdate(true);
                return _overlayTween;
            }

            activeOverlay.DOKill(false);
            _overlayTween = activeOverlay.DOFade(0f, _overlayFadeDur)
                                       .SetEase(Ease.InQuad)
                                       .SetTarget(activeOverlay)
                                       .SetUpdate(true)
                                       .OnComplete(() =>
                                       {
                                           _overlayTween = null;
                                           ResetOverlay(_popupOverlay);
                                           ResetOverlay(_tutorialOverlay);
                                           _popupParent.SetActive(false);
                                           SetPresentationMode(PopupPresentationMode.None);
                                       });
            return _overlayTween;
        }

        public IEnumerator ShowPopup(EPopup key)
        {
            var popup = GetPopup(key);
            if (popup == null) yield break;
            ShowOverlay(PopupPresentationMode.Popup).Play();
            yield return popup.Show();
        }

        public IEnumerator ShowStartupLoading()
        {
            _startupLoadingCompleted = false;
            _startupLoadingShownAt = Time.realtimeSinceStartup;
            CancelLoadingAutoHide();
            yield return ShowLoadingPopupInternal(scheduleAutoHide: false);
        }

        public void CompleteStartupLoading()
        {
            if (_startupLoadingCompleted)
            {
                return;
            }

            _startupLoadingCompleted = true;
            CancelLoadingAutoHide();

            if (_loadingPopup != null && _loadingPopup.IsActive)
            {
                var elapsed = _startupLoadingShownAt < 0f ? _loadingPopupAutoHideDelay : Time.realtimeSinceStartup - _startupLoadingShownAt;
                var remainingDelay = Mathf.Max(0f, _loadingPopupAutoHideDelay - elapsed);

                if (remainingDelay > 0f)
                {
                    _loadingAutoHideCoroutine = StartCoroutine(HideLoadingPopupAfterDelay(remainingDelay));
                }
                else
                {
                    StartCoroutine(HidePopup(EPopup.Loading));
                }
            }
        }

        public IEnumerator ShowLoadingPopup()
        {
            yield return ShowLoadingPopupInternal(scheduleAutoHide: _startupLoadingCompleted);
        }

        public void ShowBundlePopup(EPopup key, BundleSO bundle)
        {
            var popup = GetPopup(key);
            if (popup == null || popup is not BundlePurchasePopupVisual bundlePopup) 
            {
                Debug.Log("Wrong type of popup");
                return;
            }
            ShowOverlay(PopupPresentationMode.Popup).Play();
            StartCoroutine(bundlePopup.ShowBundle(bundle));
        }

        public IEnumerator ShowTutorial(ETutorial type)
        {
            var tutorialPopup = GetTutorialPopup();
            if (tutorialPopup == null)
            {
                Debug.LogError("Tutorial popup is missing from PopupManager");
                yield break;
            }

            ShowOverlay(PopupPresentationMode.Tutorial).Play();
            EnsureTutorialOverlayBehindPillar();
            yield return tutorialPopup.ShowTutorial(type);
        }

        public void EnsureTutorialPresentationActive()
        {
            SetPresentationMode(PopupPresentationMode.Tutorial);
            if (_popupParent != null)
            {
                _popupParent.SetActive(true);
            }

            if (_tutorialOverlay != null)
            {
                _tutorialOverlay.gameObject.SetActive(true);
            }

            EnsureTutorialOverlayBehindPillar();
        }

        public bool IsFinishedTutorial()
        {
            var tutorialPopup = GetTutorialPopup();
            return tutorialPopup == null || tutorialPopup.IsFinished;
        }

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
            SetPresentationMode(PopupPresentationMode.Popup);
            if (_popupParent != null && !_popupParent.activeSelf)
            {
                _popupParent.SetActive(true);
            }

            Vector2 screenPos = MapUIPosition(target, _holder, fromCanvas, _popupCanvas);
            var popup = _textPopupPool.GetItem();
            popup.Pop(content, screenPos, () => _textPopupPool.ReturnItem(popup));
        }

        public IEnumerator ShowConfirmPopup(string content, Sprite image = null, string confirmContent = "", UnityAction onConfirmed = null, string declineContent = "", UnityAction onDeclined = null)
        {
            if (_confirmPopup == null) yield break;
            _confirmPopup.SetContent(content, image, confirmContent, declineContent);
            _confirmPopup.SetActions(onConfirmed, onDeclined);
            ShowOverlay(PopupPresentationMode.Popup).Play();
            yield return _confirmPopup.Show();
        }

        public IEnumerator ShowBoosterSelectPopup(LevelRuntimeData levelData)
        {
            if (_boosterSelectPopup == null) yield break;
            ShowOverlay(PopupPresentationMode.Popup).Play();
            yield return _boosterSelectPopup.ShowSelector(levelData);
        }

        public IEnumerator HidePopup(EPopup key)
        {
            if (key == EPopup.Loading)
            {
                CancelLoadingAutoHide();
            }

            var popup = GetPopup(key);
            if (popup == null) yield break;
            yield return popup.Hide();
        }

        public bool IsPopupActive(EPopup key)
        {
            var popup = GetPopup(key);
            return popup != null && popup.IsActive;
        }

        public bool IsPopupVisible(EPopup key)
        {
            var popup = GetPopup(key);
            return popup != null && popup.gameObject.activeInHierarchy;
        }

        private IEnumerator ShowLoadingPopupInternal(bool scheduleAutoHide)
        {
            if (_loadingPopup == null)
            {
                yield break;
            }

            CancelLoadingAutoHide();

            if (!_loadingPopup.IsActive)
            {
                ShowOverlay(PopupPresentationMode.Popup).Play();
                yield return _loadingPopup.Show();
            }
            else
            {
                ShowOverlay(PopupPresentationMode.Popup).Play();
            }

            if (scheduleAutoHide)
            {
                _loadingAutoHideCoroutine = StartCoroutine(HideLoadingPopupAfterDelay(_loadingPopupAutoHideDelay));
            }
        }

        private IEnumerator HideLoadingPopupAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            _loadingAutoHideCoroutine = null;

            if (_loadingPopup != null && _loadingPopup.IsActive)
            {
                yield return HidePopup(EPopup.Loading);
            }
        }

        private void CancelLoadingAutoHide()
        {
            if (_loadingAutoHideCoroutine == null)
            {
                return;
            }

            StopCoroutine(_loadingAutoHideCoroutine);
            _loadingAutoHideCoroutine = null;
        }

        public Tween ChangeOverlayOpacity(float value, float duration, Ease ease)
        {
            var activeOverlay = _tutorialCanvas != null && _tutorialCanvas.enabled ? _tutorialOverlay : _popupOverlay;
            return activeOverlay != null
                ? activeOverlay.DOFade(value, duration).SetEase(ease)
                : DOVirtual.DelayedCall(0f, () => { }).SetEase(ease).SetUpdate(true);
        }

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
            {
                return;
            }

            DetachPopupRoots();

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

            HideOverlayInstant();

            _textPopupPool = new(_textPopupPrefab, _initAmount, _holder);

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

        private void EnsureTutorialOverlayBehindPillar()
        {
            if (_tutorialOverlay == null || _tutorialCamera == null)
            {
                return;
            }

            if (!_tutorialOverlay.gameObject.activeInHierarchy || !_tutorialCamera.isActiveAndEnabled)
            {
                return;
            }

            if (BoardController.Instance == null)
            {
                return;
            }

            var pillars = BoardController.Instance.GetAllPillars();
            if (pillars == null || pillars.Count == 0)
            {
                return;
            }

            var pillar = pillars
                .Where(p => p != null)
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();
            if (pillar == null)
            {
                return;
            }

            var overlayTransform = _tutorialOverlay.rectTransform;
            var cameraTransform = _tutorialCamera.transform;
            var pillarDepth = Vector3.Dot(pillar.transform.position - cameraTransform.position, cameraTransform.forward);
            var targetDepth = pillarDepth + _tutorialOverlayBehindPillarDistance;
            var currentDepth = Vector3.Dot(overlayTransform.position - cameraTransform.position, cameraTransform.forward);
            var depthOffset = targetDepth - currentDepth;

            if (Mathf.Approximately(depthOffset, 0f))
            {
                return;
            }

            overlayTransform.position += cameraTransform.forward * depthOffset;
        }
    }

    public struct PopupHiddenEvent : IEvent
    {
    }
}
