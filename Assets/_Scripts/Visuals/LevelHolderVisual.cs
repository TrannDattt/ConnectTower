using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;

#if UNITY_EDITOR
using Assets._Scripts.Editor;
#endif

namespace Assets._Scripts.Visuals
{
    public class LevelHolderVisual : MonoBehaviour
    {
        [SerializeField] private Transform _levelContainer;
        [SerializeField] private LevelButtonVisual _levelButtonPrefabs;
        [SerializeField] private float _spacing = 20f;
        [SerializeField] private RectTransform _view;
        [SerializeField] private ParticleSystem _mysteryZone;
        [SerializeField] private AnimationCurve _scrollCurve;

        private float _buttonHeight;
        private Vector2 _detectRange;

        private List<LevelButtonVisual> _activeButtons = new();
        private Pooling<LevelButtonVisual> _buttonPool = new();
        private int _totalLevels;
        private int _pendingTargetIndex = -1;
        private bool _hasPendingScroll;
        private float TotalHeight => _totalLevels * _buttonHeight + Mathf.Max(0, _totalLevels - 1) * _spacing;

        private int _poolAmount = 10;
        private int _maxActiveAmount = 10;

        // TODO: Add behaviors to button: Auto focus, scale when scroll, button change color,...

        public void InitVisual(int targetIndex = -1, bool instant = false)
        {
            var allLevels = LevelManager.Instance.GetAllLevels();
            _totalLevels = GetMaxLevelCount(allLevels.Count);

            var containerRt = _levelContainer as RectTransform;
            if (containerRt == null) return;

            _pendingTargetIndex = targetIndex;
            _hasPendingScroll = true;

            if (gameObject.activeInHierarchy)
            {
                float targetY = GetTargetScrollY(targetIndex);
                ScrollToPosition(targetY, instant);
                RebuildButtonsAroundScrollY(targetY);
                _hasPendingScroll = false;
                return;
            }

            if (_activeButtons.Count == 0)
            {
                RebuildButtonsAroundScrollY(containerRt.anchoredPosition.y);
            }
        }

        private int GetMaxLevelCount(int totalCount)
        {
            int maxLevels;
#if UNITY_EDITOR
            if (DebugFlagToggle.Instance.ShowAllLevel)
                return totalCount;
#endif
            maxLevels = UserManager.CurUser.CurrentLevelIndex + 3;
            if (UserManager.CurUser.CurrentLevelIndex < totalCount)
            {
                maxLevels = Mathf.Min(maxLevels, totalCount);
            }
            else
            {
                maxLevels = totalCount + 2; // Add 2 placeholders if current level is the last one.
            }

            return maxLevels;
        }

        private void RebuildButtonsAroundScrollY(float scrollY)
        {
            if (_activeButtons.Count > 0)
            {
                for (int i = _activeButtons.Count - 1; i >= 0; i--)
                {
                    _buttonPool.ReturnItem(_activeButtons[i]);
                }

                _activeButtons.Clear();
            }

            float visibleBottom = -scrollY;
            int firstVisibleIndex = Mathf.Max(1, Mathf.FloorToInt(visibleBottom / (_buttonHeight + _spacing)) + 1);
            int startIndex = Mathf.Max(1, firstVisibleIndex - 3);
            int maxStartIndex = Mathf.Max(1, _totalLevels - _maxActiveAmount + 1);
            startIndex = Mathf.Min(startIndex, maxStartIndex);

            for (int i = 0; i < _maxActiveAmount; i++)
            {
                int currIndex = startIndex + i;
                if (currIndex > _totalLevels) break;

                var levelData = LevelManager.Instance.GetLevel(currIndex);
                var newButton = _buttonPool.GetItem();
                newButton.UpdateVisual(levelData, currIndex);
                SetButtonPosition(newButton, currIndex);
                newButton.transform.SetSiblingIndex(i + 1);
                _activeButtons.Add(newButton);
            }
        }

        private void SetButtonPosition(LevelButtonVisual button, int levelIndex)
        {
            var rt = button.GetComponent<RectTransform>();

            float yPos = (levelIndex - 1) * (_buttonHeight + _spacing);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, yPos);
        }

        private bool CheckSensorInRange(Transform sensor)
        {
            Vector3 localPos = _view.InverseTransformPoint(sensor.position);

            float viewHalfHeight = _view.rect.height * 0.5f;
            float distance = Mathf.Abs(localPos.y);
            return distance < (viewHalfHeight + _buttonHeight * 2f);
        }

        private bool CheckSensorOutRange(Transform sensor)
        {
            Vector3 localPos = _view.InverseTransformPoint(sensor.position);

            float viewHalfHeight = _view.rect.height * 0.5f;
            float distance = Mathf.Abs(localPos.y);
            return distance > (viewHalfHeight + _buttonHeight * 3f);
        }

        private void SyncButtonsToCurrentViewportIfNeeded()
        {
            var containerRt = _levelContainer as RectTransform;
            if (containerRt == null) return;

            if (_activeButtons.Count == 0)
            {
                RebuildButtonsAroundScrollY(containerRt.anchoredPosition.y);
                return;
            }

            if (_activeButtons.All(button => CheckSensorOutRange(button.transform)))
            {
                RebuildButtonsAroundScrollY(containerRt.anchoredPosition.y);
            }
        }

