using System.Collections;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using DG.Tweening;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class LevelFailedVisual : GamePopupVisual
    {
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
        [SerializeField] private FloatingBlockAnimation _leftBlock = new();
        [SerializeField] private FloatingBlockAnimation _rightBlock = new();

        public override IEnumerator Show()
        {
            SoundManager.Instance.PlayRandomSFX(ESfx.Lose);
            yield return base.Show();
            StartFloatingBlocks();
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
        }

        public override IEnumerator Hide()
        {
            StopFloatingBlocks();
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

        private void OnDisable()
        {
            StopFloatingBlocks();
        }
    }
}
