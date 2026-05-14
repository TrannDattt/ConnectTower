using Assets._Scripts.Controllers;
using Assets._Scripts.Editor;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class BoosterSelectButton : ToggleButtonVisual
    {
        [field: SerializeField] public EBooster Key {get; private set;}
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _lockImage;
        [SerializeField] private Image _lockBackground;
        [SerializeField] private GameObject _useCountHolder;
        [SerializeField] private Text _useCount;
        [SerializeField] private Image _getMoreImage;

        [SerializeField] private float _disableAlpha = .5f;
        [SerializeField] private float _fadeDur;

        [SerializeField] private Vector2 _floatOffset;
        [SerializeField] private float _floatCycleDur;
        [SerializeField] private AnimationCurve _floatMoveCurve;

        private Vector2 _initPos;

        public void Selected()
        {
            if (!IsEnabled) return;

            void reset()
            {
                _buttonRt.anchoredPosition = _initPos;
            }
            var sequence = DOTween.Sequence().SetTarget(this).SetId("Select").SetUpdate(true);
            sequence.Append(_buttonRt.DOAnchorPos(_initPos + _floatOffset, _floatCycleDur).SetEase(_floatMoveCurve).SetLoops(int.MaxValue, LoopType.Yoyo));
            sequence.Join(_canvasGroup.DOFade(1f, _fadeDur).SetEase(Ease.OutQuad));
            sequence.OnComplete(reset).OnKill(reset);
            sequence.Play();
            Debug.Log($"Add booster {Key}");
        }

        public void Deselected()
        {
            if (!IsEnabled) return;

            DOTween.Kill(this, "Select", true);
            _canvasGroup.DOFade(_disableAlpha, _fadeDur).SetEase(Ease.OutQuad).SetTarget(this).SetId("Deselect").SetUpdate(true);
            Debug.Log($"Remove booster {Key}");
        }

        public void Enable()
        {
            _lockImage.gameObject.SetActive(false);
            _lockBackground.gameObject.SetActive(false);
            _useCountHolder.SetActive(true);
            SetEnable(true);
        }

        public void Disable()
        {
            _lockImage.gameObject.SetActive(true);
            _lockBackground.gameObject.SetActive(true);
            _useCountHolder.SetActive(false);
            SetEnable(false);
        }

        private void Init()
        {
            var useCount = BoosterController.Instance.GetUseCount(Key);
            _getMoreImage.gameObject.SetActive(useCount <= 0);
            _useCount.text = useCount.ToString();
            _canvasGroup.alpha = _disableAlpha;
        }

        protected override void Awake()
        {
            base.Awake();

            OnToggled.AddListener((active) =>
            {
                if (active) Selected();
                else Deselected();
            });
        }

        void OnEnable()
        {
            if (PlayerProgressHelper.CheckUnlockBooster(Key, passMilestone: true) || DebugFlagToggle.Instance.IgnoreMilestone)
                Enable();
            else
                Disable();

            Init();
        }

        void OnDisable()
        {
            _canvasGroup.alpha = _disableAlpha;

            DOTween.Kill(this, "Select");
            DOTween.Kill(this, "Deselect", true);
        }

        protected override void Start()
        {
            base.Start();
            _initPos = _buttonRt.anchoredPosition;
        }
    }
}