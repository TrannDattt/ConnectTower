using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TutorialCharacterVisual : MonoBehaviour
    {
        [SerializeField] private string TEST_dialogMessage = "Hello World!";

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
        [SerializeField] private float _dialogBoxMinRatio = 1.5f;
        [SerializeField] private float _dialogBoxMaxRatio = 4f;
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

        private Tween Hide()
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
            DOTween.Kill(this);
            var sequence = DOTween.Sequence().SetTarget(this).SetUpdate(true).SetRelative();
            sequence.Append(_characterBase.rectTransform.DOAnchorPos(_idleMoveOffset, _idleMoveDur).SetEase(_idleMoveCurve).SetLoops(-1, LoopType.Yoyo));
            sequence.Join(_characterBase.transform.DORotate(_idleRotateAngle, _idleRotateDur).SetEase(_idleRotateCurve).SetLoops(-1, LoopType.Restart));
            sequence.OnKill(resetBase);
            void resetBase()
            {
                _characterBase.rectTransform.anchoredPosition = Vector2.zero;
                _characterBase.transform.rotation = _baseOriginRotation;
            }
            sequence.Play();
        }

        public void Talk(string message)
        {
            // Setup
            _dialogText.text = "";
            _dialogText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _dialogText.verticalOverflow = VerticalWrapMode.Overflow;
            _dialogBox.gameObject.SetActive(true);
            ResizeDialogBoxForMessage(message);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_dialogBox.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_dialogText.rectTransform);

            DOTween.Kill(this);
            var masterSequence = DOTween.Sequence().SetTarget(this).SetUpdate(true);

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

            masterSequence.Append(dialogSequence).Join(baseSequence).Play();
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
            var effectiveLength = safeMessage.Length * (fontSize / 14f);
            var ratioByLength = Mathf.Lerp(_dialogBoxMaxRatio, _dialogBoxMinRatio, Mathf.InverseLerp(18f, 120f, effectiveLength));

            var estimatedTextArea = Mathf.Max(fontSize * fontSize * effectiveLength * 0.45f, fontSize * fontSize * 8f);
            var estimatedContentWidth = Mathf.Sqrt(estimatedTextArea * ratioByLength);
            var width = Mathf.Max((horizontalPadding * 2f) + estimatedContentWidth, fontSize * 6f);
            var height = Mathf.Max(width / ratioByLength, fontSize + (verticalPadding * 2f));

            var wrappedSettings = baseSettings;
            wrappedSettings.horizontalOverflow = HorizontalWrapMode.Wrap;
            wrappedSettings.verticalOverflow = VerticalWrapMode.Overflow;

            for (var i = 0; i < 3; i++)
            {
                var innerWidth = Mathf.Max(1f, width - (horizontalPadding * 2f));
                wrappedSettings.generationExtents = new Vector2(innerWidth, 0f);

                var preferredWrappedHeight = _dialogText.cachedTextGeneratorForLayout.GetPreferredHeight(safeMessage, wrappedSettings) / _dialogText.pixelsPerUnit;
                var contentHeight = preferredWrappedHeight + (verticalPadding * 2f);
                height = Mathf.Max(contentHeight, fontSize + (verticalPadding * 2f));

                var currentRatio = width / Mathf.Max(1f, height);
                if (currentRatio < _dialogBoxMinRatio)
                {
                    width = _dialogBoxMinRatio * height;
                }
                else if (currentRatio > _dialogBoxMaxRatio)
                {
                    width = _dialogBoxMaxRatio * height;
                }
            }

            // Final shrink pass so the box does not keep unnecessary vertical space.
            var finalInnerWidth = Mathf.Max(1f, width - (horizontalPadding * 2f));
            wrappedSettings.generationExtents = new Vector2(finalInnerWidth, 0f);
            var finalPreferredHeight = _dialogText.cachedTextGeneratorForLayout.GetPreferredHeight(safeMessage, wrappedSettings) / _dialogText.pixelsPerUnit;
            height = Mathf.Max(finalPreferredHeight + (verticalPadding * 2f), fontSize + (verticalPadding * 2f));

            var finalRatio = width / Mathf.Max(1f, height);
            if (finalRatio < _dialogBoxMinRatio)
            {
                width = _dialogBoxMinRatio * height;
            }
            else if (finalRatio > _dialogBoxMaxRatio)
            {
                width = _dialogBoxMaxRatio * height;
            }

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public void StopTalk(bool disableDialog)
        {
            _dialogBox.gameObject.SetActive(!disableDialog);
            DOTween.Kill(this);
            Idle();
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
            if (gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    Talk(TEST_dialogMessage);
                }
                
                if (Input.GetKeyDown(KeyCode.S))
                {
                    StopTalk(false);
                }
            }
        }
    }
}
