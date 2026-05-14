using Assets._Scripts.Datas;
using Assets._Scripts.Editor;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
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
        [SerializeField] private Sprite _nextIcon;

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
            
            if (IsCleared()) 
                _icon.sprite = _clearedIcon;
            else if (_levelData.Index == UserManager.CurUser.CurrentLevelIndex && _levelData.Difficulty == EDifficulty.Normal) 
                _icon.sprite = _nextIcon;

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
                    ShowPopupText("Coming soon");
                    return;
                }
                
                var progress = UserManager.CurUser.CurrentLevelIndex;
                SetEnable(!_levelData.IsLocked);

                var playable = !_levelData.IsLocked;
#if UNITY_EDITOR
                playable = playable || DebugFlagToggle.Instance.AllowPlayLockedLevel;
#endif
                if (playable)
                {
                    if (UserManager.CurUser.HeartCount == 0)
                    {
                        PopupManager.Instance.ShowBundlePopup(EPopup.GetLife, BundleManager.Instance.GetLifeBundle());
                        return;
                    }

                    bool showBoosterSelector = PlayerProgressHelper.CheckUnlockBooster(EBooster.Hint, passMilestone: true);
#if UNITY_EDITOR
                    showBoosterSelector |= !DebugFlagToggle.Instance.SkipSelectBoosters;
#endif
                    if (showBoosterSelector)
                        StartCoroutine(PopupManager.Instance.ShowBoosterSelectPopup(_levelData));
                    else
                        GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                        {
                            GameManager.Instance.StartLevel(_levelData, boosters: new EBooster[] { EBooster.ExtraMove, EBooster.Shuffle, EBooster.Hint });
                        });
                }
                else
                    ShowPopupText("Locked!");
            });

            base.Awake();
        }

    }
}