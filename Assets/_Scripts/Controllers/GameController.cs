using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Patterns;
using Assets._Scripts.Visuals;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Controllers
{
    public class GameController : Singleton<GameController>
    {
        // TODO: Move to use IngameVisual
        [SerializeField] private MoveCountVisual _moveCount;
        [SerializeField] private ProgressBarVisual _progressBar;
        [SerializeField] private DifficultyTagVisual _difficultyTag;
        [SerializeField] private LevelIndexVisual _levelIndex;
        [SerializeField] private GameButtonVisual _settingButton;
        [SerializeField] private SettingPopupVisual _settingPopup;
        [SerializeField] private BoosterButtonVisual _extraMoveButton;
        [SerializeField] private BoosterButtonVisual _shuffleButton;
        [SerializeField] private BoosterButtonVisual _hintButton;

        //------------------------------
        [SerializeField] private LevelSO _testData;
        //------------------------------

        private LevelRuntimeData _currentLevelData;
        private List<PillarController> _pillars = new();

        public bool IsPaused { get; private set; }

        public void LoadLevel(LevelSO data)
        {
            _currentLevelData = new LevelRuntimeData(data);
            _moveCount.UpdateMoveCount(_currentLevelData.MoveCount);
            _progressBar.UpdateProgress(0, _currentLevelData.TotalGroups);
            _difficultyTag.SetDifficulty(_currentLevelData.Difficulty);
            _levelIndex.SetLevelIndex(_currentLevelData.Index);

            //TODO: Check if booster is unlocked
            _extraMoveButton.ChangeLockStatus(BoosterController.GetLockStatus(Enums.EBooster.ExtraMove));
            _shuffleButton.ChangeLockStatus(BoosterController.GetLockStatus(Enums.EBooster.Shuffle));
            _hintButton.ChangeLockStatus(BoosterController.GetLockStatus(Enums.EBooster.Hint));

            _extraMoveButton.SetCount(BoosterController.GetUseCount(Enums.EBooster.ExtraMove));
            _shuffleButton.SetCount(BoosterController.GetUseCount(Enums.EBooster.Shuffle));
            _hintButton.SetCount(BoosterController.GetUseCount(Enums.EBooster.Hint));

            _pillars = FindObjectsByType<PillarController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
            foreach (var pillar in _pillars)
            {
                pillar.OnFullMatched.AddListener(OnPillarFullMatched);
            }
        }

        public void PauseGame()
        {
            IsPaused = true;
        }

        public void ResumeGame()
        {
            IsPaused = false;
        }

        public void AddMoves(int amount)
        {
            _currentLevelData.ChangeMoveAmount(amount);
            _moveCount.UpdateMoveCount(_currentLevelData.MoveCount);
        }

        private void OnBlocksMoved()
        {
            _currentLevelData.DecreaseMove();
            _moveCount.UpdateMoveCount(_currentLevelData.MoveCount);

            //TODO: Check lose condition
        }

        private void OnPillarFullMatched()
        {
            _currentLevelData.IncreaseMatchedPillars();
            _progressBar.UpdateProgress(_currentLevelData.MatchedGroups, _currentLevelData.TotalGroups);

            //TODO: Check win condition
        }

        void Start()
        {
            InputController.Instance.OnBlocksMoved.AddListener(OnBlocksMoved);
            _settingButton.OnClicked.AddListener(() =>
            {
                _settingPopup.Show();
            });

            _extraMoveButton.OnClicked.AddListener(() => 
            {
                var useCount = BoosterController.GetUseCount(Enums.EBooster.ExtraMove);
                if (_extraMoveButton.IsLocked) _extraMoveButton.ShowPopupText();
                else if (useCount > 0) 
                {
                    BoosterController.UseBooster(Enums.EBooster.ExtraMove);
                    _extraMoveButton.SetCount(BoosterController.GetUseCount(Enums.EBooster.ExtraMove));
                }
                else Debug.Log("Extra Move is out of use"); //TODO: Show popup to buy more

            });
            _shuffleButton.OnClicked.AddListener(() =>
            {
                var useCount = BoosterController.GetUseCount(Enums.EBooster.Shuffle);
                if (_shuffleButton.IsLocked) _shuffleButton.ShowPopupText();
                else if (useCount > 0) 
                {
                    BoosterController.UseBooster(Enums.EBooster.Shuffle);
                    _shuffleButton.SetCount(BoosterController.GetUseCount(Enums.EBooster.Shuffle));
                }
                else Debug.Log("Shuffle is out of use"); //TODO: Show popup to buy more
            });
            _hintButton.OnClicked.AddListener(() =>
            {
                var useCount = BoosterController.GetUseCount(Enums.EBooster.Hint);
                if (_hintButton.IsLocked) _hintButton.ShowPopupText();
                else if (useCount > 0) 
                {
                    BoosterController.UseBooster(Enums.EBooster.Hint);
                    _hintButton.SetCount(BoosterController.GetUseCount(Enums.EBooster.Hint));
                }
                else Debug.Log("Hint is out of use"); //TODO: Show popup to buy more
            });


            //------------------------------
            LoadLevel(_testData);
        }

        void OnDestroy()
        {
            InputController.Instance.OnBlocksMoved.RemoveListener(OnBlocksMoved);
            _settingButton.OnClicked.RemoveAllListeners();
            _extraMoveButton.OnClicked.RemoveAllListeners();
            _shuffleButton.OnClicked.RemoveAllListeners();
            _hintButton.OnClicked.RemoveAllListeners();

            foreach (var pillar in _pillars)
            {
                pillar.OnFullMatched.RemoveListener(OnPillarFullMatched);
            }
        }
    }
}