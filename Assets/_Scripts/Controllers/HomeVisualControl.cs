using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using Assets._Scripts.Editor;
using UnityEditor.SceneManagement;
#endif

namespace Assets._Scripts.Controllers
{
    public class HomeVisualControl : MonoBehaviour
    {
        [SerializeField] private Canvas _baseCanvas;

        [Header("----Currency----")]
        [SerializeField] private CanvasGroup _currencyCanvasGroup;
        [SerializeField] private RectTransform _currencyHolder;
        [SerializeField] private CoinDisplayVisual _coinDisplay;
        [SerializeField] private HeartDisplayVisual _heartDisplay;

        [Header("----Buttons----")]
        [SerializeField] private GameButtonVisual _settingButton;
        [SerializeField] private GameButtonVisual _noAdsButton;
        [SerializeField] private LevelPlayButton _playButton;
        [SerializeField] private LevelHolderVisual _levelHolder;
        
        [Header("----User Info----")]
        [SerializeField] private RectTransform _userInfoHolder;
        [SerializeField] private RectTransform _avatarRt;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Image _nameBg;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TMP_InputField _nameInputField;
        [SerializeField] private ToggleButtonVisual _renameToggle;
        [SerializeField] private ToggleButtonVisual _changeAvatarToggle;
        [SerializeField] private AvatarSelectorVisual _avatarSelector;
        [SerializeField] private Sprite[] _avatarPool;

        [Header("----Animation----")]
        [SerializeField] private float _collapseExpandDur = 0.3f;
        [SerializeField] private float _holderScaleFactor;
        [SerializeField] private float _holderScaleDur;
        [SerializeField] private AnimationCurve _holderScaleCurve;
        [SerializeField] private float _changeAvatarScaleDelay;
        [SerializeField] private float _changeAvatarToggleScaleDur;
        [SerializeField] private float _changeAvatarToggleRotateDur;
        [SerializeField] private float _avatarExpandCollapseDelay;

        private Vector2 _currencyExpandedSize;
        private Vector2 _userInfoCollapsedSize;
        private Vector2 _userInfoExpandedSize;
        private bool _isUserInfoExpanded;
        private bool _widthsCached = false;
        private Color _nameBgInitialColor;
        private bool _nameBgColorCached = false;

#if UNITY_EDITOR
        [Header("----Debug----")]
        [SerializeField] private InputField _indexInput;
        [SerializeField] private Button _setLevelBtn;
#endif

        void Awake()
        {
            CacheWidthsIfNeeded();
            CacheNameBgColorIfNeeded();
        }

        public void InitVisual()
        {
            _levelHolder.InitVisual(-1);
            _coinDisplay.UpdateVisual();
            _heartDisplay.UpdateVisual();
            _playButton.UpdateVisual();
            _avatarSelector.Init(_avatarPool, (icon) => _avatarImage.sprite = icon);
            SyncNameVisualFromData();
            SyncAvatarVisualFromData();
            ShowCurrency();
        }

