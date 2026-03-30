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
        [SerializeField] private Sprite _clearedIcon;
        [SerializeField] private Sprite _normalIcon;
        [SerializeField] private Sprite _hardIcon;
        [SerializeField] private Sprite _superHardIcon;

        [SerializeField] private GameObject _hardDecorate;
        [SerializeField] private GameObject _superHardDecorate;

        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _decorateHolder;
        [SerializeField] private TextMeshProUGUI _indexText;

        private LevelRuntimeData _levelData;

        public void InitVisual(LevelRuntimeData data)
        {
            _levelData = data;

            if (_levelData == null) return;
            _icon.sprite = _levelData.Difficulty switch
            {
                EDifficulty.Normal => _normalIcon,
                EDifficulty.Hard => _hardIcon,
                EDifficulty.SuperHard => _superHardIcon,
                _ => null
            };
            if (_levelData.IsCleared) _icon.sprite = _clearedIcon;

            _decorateHolder.SetActive(_levelData.Difficulty != EDifficulty.Normal && !_levelData.IsCleared);
            _hardDecorate.SetActive(_levelData.Difficulty == EDifficulty.Hard);
            _superHardDecorate.SetActive(_levelData.Difficulty == EDifficulty.SuperHard);

            _indexText.text = _levelData.Index.ToString();
        }

        protected override void Awake()
        {
            base.Awake();

            OnClicked.AddListener(() => 
            {
                GameSceneManager.Instance.ChangeScene(EGameScene.Ingame, onLoad: () =>
                {
                    GameManager.Instance.StartLevel(_levelData);
                });
            });
        }
    }
}