using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using UnityEditor;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        private StateMachine<EGameState> _gameSM = new();
        private LevelRuntimeData CurrentLevelData => LevelManager.PlayingLevel;

        private EGameState _lastState;

        public EGameState CurState => _gameSM.CurrentState.Key;

        //--------------------
        private List<PillarController> _pillars = new();
        //--------------------

        public void GoToMenu()
        {
            // GameSceneManager.Instance.ChangeScene(EGameScene.Menu, onLoad: () =>
            // {
            //     MainMenuVisualControl.Instance.ChangeTab(EMenuTab.Home);
            // });
                
            _gameSM.ChangeState(EGameState.Menu);
        }

        public void PauseGame()
        {
            if (_gameSM.CurrentState.Key != EGameState.Pause)
                _lastState = _gameSM.CurrentState.Key;
            _gameSM.ChangeState(EGameState.Pause);
        }

        public void ResumeGame()
        {
            // _gameSM.ChangeState(EGameState.Playing);
            _gameSM.ChangeState(_lastState);
        }

        public void ContinuePlaying()
        {
            _gameSM.ChangeState(EGameState.Playing);
        }

        public void FailedLevel()
        {
            Debug.Log("Level Failed");
            _gameSM.ChangeState(EGameState.Lose);
        }

        public void FinishedLevel()
        {
            Debug.Log("Level Finished");
            _gameSM.ChangeState(EGameState.Win);
        }

        public void ChangeMoveCount(int amount)
        {
            CurrentLevelData.ChangeMoveAmount(amount);
            IngameVisualController.Instance.UpdateMoveCount(CurrentLevelData.MoveCount);
        }

        private void OnBlocksMoved()
        {
            ChangeMoveCount(-1);

            //TODO: Events order is kinda bựa
            //TODO: Check win condition
            if (CurrentLevelData.MatchedGroups == CurrentLevelData.TotalGroups)
            {
                FinishedLevel();
                return;
            }

            //TODO: Check lose condition
            if (CurrentLevelData.MoveCount <= 0)
            {
                if (_gameSM.TryGetState(EGameState.Revive, out var reviveState) && !reviveState.IsFinished)
                {
                    _gameSM.ChangeState(EGameState.Revive);
                    Debug.Log("Waiting for reviving");
                }
                else
                {
                    FailedLevel();
                }
            }
        }

        private void OnPillarFullMatched(string tag)
        {
            CurrentLevelData.IncreaseMatchedPillars();
            IngameVisualController.Instance.UpdateProgressBar(CurrentLevelData.MatchedGroups, CurrentLevelData.TotalGroups);
        }

        public void StartLevel(LevelRuntimeData levelData)
        {
            if (levelData == null)
            {
                Debug.Log("No level data");
                return;
            }
            Debug.Log($"Start level {levelData.Index}");
            LevelManager.Instance.SetPlayingLevel(levelData);
            BoardController.Instance.InitBoard(CurrentLevelData);
            _pillars = BoardController.Instance.GetAllPillars();
            BoosterController.Instance.InitData();
            IngameVisualController.Instance.InitVisual(CurrentLevelData);
            
            _gameSM.ChangeState(EGameState.Playing);
        }

        protected override void Awake()
        {
            base.Awake();

            MenuState menuState = new(EGameState.Menu);
            PlayingState playingState = new(EGameState.Playing);
            PauseState pauseState = new(EGameState.Pause);
            ReviveState reviveState = new(EGameState.Revive);
            WinState winState = new(EGameState.Win);
            LoseState loseState = new(EGameState.Lose);

            _gameSM.AddStates(menuState, playingState, pauseState, reviveState, winState, loseState);
            _gameSM.SetDefaultState(EGameState.Menu);
            _gameSM.ChangeToDefault();
        }

        void Start()
        {

            // START GAME
            // StartLevel(LevelManager.Instance.GetCurrentLevel());
            //-----------
        }

        void Update()
        {
            _gameSM.DoState();
        }

        #region Menu State
        public class MenuState : AState<EGameState>
        {
            public MenuState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();
                
                GameSceneManager.Instance.ChangeScene(EGameScene.Menu, onLoad: () =>
                {
                    MainMenuVisualControl.Instance.ChangeTab(EMenuTab.Home);
                });
            }
        }
        #endregion

        #region Playing State
        public class PlayingState : AState<EGameState>
        {
            public PlayingState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();

                BlockMovementController.Instance.OnBlocksMoved.AddListener(Instance.OnBlocksMoved);
                foreach (var pillar in Instance._pillars)
                {
                    pillar.OnPillarClicked.AddListener(BlockMovementController.Instance.OnPillarClicked);
                    pillar.OnFullMatched.AddListener(Instance.OnPillarFullMatched);
                }
            }

            public override void Exit()
            {
                base.Exit();

                BlockMovementController.Instance.OnBlocksMoved.RemoveListener(Instance.OnBlocksMoved);
                foreach (var pillar in Instance._pillars)
                {
                    pillar.OnPillarClicked.RemoveListener(BlockMovementController.Instance.OnPillarClicked);
                    pillar.OnFullMatched.RemoveListener(Instance.OnPillarFullMatched);
                }
            }
        }
        #endregion

        #region Pause State
        public class PauseState : AState<EGameState>
        {
            public PauseState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();
                Time.timeScale = 0;
            }

            public override void Exit()
            {
                base.Exit();
                Time.timeScale = 1;
            }
        }
        #endregion

        #region Revive State
        public class ReviveState : AState<EGameState>
        {
            public ReviveState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();
                IngameVisualController.Instance.ShowRevivePopup();
            }
        }
        #endregion

        #region Win State
        public class WinState : AState<EGameState>
        {
            public WinState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();

                Instance.CurrentLevelData.FinishLevel();
                IngameVisualController.Instance.ShowFinishedPopup(!Instance.CurrentLevelData.IsCleared);
            }
        }
        #endregion

        #region Lose State
        public class LoseState : AState<EGameState>
        {
            public LoseState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();

                //TODO: Logic reduce heart if not clear before
                IngameVisualController.Instance.ShowFailedPopup();
            }
        }
        #endregion
    }
}