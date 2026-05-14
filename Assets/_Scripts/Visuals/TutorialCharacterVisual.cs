using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TutorialCharacterVisual : MonoBehaviour
    {
        [SerializeField] private RectTransform _rt;

        // IDLING
        [SerializeField] private float _idleMoveDur;
        [SerializeField] private Vector2 _idleMoveOffset;
        [SerializeField] private AnimationCurve _idleMoveCurve;
        [SerializeField] private float _idleRotateDur;
        [SerializeField] private Vector3 _idleRotateAngle;
        [SerializeField] private AnimationCurve _idleRotateCurve;

        // TALKING
        [SerializeField] private Text _dialogText;
        [SerializeField] private Image _dialogBox;
        [SerializeField] private float _dialogDisplayDur = 1f;
        [SerializeField] private Vector3 _baseRotateOffset;
        [SerializeField] private AnimationCurve _baseRotateCurve;
        [SerializeField] private float _baseScaleFactor;
        [SerializeField] private AnimationCurve _baseScaleCurve;

        // POINTING
        [SerializeField] private CanvasGroup _characterGroup;
        [SerializeField] private float _fadeDur;
        [SerializeField] private Image _characterBase;
        [SerializeField] private GameObject _characterHand;

        // MOVING
        [SerializeField] private float _moveDur;

        public bool IsTalking { get; private set; }

        private Tween _activeTalkTween;
        private string _currentMessage;

        public Tween Show()
        {
            _characterGroup.gameObject.SetActive(true);
            return _characterGroup.DOFade(1f, _fadeDur)
                                 .SetEase(Ease.OutQuad)
                                 .SetTarget(gameObject)
                                 .SetUpdate(true)
                                 .OnKill(() =>
                                 {
                                     _characterGroup.alpha = 1f;
                                 });
        }

        public Tween Hide()
        {
            return _characterGroup.DOFade(0f, _fadeDur)
                                  .SetEase(Ease.InQuad)
                                  .SetTarget(gameObject)
                                  .SetUpdate(true)
                                  .OnComplete(() => _characterGroup.gameObject.SetActive(false))
                                  .OnKill(() =>
                                  {
                                      _characterGroup.gameObject.SetActive(false);
                                      _characterGroup.alpha = 0;
                                  });
        }

        private Vector3 _baseOriginScale;
        private Quaternion _baseOriginRotation;

        private void Idle()
        {
            // Floating
            // DOTween.Kill(this);
            var sequence = DOTween.Sequence().SetTarget(this).SetUpdate(true).SetRelative().SetId("Idle");
            sequence.Append(_characterBase.rectTransform.DOAnchorPos(_idleMoveOffset, _idleMoveDur).SetEase(_idleMoveCurve).SetLoops(int.MaxValue, LoopType.Yoyo));
            sequence.Join(_characterBase.transform.DORotate(_idleRotateAngle, _idleRotateDur).SetEase(_idleRotateCurve).SetLoops(int.MaxValue, LoopType.Restart));
            sequence.OnKill(resetBase);
            void resetBase()
            {
                _characterBase.rectTransform.anchoredPosition = Vector2.zero;
                _characterBase.transform.rotation = _baseOriginRotation;
            }
            sequence.Play();
        }

        public Sequence Talk(string message, TweenCallback onFinishTalking = null)
        {
            DOTween.Kill(this, "Talk");
            DOTween.Kill(this, "Idle");

            // Setup
            _currentMessage = message;
            IsTalking = true;
            _dialogText.text = "";
            _dialogText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _dialogText.verticalOverflow = VerticalWrapMode.Overflow;
            _dialogBox.gameObject.SetActive(true);
            ResizeDialogBoxForMessage(message);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_dialogBox.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_dialogText.rectTransform);

            var masterSequence = DOTween.Sequence().SetTarget(this).SetUpdate(true).SetId("Talk");

            //TODO: Resize box and show text
            var dialogSequence = DOTween.Sequence();
            dialogSequence.Append(_dialogText.DOText(message, _dialogDisplayDur));
            dialogSequence.OnKill(() => _dialogText.text = message);

            // Character bouncing
            void resetBase()
            {
                _characterBase.transform.localScale = _baseOriginScale;
                _characterBase.transform.rotation = _baseOriginRotation;
                Idle();
            }
            var baseSequence = DOTween.Sequence();
            baseSequence.Append(_characterBase.transform.DOScale(Vector3.one * _baseScaleFactor, _dialogDisplayDur).SetEase(_baseScaleCurve).SetRelative());
            baseSequence.Join(_characterBase.transform.DOLocalRotate(_baseRotateOffset, _dialogDisplayDur).SetEase(_baseRotateCurve).SetRelative());
            baseSequence.OnComplete(resetBase).OnKill(resetBase);

            _activeTalkTween = masterSequence.Append(dialogSequence)
                                           .Join(baseSequence)
                                           .OnComplete(() =>
                                           {
                                               IsTalking = false;
                                               _activeTalkTween = null;
                                               onFinishTalking?.Invoke();
                                           })
                                           .OnKill(() =>
                                           {
                                               _dialogText.text = _currentMessage;
                                               IsTalking = false;
                                               _activeTalkTween = null;
                                               onFinishTalking?.Invoke();
                                           });

            return (Sequence)_activeTalkTween;
        }

        private void ResizeDialogBoxForMessage(string message)
        {
            var rectTransform = _dialogBox.rectTransform;
            if (rectTransform == null)
            {
                return;
            }

            var safeMessage = string.IsNullOrEmpty(message) ? " " : message;
            var fontSize = _dialogText.fontSize;
            var textRect = _dialogText.rectTransform;
            var textInsetX = Mathf.Max(0f, -textRect.sizeDelta.x);
            var textInsetY = Mathf.Max(0f, -textRect.sizeDelta.y);
            var horizontalPadding = Mathf.Max(textInsetX, fontSize * 0.8f);
            var verticalPadding = Mathf.Max(textInsetY, fontSize * 0.4f);
            var baseSettings = _dialogText.GetGenerationSettings(Vector2.zero);
            var canvas = _dialogBox.canvas != null ? _dialogBox.canvas.rootCanvas : null;
            var canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
            var maxWidth = canvasRect != null ? canvasRect.rect.width * 0.6f : Screen.width * 0.6f;
            maxWidth = Mathf.Max(maxWidth, fontSize * 8f);

            // Grow width first; when width reaches 60% screen/canvas, extra content grows height via wrapping.
            var singleLineWidth = _dialogText.cachedTextGeneratorForLayout.GetPreferredWidth(safeMessage, baseSettings) / _dialogText.pixelsPerUnit;
            var width = Mathf.Clamp(singleLineWidth + (horizontalPadding * 2f), fontSize * 6f, maxWidth);

            var wrappedSettings = baseSettings;
            wrappedSettings.horizontalOverflow = HorizontalWrapMode.Wrap;
            wrappedSettings.verticalOverflow = VerticalWrapMode.Overflow;
            var innerWidth = Mathf.Max(1f, width - (horizontalPadding * 2f));
            wrappedSettings.generationExtents = new Vector2(innerWidth, 0f);
            var preferredWrappedHeight = _dialogText.cachedTextGeneratorForLayout.GetPreferredHeight(safeMessage, wrappedSettings) / _dialogText.pixelsPerUnit;
            var height = Mathf.Max(preferredWrappedHeight + (verticalPadding * 2f), fontSize + (verticalPadding * 2f));

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public void StopTalk(bool disableDialog)
        {
            _dialogBox.gameObject.SetActive(!disableDialog);
            DOTween.Kill(this, "Talk");
            IsTalking = false;
            _activeTalkTween = null;
            Idle();
        }

        public void CompleteTalk()
        {
            if (_activeTalkTween == null || !_activeTalkTween.IsActive() || !IsTalking) return;

            _activeTalkTween.Kill();
        }

        public void PointAt(Vector3 worldPos)
        {
            // Setup
            _characterHand.gameObject.SetActive(true);

            //TODO: Hand move in a parabol-path and rotate at oposite direction
        }

        public void StopPoint(bool withAnim)
        {
            if (!withAnim)
            {
                _characterHand.gameObject.SetActive(false);
            }

            //TODO: Retreat hand back to base
        }

        public Tween Move(Vector2 anchorPos)
        {
            StopTalk(true);
            return _rt.DOAnchorPos(anchorPos, _moveDur).SetEase(Ease.OutQuad).SetUpdate(true).SetTarget(this).SetId("Move");
        }

        void Awake()
        {
            _baseOriginScale = _characterBase.transform.localScale;
            _baseOriginRotation = _characterBase.transform.rotation;
        }

        void OnEnable()
        {
            Idle();
        }

        void OnDisable()
        {
            DOTween.Kill(this);
        }

        void Update()
        {
#if UNITY_EDITOR
            if (gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    // Talk(TEST_dialogMessage).Play();
                }
                
                if (Input.GetKeyDown(KeyCode.S))
                {
                    StopTalk(false);
                }
            }
#endif
        }
    }
}
