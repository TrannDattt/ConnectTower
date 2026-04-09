using System.Collections.Generic;
using System.Windows.Forms;
using Assets._Scripts.Datas;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class LevelHolderVisual : MonoBehaviour
    {
        [SerializeField] private Transform _levelContainer;
        [SerializeField] private LevelButtonVisual _levelButtonPrefabs;
        [SerializeField] private float _spacing = 20f;
        [SerializeField] private RectTransform _view;
        private float _buttonHeight;
        private Vector2 _detectRange;

        private List<LevelButtonVisual> _activeButtons = new();
        private Pooling<LevelButtonVisual> _buttonPool = new();
        private int _poolAmount = 10;
        private int _maxActiveAmount = 7;
        
        //TODO: Add behaviors to button: Auto focus, scale when scroll, button change color,...

        public void InitVisual(int startIndex = 1)
        {
            if (_activeButtons.Count > 0) return;

            int totalLevels = LevelManager.Instance.GetTotalLevelCount();
            float totalHeight = totalLevels * _buttonHeight + Mathf.Max(0, totalLevels - 1) * _spacing;
            var containerRt = _levelContainer.GetComponent<RectTransform>();
            containerRt.sizeDelta = new Vector2(containerRt.sizeDelta.x, totalHeight);

            // Starting viewport position target (Bottom anchored container Y maps down to 0)
            float targetY = -(startIndex - 1) * (_buttonHeight + _spacing);
            
            // Adjust to rough center using viewport 
            targetY += _view.rect.height * 0.5f; 
            if (targetY > 0) targetY = 0; // prevent overscroll at the very bottom
            
            containerRt.anchoredPosition = new Vector2(
                containerRt.anchoredPosition.x, 
                targetY
            );

            for(int i = 0; i < _maxActiveAmount; i++)
            {
                var levelData = LevelManager.Instance.GetLevel(startIndex + i);
                if (levelData == null) break;

                var newButton = _buttonPool.GetItem();
                newButton.UpdateVisual(levelData);
                SetButtonPosition(newButton, levelData.Index);
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
            return Mathf.Abs(sensor.position.y - _view.position.y) < _detectRange.x;
        }

        private bool CheckSensorOutRange(Transform sensor)
        {
            return Mathf.Abs(sensor.position.y - _view.position.y) > _detectRange.y;
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
                var nextLevelData = LevelManager.Instance.GetLevel(_activeButtons[^1].LevelIndex + 1);
                if (nextLevelData != null)
                {
                    var toAdd = _buttonPool.GetItem();
                    toAdd.UpdateVisual(nextLevelData);
                    SetButtonPosition(toAdd, nextLevelData.Index);
                    _activeButtons.Add(toAdd);
                    toAdd.transform.SetAsLastSibling();
                }
            }
            // 2. ADD BELOW (Physically LOWER, visually going towards Bottom)
            else if (CheckSensorInRange(_activeButtons[0].transform))
            {
                var nextLevelData = LevelManager.Instance.GetLevel(_activeButtons[0].LevelIndex - 1);
                if (nextLevelData != null)
                {
                    var toAdd = _buttonPool.GetItem();
                    toAdd.UpdateVisual(nextLevelData);
                    SetButtonPosition(toAdd, nextLevelData.Index);
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
            _detectRange = new(_buttonHeight * 2, _buttonHeight * 5);
        }

        void Update()
        {
            CheckAndUpdateVisual();
        }
    }
}