        void Start()
        {
#if UNITY_EDITOR
            _indexInput.gameObject.SetActive(true);
            _setLevelBtn.gameObject.SetActive(true);
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

                bool showBoosterSelector = PlayerProgressHelper.CheckUnlockBooster(EBooster.Hint, passMilestone: true);
#if UNITY_EDITOR
                showBoosterSelector |= !DebugFlagToggle.Instance.SkipSelectBoosters;
#endif

                var toPlay = LevelManager.Instance.GetLatestNotClearedLevel();
                if (showBoosterSelector)
                    StartCoroutine(PopupManager.Instance.ShowBoosterSelectPopup(toPlay));
                else
                    GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                    {
                        GameManager.Instance.StartLevel(toPlay, boosters: new EBooster[] {EBooster.ExtraMove, EBooster.Shuffle, EBooster.Hint});
                    });
            });

            _renameToggle.OnToggled.AddListener(isOn =>
            {
                ApplyRenameToggleState(isOn);
            });

            _changeAvatarToggle.OnToggled.AddListener(isOn =>
            {
                ChangeAvatarToggleRotate(isOn ? -180f : 0f);
                if (!isOn)
                {
                    HideAvatarSelector();
                    UserManager.ChangeAvatar(_avatarImage.sprite.name);
                }
                else
                {
                    ShowAvatarSelector();
                }
            });

            _nameInputField.onEndEdit.AddListener(name => UserManager.ChangeName(name));
        }

        void Update()
        {
            if (!TryGetPointerDownPosition(out var screenPosition)) return;

            if (IsScreenPointInside(_userInfoHolder, screenPosition))
            {
                // Debug.Log("Show user info");
                if (!_isUserInfoExpanded)
                    ShowUserInfo();
                return;
            }

            // Debug.Log("Show user currency");
            if (_isUserInfoExpanded)
                ShowCurrency();
        }

        private void CacheWidthsIfNeeded()
        {
            if (_widthsCached) return;

            _currencyExpandedSize = _currencyHolder.sizeDelta;
            _userInfoCollapsedSize = _avatarRt.sizeDelta;
            _userInfoExpandedSize = _userInfoHolder.sizeDelta;
            _widthsCached = true;
        }

        private void CacheNameBgColorIfNeeded()
        {
            if (_nameBgColorCached || _nameBg == null) return;

            _nameBgInitialColor = _nameBg.color;
            _nameBgColorCached = true;
        }

        private Sequence ShowCurrency()
        {
            _isUserInfoExpanded = false;

            void finish()
            {
                _userInfoHolder.sizeDelta = _userInfoCollapsedSize;
                _currencyHolder.sizeDelta = new (_currencyExpandedSize.x, _currencyHolder.rect.height);
                _currencyHolder.transform.localScale = Vector3.one;
                _userInfoHolder.transform.localScale = Vector3.one;

                _changeAvatarToggle.transform.localScale = Vector3.zero;
                _changeAvatarToggle.transform.localRotation = Quaternion.identity;
                _changeAvatarToggle.UpdateToggle(false, false);
                ApplyRenameToggleState(false);
                _renameToggle.UpdateToggle(false, false);
            }

            var currencyHolderSeq = DOTween.Sequence()
                                           .Append(SetRectWidth(_currencyHolder, _currencyExpandedSize.x))
                                           .Join(_currencyHolder.DOScale(Vector3.one, _holderScaleDur).SetEase(_holderScaleCurve));

            var userHolderSeq = DOTween.Sequence()
                                       .Append(SetRectWidth(_userInfoHolder, _userInfoCollapsedSize.x))
                                       .Join(_userInfoHolder.DOScale(Vector3.one, _holderScaleDur).SetEase(Ease.OutQuad))
                                       .Join(_changeAvatarToggle.transform.DOScale(Vector3.zero, _changeAvatarToggleScaleDur).SetEase(Ease.OutQuad))
                                       .Join(HideAvatarSelector());

            DOTween.Kill(this, "ExpandHolder");
                                         
            return DOTween.Sequence()
                          .Append(userHolderSeq)
                          .Join(currencyHolderSeq)
                          .OnKill(finish)
                          .SetUpdate(true)
                          .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                          .SetTarget(this)
                          .SetId("ExpandHolder");
        }

        private Sequence ShowUserInfo()
        {
            _isUserInfoExpanded = true;

            void finish()
            {
                _userInfoHolder.sizeDelta = new (_userInfoExpandedSize.x, _userInfoHolder.rect.height);
                _currencyHolder.sizeDelta = new (0, _currencyHolder.rect.height);
                _currencyHolder.transform.localScale = Vector3.one * .9f;
                _userInfoHolder.transform.localScale = Vector3.one * _holderScaleFactor;
            }

            var currencyHolderSeq = DOTween.Sequence()
                                           .Append(SetRectWidth(_currencyHolder, 0f))
                                           .Join(_currencyHolder.DOScale(Vector3.one * .9f, _holderScaleDur).SetEase(_holderScaleCurve));

            var userHolderSeq = DOTween.Sequence()
                                       .Append(SetRectWidth(_userInfoHolder, _userInfoExpandedSize.x))
                                       .Join(_userInfoHolder.DOScale(Vector3.one * _holderScaleFactor, _holderScaleDur).SetEase(_holderScaleCurve));

            var changeAvatarToggleSeq = DOTween.Sequence()
                                               .Append(_changeAvatarToggle.transform.DOScale(Vector3.one, _changeAvatarToggleScaleDur))
                                               .Join(ChangeAvatarToggleRotate(-360f));

            DOTween.Kill(this, "ExpandHolder");

            return DOTween.Sequence()
                          .Append(currencyHolderSeq)
                          .Join(userHolderSeq)
                          .Insert(_changeAvatarScaleDelay, changeAvatarToggleSeq)
                          .OnKill(finish)
                          .SetUpdate(true)
                          .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                          .SetTarget(this)
                          .SetId("ExpandHolder");
        }

        private Sequence ShowAvatarSelector()
        {
            DOTween.Kill(this, "SelectAvatar");
            return DOTween.Sequence()
                          .Append(SetRectHeight(_userInfoHolder, _userInfoExpandedSize.y))
                          .Insert(_avatarExpandCollapseDelay, _avatarSelector.SetVisible(_avatarExpandCollapseDelay, _collapseExpandDur))
                          .SetTarget(this)
                          .SetId("SelectAvatar")
                          .SetUpdate(true);
        }

        private Sequence HideAvatarSelector()
        {
            DOTween.Kill(this, "SelectAvatar");
            return DOTween.Sequence()
                          .Append(_avatarSelector.SetInvisible(0, _collapseExpandDur))
                          .Insert(_avatarExpandCollapseDelay, SetRectHeight(_userInfoHolder, _userInfoCollapsedSize.y))
                          .SetTarget(this)
                          .SetId("SelectAvatar")
                          .SetUpdate(true);
        }

        private Tween ChangeAvatarToggleRotate(float targetAngle)
        {
            DOTween.Kill(_changeAvatarToggle, "ChangeAvatarToggle");
            return _changeAvatarToggle.transform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), _changeAvatarToggleRotateDur, RotateMode.FastBeyond360)
                                                .SetEase(Ease.OutQuad)
                                                .SetId("ChangeAvatarToggle")
                                                .SetTarget(_changeAvatarToggle)
                                                .SetUpdate(true);
        }

        private void ApplyRenameToggleState(bool isOn)
        {
            if (_nameText == null || _nameInputField == null) return;

            CacheNameBgColorIfNeeded();
            ApplyNameBgValue(isOn);

            if (isOn)
            {
                _nameInputField.SetTextWithoutNotify(_nameText.text);
                _nameText.gameObject.SetActive(false);
                _nameInputField.gameObject.SetActive(true);
                _nameInputField.ActivateInputField();
                _nameInputField.MoveTextEnd(false);
                return;
            }

            _nameText.SetText(_nameInputField.text);
            _nameInputField.DeactivateInputField();
            _nameInputField.gameObject.SetActive(false);
            _nameText.gameObject.SetActive(true);
        }

        private void ApplyNameBgValue(bool isOn)
        {
            if (_nameBg == null || !_nameBgColorCached) return;

            if (!isOn)
            {
                _nameBg.color = _nameBgInitialColor;
                return;
            }

            Color.RGBToHSV(_nameBgInitialColor, out float hue, out float saturation, out _);
            var hiddenColor = Color.HSVToRGB(hue, saturation, 0f);
            hiddenColor.a = _nameBgInitialColor.a;
            _nameBg.color = hiddenColor;
        }

        private void SyncNameVisualFromData()
        {
            var userName = UserManager.CurUser?.Name ?? string.Empty;

            if (_nameText != null)
            {
                _nameText.SetText(userName);
                _nameText.gameObject.SetActive(true);
            }

            if (_nameInputField != null)
            {
                _nameInputField.SetTextWithoutNotify(userName);
                _nameInputField.DeactivateInputField();
                _nameInputField.gameObject.SetActive(false);
            }

            _renameToggle?.UpdateToggle(false, false);
        }

        private void SyncAvatarVisualFromData()
        {
            if (_avatarImage == null) return;

            var avatarSprite = ResolveAvatarSprite(UserManager.CurUser?.AvatarURL);
            if (avatarSprite == null) return;

            _avatarImage.sprite = avatarSprite;
            _avatarImage.preserveAspect = true;
        }

        private Sprite ResolveAvatarSprite(string avatarId)
        {
            if (_avatarPool != null)
            {
                foreach (var avatarSprite in _avatarPool)
                {
                    if (avatarSprite == null) continue;
                    if (string.Equals(avatarSprite.name, avatarId)) return avatarSprite;
                }
            }

            return _avatarImage != null ? _avatarImage.sprite : null;
        }

        private Tween SetRectWidth(RectTransform target, float width)
        {
            return DOTween.To(() => target.rect.width, newWidth =>
            {
                if (target == null) return;
                Vector2 sizeDelta = target.sizeDelta;
                sizeDelta.x = newWidth;
                target.sizeDelta = sizeDelta;
            }, width, _collapseExpandDur).SetEase(Ease.InOutQuad);
        }

        private Tween SetRectHeight(RectTransform target, float height)
        {
            return DOTween.To(() => target.rect.height, newHeight =>
            {
                if (target == null) return;
                Vector2 sizeDelta = target.sizeDelta;
                sizeDelta.y = newHeight;
                target.sizeDelta = sizeDelta;
            }, height, _collapseExpandDur).SetEase(Ease.InOutQuad);
        }

        private bool TryGetPointerDownPosition(out Vector2 screenPosition)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screenPosition = touch.position;
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }

            screenPosition = Vector2.zero;
            return false;
        }

        private bool IsScreenPointInside(RectTransform target, Vector2 screenPosition)
        {
            var cam = _baseCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _baseCanvas.worldCamera;
            return target != null && RectTransformUtility.RectangleContainsScreenPoint(target, screenPosition, cam);
        }
    }
}
