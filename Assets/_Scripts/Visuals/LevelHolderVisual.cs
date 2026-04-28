using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;

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
#if UNITY_EDITOR
        [SerializeField] private bool _showAllLevel = true;
#endif
        private float _buttonHeight;
        private Vector2 _detectRange;

        private List<LevelButtonVisual> _activeButtons = new();
        private Pooling<LevelButtonVisual> _buttonPool = new();
        private int _totalLevels;
        private float TotalHeight => _totalLevels * _buttonHeight + Mathf.Max(0, _totalLevels - 1) * _spacing;

        private int _poolAmount = 10;
        private int _maxActiveAmount = 10;
        
        //TODO: Add behaviors to button: Auto focus, scale when scroll, button change color,...

        public void InitVisual(int targetIndex = -1, bool instant = false)
        {
            if (_activeButtons.Count > 0)
            {
                for (int i = _activeButtons.Count - 1; i >= 0; i--)
                {
                    _buttonPool.ReturnItem(_activeButtons[i]);
                }
                _activeButtons.Clear();
            }

            var allLevels = LevelManager.Instance.GetAllLevels();
            var clearedLevel = allLevels.Where(l => l.Index < UserManager.CurUser.CurrentLevelIndex);
            int totalLevels;
#if UNITY_EDITOR
            if (_showAllLevel)
                totalLevels = allLevels.Count;
            else
#endif
                totalLevels = clearedLevel.Count() + 4;

            if (UserManager.CurUser.CurrentLevelIndex < allLevels.Count)
            {
                totalLevels = Mathf.Min(totalLevels, allLevels.Count);
            }
            else
            {
                totalLevels = allLevels.Count + 2; // thêm 2 placeholders nếu là level cuối
            }
            _totalLevels = totalLevels;

            // Đảm bảo container được scroll tới đúng vị trí trước khi spawn buttons
            ScrollToCurrentLevel(targetIndex, instant);
            
            var containerRt = _levelContainer as RectTransform;
            float currentY = containerRt.anchoredPosition.y;
            
            // Tính index đầu tiên hiển thị dựa trên vị trí container thực tế
            float visibleBottom = -currentY; 
            int firstVisibleIndex = Mathf.Max(1, Mathf.FloorToInt(visibleBottom / (_buttonHeight + _spacing)) + 1);

            // Spawn nhiều hơn để bao phủ toàn bộ vùng nhìn thấy + buffer 2 bên
            // Tăng startIndex lùi về sau nhiều hơn để đảm bảo không bị trống phía dưới
            int startIndex = Mathf.Max(1, firstVisibleIndex - 3);

            for(int i = 0; i < _maxActiveAmount; i++)
            {
                int currIndex = startIndex + i;
                if (currIndex > _totalLevels) break;
                if (currIndex < 1) continue;

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
            
            // Container pinned to bottom: Y goes upwards standardly. Level 1 starts near Y=0.
            float yPos = (levelIndex - 1) * (_buttonHeight + _spacing);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, yPos);
        }

        private bool CheckSensorInRange(Transform sensor)
        {
            // Chuyển vị trí từ World Space về Local Space của _view để tính toán chính xác
            // bất kể Canvas đang ở chế độ Overlay hay Camera
            Vector3 localPos = _view.InverseTransformPoint(sensor.position);
            
            float viewHalfHeight = _view.rect.height * 0.5f;
            float distance = Mathf.Abs(localPos.y); // localPos.y là khoảng cách tới tâm của _view
            return distance < (viewHalfHeight + _buttonHeight * 2f);
        }

        private bool CheckSensorOutRange(Transform sensor)
        {
            Vector3 localPos = _view.InverseTransformPoint(sensor.position);
            
            float viewHalfHeight = _view.rect.height * 0.5f;
            float distance = Mathf.Abs(localPos.y);
            return distance > (viewHalfHeight + _buttonHeight * 3f);
        }

        private void CheckAndUpdateVisual()
        {
            if (_activeButtons.Count == 0) return;

            // Arrays are ordered by LevelIndex ascending (e.g. 1, 2... 7)
            // But because of our inversion:
            // _activeButtons[0] (Index 1) is physically DOWN (Bottom).
            // _activeButtons[^1] (Index 7) is physically UP (Top).

            // 1. ADD ABOVE (Physically HIGHER, visually going towards Top)
            if (CheckSensorInRange(_activeButtons[^1].transform))
            {
                int nextIndex = _activeButtons[^1].LevelIndex + 1;
                int totalCount = LevelManager.Instance.GetTotalLevelCount();
                
                int maxLevels;
#if UNITY_EDITOR
                if (_showAllLevel)
                    maxLevels = totalCount;
                else
#endif
                    maxLevels = UserManager.CurUser.CurrentLevelIndex + 3;
                if (UserManager.CurUser.CurrentLevelIndex < totalCount)
                {
                    maxLevels = Mathf.Min(maxLevels, totalCount);
                }
                else
                {
                    maxLevels = totalCount + 2; // thêm 2 placeholders nếu là level cuối
                }

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
            // 2. ADD BELOW (Physically LOWER, visually going towards Bottom)
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

            // 3. REMOVE ABOVE (Physically HIGHER)
            if (_activeButtons.Count > _maxActiveAmount && CheckSensorOutRange(_activeButtons[^1].transform))
            {
                var toRemove = _activeButtons[^1];
                _activeButtons.Remove(toRemove);
                _buttonPool.ReturnItem(toRemove);
            }
            // 4. REMOVE BELOW (Physically LOWER)
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
                
                // Enable Mystery Zone only when the absolute top level is visibly near the top edge.
                // It shouldn't be way above the screen (e.g. user scrolled to bottom levels).
                // It shouldn't be pulled way down (e.g. user elastic-scrolled it to the middle).
                if (localPos.y <= viewTopY - _buttonHeight * .8f && 
                    localPos.y >= viewTopY - _buttonHeight * 1.7f)
                {
                    shouldEnable = true;
                }
            }

            if (_mysteryZone.gameObject.activeSelf != shouldEnable)
                _mysteryZone.gameObject.SetActive(shouldEnable);
        }

        private void ScrollToCurrentLevel(int specificIndex = -1, bool instant = false)
        {
            var containerRt = _levelContainer as RectTransform;
            if (containerRt == null) return;

            // Đảm bảo sizeDelta của container đã được cập nhật trước khi tính toán scroll
            containerRt.sizeDelta = new Vector2(containerRt.sizeDelta.x, TotalHeight);

            int targetIndex = specificIndex;
            if (targetIndex <= 0)
            {
                var currentLevel = LevelManager.Instance.GetLatestNotClearedLevel();
                targetIndex = currentLevel != null ? currentLevel.Index : _totalLevels;
            }

            float targetY;
            if (targetIndex <= 1)
            {
                targetY = 0;
            }
            else if (targetIndex >= _totalLevels)
            {
                targetY = -(TotalHeight - _view.rect.height);
                if (targetY > 0) targetY = 0;
            }
            else
            {
                targetY = -(targetIndex - 1) * (_buttonHeight + _spacing);
                targetY += _view.rect.height * 0.5f;
                targetY -= _buttonHeight;

                if (targetY > 0) targetY = 0;
                float minTargetY = -Mathf.Max(0, TotalHeight - _view.rect.height);
                if (targetY < minTargetY) targetY = minTargetY;
            }

            containerRt.DOKill();
            if (instant)
            {
                containerRt.anchoredPosition = new Vector2(containerRt.anchoredPosition.x, targetY);
            }
            else
            {
                containerRt.DOAnchorPosY(targetY, 0.75f).SetEase(_scrollCurve).SetUpdate(true).SetLink(gameObject);
                // containerRt.DOAnchorPosY(targetY, 0.75f).SetEase(Ease.OutSine).SetUpdate(true).SetLink(gameObject);
            }
        }

        void OnEnable()
        {
            ScrollToCurrentLevel();
        }

        void Update()
        {
            CheckAndUpdateVisual();
            UpdateMysteryZone();
        }
    }
}