using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class LevelButtonVisual : GameButtonVisual
    {
        // First Level
        // [SerializeField] private Sprite _firstLevelIcon;
        // [SerializeField] private Sprite _firstLevelClearedIcon;

        // Normal Level
        [SerializeField] private Sprite _clearedIcon;
        [SerializeField] private Sprite _normalIcon;
        [SerializeField] private Sprite _hardIcon;
        [SerializeField] private Sprite _superHardIcon;

        [SerializeField] private GameObject _hardDecorate;
        [SerializeField] private GameObject _superHardDecorate;

        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _decorateHolder;
        [SerializeField] private Text _indexText;

        private LevelRuntimeData _levelData;

        public void UpdateVisual(LevelRuntimeData data)
        {
            // if (data.IsCleared) Debug.Log($"Update visual button level {data.Index}");
            _levelData = new(data);

            if (_levelData == null) return;
            _icon.sprite = _levelData.Difficulty switch
            {
                EDifficulty.Normal => _normalIcon,
                EDifficulty.Hard => _hardIcon,
                EDifficulty.SuperHard => _superHardIcon,
                _ => null
            };
            if (IsCleared()) _icon.sprite = _clearedIcon;

            _decorateHolder.SetActive(_levelData.Difficulty != EDifficulty.Normal && !IsCleared());
            _hardDecorate.SetActive(_levelData.Difficulty == EDifficulty.Hard);
            _superHardDecorate.SetActive(_levelData.Difficulty == EDifficulty.SuperHard);

            _indexText.text = _levelData.Index.ToString();
        }

        private bool IsCleared()
        {
            return _levelData != null && _levelData.IsCleared;
        }

        private bool IsFirstLevel()
        {
            return _levelData.Index == 0;
        }

        private bool IsLastLevel()
        {
            return _levelData.Index == LevelManager.Instance.GetTotalLevelCount() - 1;
        }

        protected override void Awake()
        {
            OnClicked.AddListener(() => 
            {
                var progress = UserManager.CurUser.CurrentLevelIndex;
                SetEnable(progress >= _levelData.Index);
                if (progress >= _levelData.Index)
                    GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                    {
                        Debug.Log($"Start level {_levelData.Index} with clear state: {_levelData.IsCleared}");
                        GameManager.Instance.StartLevel(_levelData);
                    });
                else
                    PopupManager.Instance.ShowPopupText("Locked", transform.position);
            });

            base.Awake();
        }
    }
}