        private void CheckAndUpdateVisual()
        {
            SyncButtonsToCurrentViewportIfNeeded();
            if (_activeButtons.Count == 0) return;

            if (CheckSensorInRange(_activeButtons[^1].transform))
            {
                int nextIndex = _activeButtons[^1].LevelIndex + 1;
                int maxLevels = GetMaxLevelCount(LevelManager.Instance.GetTotalLevelCount());

                if (nextIndex <= maxLevels)
                {
                    var nextLevelData = LevelManager.Instance.GetLevel(nextIndex);
                    var toAdd = _buttonPool.GetItem();
                    toAdd.UpdateVisual(nextLevelData, nextIndex);
                    SetButtonPosition(toAdd, nextIndex);
                    _activeButtons.Add(toAdd);
                    toAdd.transform.SetAsLastSibling();
                }
            }
            else if (CheckSensorInRange(_activeButtons[0].transform))
            {
                int prevIndex = _activeButtons[0].LevelIndex - 1;

                if (prevIndex >= 1)
                {
                    var prevLevelData = LevelManager.Instance.GetLevel(prevIndex);
                    var toAdd = _buttonPool.GetItem();
                    toAdd.UpdateVisual(prevLevelData, prevIndex);
                    SetButtonPosition(toAdd, prevIndex);
                    _activeButtons.Insert(0, toAdd);
                    toAdd.transform.SetAsFirstSibling();
                }
            }

            if (_activeButtons.Count > _maxActiveAmount && CheckSensorOutRange(_activeButtons[^1].transform))
            {
                var toRemove = _activeButtons[^1];
                _activeButtons.Remove(toRemove);
                _buttonPool.ReturnItem(toRemove);
            }
            else if (_activeButtons.Count > _maxActiveAmount && CheckSensorOutRange(_activeButtons[0].transform))
            {
                var toRemove = _activeButtons[0];
                _activeButtons.Remove(toRemove);
                _buttonPool.ReturnItem(toRemove);
            }
        }

        void Awake()
        {
            _buttonPool = new(_levelButtonPrefabs, _poolAmount, _levelContainer);
            _buttonHeight = _levelButtonPrefabs.GetComponent<RectTransform>().sizeDelta.y;
            _detectRange = new(_buttonHeight * 3, _buttonHeight * 3.5f);

            if (_mysteryZone != null)
                _mysteryZone.gameObject.SetActive(false);
        }

        private void UpdateMysteryZone()
        {
            if (_mysteryZone == null || _activeButtons.Count == 0) return;

            bool shouldEnable = false;
            var topButton = _activeButtons.FirstOrDefault(b => b.LevelIndex == _totalLevels);
            if (topButton != null)
            {
                Vector3 localPos = _view.InverseTransformPoint(topButton.transform.position);
                float viewTopY = _view.rect.yMax;

                if (localPos.y <= viewTopY - _buttonHeight * .8f &&
                    localPos.y >= viewTopY - _buttonHeight * 1.7f)
                {
                    shouldEnable = true;
                }
            }

            if (_mysteryZone.gameObject.activeSelf != shouldEnable)
                _mysteryZone.gameObject.SetActive(shouldEnable);
        }

        private int GetTargetLevelIndex(int specificIndex = -1)
        {
            if (specificIndex > 0) return specificIndex;

            var currentLevel = LevelManager.Instance.GetLatestNotClearedLevel();
            return currentLevel != null ? currentLevel.Index : _totalLevels;
        }

        private float GetTargetScrollY(int specificIndex = -1)
        {
            var containerRt = _levelContainer as RectTransform;
            if (containerRt == null) return 0f;

            containerRt.sizeDelta = new Vector2(containerRt.sizeDelta.x, TotalHeight);

            int targetIndex = GetTargetLevelIndex(specificIndex);

            float targetY;
            if (targetIndex <= 1)
            {
                targetY = 0f;
            }
            else if (targetIndex >= _totalLevels)
            {
                targetY = -(TotalHeight - _view.rect.height);
                if (targetY > 0f) targetY = 0f;
            }
            else
            {
                targetY = -(targetIndex - 1) * (_buttonHeight + _spacing);
                targetY += _view.rect.height * 0.5f;
                targetY -= _buttonHeight;

                if (targetY > 0f) targetY = 0f;
                float minTargetY = -Mathf.Max(0f, TotalHeight - _view.rect.height);
                if (targetY < minTargetY) targetY = minTargetY;
            }

            return targetY;
        }

        private void ScrollToPosition(float targetY, bool instant = false)
        {
            var containerRt = _levelContainer as RectTransform;
            if (containerRt == null) return;

            containerRt.DOKill();
            if (instant)
            {
                containerRt.anchoredPosition = new Vector2(containerRt.anchoredPosition.x, targetY);
            }
            else
            {
                containerRt.DOAnchorPosY(targetY, 0.75f).SetEase(_scrollCurve).SetUpdate(true).SetLink(gameObject);
            }
        }

        private void ScrollToCurrentLevel(int specificIndex = -1, bool instant = false)
        {
            ScrollToPosition(GetTargetScrollY(specificIndex), instant);
        }

        void OnEnable()
        {
            if (_hasPendingScroll)
            {
                ScrollToCurrentLevel(_pendingTargetIndex);
                _hasPendingScroll = false;
                return;
            }

            ScrollToCurrentLevel();
        }

        void Update()
        {
            CheckAndUpdateVisual();
            UpdateMysteryZone();
        }
    }
}
