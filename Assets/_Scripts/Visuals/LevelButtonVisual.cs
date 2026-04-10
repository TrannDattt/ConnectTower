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
        private int _placeholderIndex = -1;

        public int LevelIndex => _levelData == null ? _placeholderIndex : _levelData.Index;

        public void UpdateVisual(LevelRuntimeData data, int backupIndex = -1)
        {
            _placeholderIndex = backupIndex;

            if (data == null)
            {
                _levelData = null;
                _indexText.text = "Coming soon";
                _icon.sprite = _normalIcon;
                _decorateHolder.SetActive(false);
                gameObject.name = $"Level_{backupIndex}_PlaceHolder";
                return;
            }

            _levelData = new(data);
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
            gameObject.name = $"Level_{_levelData.Index}";
        }

        private bool IsCleared()
        {
            return _levelData != null && _levelData.IsCleared;
        }

        protected override void Awake()
        {
            OnClicked.AddListener(() => 
            {
                if (_levelData == null) 
                {
                    PopupManager.Instance.ShowPopupText("Coming soon", GetCenterPosition());
                    return;
                }
                
                var progress = UserManager.CurUser.CurrentLevelIndex;
                SetEnable(!_levelData.IsLocked);
                if (!_levelData.IsLocked || GameManager.Instance.AllowPlayLockedLevel)
                    GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                    {
                        Debug.Log($"Start level {_levelData.Index} with clear state: {_levelData.IsCleared}");
                        GameManager.Instance.StartLevel(_levelData);
                    });
                else
                    PopupManager.Instance.ShowPopupText("Locked", GetCenterPosition());
            });

            base.Awake();
        }

        private Vector3 GetCenterPosition()
        {
            return transform is RectTransform rt ? rt.TransformPoint(rt.rect.center) : transform.position;
        }
    }
}