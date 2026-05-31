using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class LevelFailedVisual : GamePopupVisual
    {
        [System.Serializable]
        private sealed class ButtonIdleFloatAnimation
        {
            [SerializeField] private float _moveOffsetY = 16f;
            [SerializeField] private float _moveDuration = 1.35f;
            [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [SerializeField] private float _startDelayStep = 0.12f;

            private readonly List<Tween> _moveTweens = new();
            private readonly Dictionary<RectTransform, Vector2> _initialPositions = new();

            public void CacheTargets(IEnumerable<RectTransform> targets)
            {
                _initialPositions.Clear();
                if (targets == null)
                    return;

                foreach (var target in targets)
                {
                    if (target == null)
                        continue;

                    _initialPositions[target] = target.anchoredPosition;
                }
            }

            public void Play(IEnumerable<RectTransform> targets, GameObject owner)
            {
                Stop();
                if (targets == null)
                    return;

                var index = 0;
                foreach (var target in targets)
                {
                    if (target == null || !target.gameObject.activeInHierarchy)
                        continue;

                    if (!_initialPositions.TryGetValue(target, out var initialPosition))
                    {
                        initialPosition = target.anchoredPosition;
                        _initialPositions[target] = initialPosition;
                    }

                    target.anchoredPosition = initialPosition;
                    var tween = target.DOAnchorPosY(initialPosition.y + _moveOffsetY, _moveDuration)
                        .SetEase(_moveCurve)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetDelay(index * _startDelayStep)
                        .SetUpdate(true)
                        .SetLink(owner, LinkBehaviour.KillOnDisable);
                    _moveTweens.Add(tween);
                    index++;
                }
            }

            public void Stop()
            {
                foreach (var tween in _moveTweens)
                    tween?.Kill();

                _moveTweens.Clear();

                foreach (var pair in _initialPositions)
                {
                    if (pair.Key == null)
                        continue;

                    pair.Key.anchoredPosition = pair.Value;
                }
            }
        }

        [System.Serializable]
        private sealed class FloatingBlockAnimation
        {
            [SerializeField] private RectTransform _target;
            [SerializeField] private Vector2 _moveOffset = new(0f, 18f);
            [SerializeField] private float _moveDuration = 1.9f;
            [SerializeField] private AnimationCurve _moveEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [SerializeField] private float _rotateOffset = 6f;
            [SerializeField] private float _rotateDuration = 2.1f;
            [SerializeField] private AnimationCurve _rotateEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            [SerializeField] private float _startDelay;

            private Tween _moveTween;
            private Tween _rotateTween;
            private Vector2 _initialAnchoredPosition;
            private float _initialRotationZ;
            private bool _hasCachedState;

            public RectTransform Target
            {
                get => _target;
                set => _target = value;
            }

            public void CacheState()
            {
                if (_target == null)
                    return;

                _initialAnchoredPosition = _target.anchoredPosition;
                _initialRotationZ = _target.localEulerAngles.z;
                _hasCachedState = true;
            }

            public void Play(GameObject owner)
            {
                if (_target == null)
                    return;

                CacheState();
                Stop();
                RestoreInitialState();

                _moveTween = _target.DOAnchorPos(_initialAnchoredPosition + _moveOffset, _moveDuration)
                    .SetEase(_moveEase)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(_startDelay)
                    .SetUpdate(true)
                    .SetLink(owner, LinkBehaviour.KillOnDisable);

                _rotateTween = _target.DOLocalRotate(new Vector3(0f, 0f, _initialRotationZ + _rotateOffset), _rotateDuration)
                    .SetEase(_rotateEase)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetDelay(_startDelay)
                    .SetUpdate(true)
                    .SetLink(owner, LinkBehaviour.KillOnDisable);
            }

            public void Stop()
            {
                _moveTween?.Kill();
                _rotateTween?.Kill();
                RestoreInitialState();
            }

            private void RestoreInitialState()
            {
                if (_target == null || !_hasCachedState)
                    return;

                _target.anchoredPosition = _initialAnchoredPosition;
                var localEulerAngles = _target.localEulerAngles;
                localEulerAngles.z = _initialRotationZ;
                _target.localEulerAngles = localEulerAngles;
            }
        }

        [SerializeField] private GameButtonVisual _retryButton;
        [SerializeField] private GameButtonVisual _homeButton;
        [SerializeField] private RectTransform _buttonsRoot;
        [SerializeField] private ButtonIdleFloatAnimation _buttonIdleFloat = new();
        [SerializeField] private FloatingBlockAnimation _leftBlock = new();
        [SerializeField] private FloatingBlockAnimation _rightBlock = new();

        private readonly List<GameButtonVisual> _idleButtons = new();
        private readonly List<RectTransform> _buttonIdleTargets = new();

        public override IEnumerator Show()
        {
            SoundManager.Instance.PlayRandomSFX(ESfx.Lose);
            yield return base.Show();
            StartFloatingBlocks();
            StartIdleButtonEffects();
        }

        protected override void Start()
        {
            _retryButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Retry level");
                StartCoroutine(Hide());
                GameManager.Instance.RestartLevel();
            });
            _homeButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Go to main menu");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu();
            });

            base.Start();
            _leftBlock.CacheState();
            _rightBlock.CacheState();
            CacheIdleButtonTargets();
        }

        public override IEnumerator Hide()
        {
            StopFloatingBlocks();
            StopIdleButtonEffects();
            yield return base.Hide();
        }

        private void StartFloatingBlocks()
        {
            StopFloatingBlocks();
            _leftBlock.Play(gameObject);
            _rightBlock.Play(gameObject);
        }

        private void StopFloatingBlocks()
        {
            _leftBlock.Stop();
            _rightBlock.Stop();
        }

        private void CacheIdleButtonTargets()
        {
            _idleButtons.Clear();
            _buttonIdleTargets.Clear();
            if (_buttonsRoot == null)
            {
                var buttonsTransform = transform.Find("Buttons") ?? transform.Find("Button");
                if (buttonsTransform != null)
                    _buttonsRoot = buttonsTransform as RectTransform;
            }

            if (_buttonsRoot == null)
                return;

            _buttonsRoot.GetComponentsInChildren(true, _idleButtons);
            foreach (var button in _idleButtons)
            {
                if (button == null || button.ButtonRt == null)
                    continue;

                _buttonIdleTargets.Add(button.ButtonRt);
            }

            _buttonIdleFloat.CacheTargets(_buttonIdleTargets);
        }

        private void StartIdleButtonEffects()
        {
            StopIdleButtonEffects();
            _buttonIdleFloat.Play(_buttonIdleTargets, gameObject);
        }

        private void StopIdleButtonEffects()
        {
            _buttonIdleFloat.Stop();
        }

        private void OnDisable()
        {
            StopFloatingBlocks();
            StopIdleButtonEffects();
        }
    }
}
