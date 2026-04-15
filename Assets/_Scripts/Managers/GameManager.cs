using System.Collections;
using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Managers
{
    public class GameManager : Singleton<GameManager>
    {
#if UNITY_EDITOR
        [field: SerializeField] public bool AllowPlayLockedLevel {get; private set;}
        public bool IsPlayTest {get; private set;} = false;
#endif

        private StateMachine<EGameState> _gameSM = new();
        private LevelRuntimeData CurrentLevelData => LevelManager.PlayingLevel;

        private EGameState _lastState;

        public EGameState CurState => _gameSM.CurrentState.Key;
        private List<PillarController> _pillars = new();
        private UnityEvent<int> _onStartNewLevel = new();
        private UnityAction _onGoToMenuCallback;

        public void GoToMenu(UnityAction onLoaded = null)
        {
            if (CurState != EGameState.None) BoardController.Instance.ClearBoard();
            _onGoToMenuCallback = onLoaded;
            _gameSM.ChangeState(EGameState.None);
        }

        public void PauseGame()
        {
            if (CurState != EGameState.Pause)
                _lastState = CurState;
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
#if UNITY_EDITOR
            if (IsPlayTest) return;
#endif
            Debug.Log("Level Failed");
            _gameSM.ChangeState(EGameState.Lose);
        }

        public void ClearedLevel()
        {
#if UNITY_EDITOR
            if (IsPlayTest) return;
#endif
            Debug.Log("Level Finished");
            _gameSM.ChangeState(EGameState.Win);
        }

        public void ChangeMoveCount(int amount, bool updateVisual = true)
        {
            CurrentLevelData.ChangeMoveAmount(amount);
            if (updateVisual)
                IngameVisualController.Instance.UpdateMoveCount(CurrentLevelData.MoveCount);
        }

        private void OnPillarFullMatched(string tag)
        {
            CurrentLevelData.IncreaseMatchedPillars();
            IngameVisualController.Instance.UpdateProgressBar(CurrentLevelData.MatchedGroups, CurrentLevelData.TotalGroups);
        }

        public void StartLevel(LevelRuntimeData levelData, bool isPlayTest = false)
        {
            if (UserManager.CurUser.HeartCount == 0)
            {
                //TODO: Show buy heart popup
            }

#if UNITY_EDITOR
            IsPlayTest = isPlayTest;
#endif
            if (levelData == null)
            {
                Debug.Log("No level data");
                return;
            }
            Debug.Log($"Start level {levelData.Index}");
            LevelManager.Instance.SetPlayingLevel(levelData);
            BlockMovementController.Instance.Init();
            // BoardController.Instance.ClearBoard();
            BoardController.Instance.InitBoard(CurrentLevelData);
            _pillars = BoardController.Instance.GetAllPillars();
            BoosterController.Instance.InitData();
            IngameVisualController.Instance.InitVisual(CurrentLevelData);
            _onStartNewLevel?.Invoke(CurrentLevelData.Index);
            
            _gameSM.ChangeState(EGameState.Playing);
        }

        protected override void Awake()
        {
            Application.targetFrameRate = 60;

            base.Awake();

            MenuState menuState = new(EGameState.None);
            PlayingState playingState = new(EGameState.Playing);
            PauseState pauseState = new(EGameState.Pause);
            ReviveState reviveState = new(EGameState.Revive);
            WinState winState = new(EGameState.Win);
            LoseState loseState = new(EGameState.Lose);

            _gameSM.AddStates(menuState, playingState, pauseState, reviveState, winState, loseState);
            _gameSM.SetDefaultState(EGameState.None);
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

        public abstract class GameState : AState<EGameState>
        {
            protected GameState(EGameState key) : base(key)
            {
            }
        }

        #region Menu State
        public class MenuState : GameState
        {
            public MenuState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();
                
                GameSceneManager.Instance.ChangeScene(EGameScene.Menu, onLoad: () =>
                {
                    MainMenuVisualControl.Instance.InitVisual();
                    MainMenuVisualControl.Instance.ChangeTab(EMenuTab.Home);

                    Instance._onGoToMenuCallback?.Invoke();
                    Instance._onGoToMenuCallback = null;
                    LevelManager.Instance.SetPlayingLevel(null);
                });
            }
        }
        #endregion

        #region Playing State
        public class PlayingState : GameState
        {
            private enum EPlayingSubState
            {
                Opening,
                Tutorial,
                WhilePlaying,
                Closing,
            }

            private LevelRuntimeData CurLevel => LevelManager.PlayingLevel;
            private StateMachine<EPlayingSubState> _playingSM = new();

            public PlayingState(EGameState key) : base(key)
            {
                var openingState = new OpeningState(EPlayingSubState.Opening);
                var tutorialState = new TutorialState(EPlayingSubState.Tutorial);
                var whilePlayingState = new WhilePlayingState(EPlayingSubState.WhilePlaying);
                var closingState = new ClosingState(EPlayingSubState.Closing);

                _playingSM.AddStates(openingState, tutorialState, whilePlayingState, closingState);
                _playingSM.SetDefaultState(EPlayingSubState.WhilePlaying);
            }

            public override void Do()
            {
                base.Do();
                _playingSM.DoState();
            }

            public override void Enter()
            {
                base.Enter();

                _playingSM.ChangeState(EPlayingSubState.Opening);
            }

            public override void Exit()
            {
                base.Exit();

                _playingSM.CurrentState?.Exit();
                _playingSM.Reset();
            }

            private class PlayingSubState : AState<EPlayingSubState>
            {
                public PlayingSubState(EPlayingSubState key) : base(key)
                {
                }
            }

#region Opening State
            private class OpeningState : PlayingSubState
            {
                private Coroutine _coroutine;
                private bool _playOpening;

                public OpeningState(EPlayingSubState key) : base(key)
                {
                    _playOpening = false;
                    Instance._onStartNewLevel.AddListener((_) => _playOpening = false);
                }

                public override void Enter()
                {
                    base.Enter();

                    if (!_playOpening)
                    {
                        _playOpening = true;
                        _coroutine = Instance.StartCoroutine(DoOpeningAnim());
                    }
                    else
                    {
                        FinishState();
                    }
                }

                public override void Exit()
                {
                    base.Exit();
                    if (_coroutine != null) Instance.StopCoroutine(_coroutine);
                }

                private IEnumerator DoOpeningAnim()
                {
                    IngameVisualController.Instance.PrepareIntroducingAnim();

                    yield return BoardController.Instance.DoSpawnBlockAnim();
                    yield return IngameVisualController.Instance.DoLevelIntroducingAnim();
                    FinishState();
                    Debug.Log($"Finished state {Key}");
                }

                public override EPlayingSubState GetNextState()
                {
                    if (IsFinished) return EPlayingSubState.Tutorial;

                    return base.GetNextState();
                }
            }
            #endregion

            #region Tutorial State
            private class TutorialState : PlayingSubState
            {
                private ETutorial _toPlay;

                public TutorialState(EPlayingSubState key) : base(key)
                {
                    _toPlay = ETutorial.None;
                }

                public override void Enter()
                {
                    base.Enter();

                    if(Instance.CurrentLevelData.Index == UserManager.CurUser.CurrentLevelIndex && TutorialManager.CheckCanPlayTutorial(out var toPlay))
                    {
                        _toPlay = toPlay;
                        Instance.StartCoroutine(PopupManager.Instance.ShowTutorial(toPlay));
                        PopupManager.Instance.OnPopupHidden.AddListener(FinishState);
                    }
                    else
                    {
                        FinishState();
                    }
                }

                public override void Do()
                {
                    base.Do();

                    // if (Input.GetKeyDown(KeyCode.F)) FinishState();
                    //TODO: Finish after complete tutorial
                }

                public override void Exit()
                {
                    if (_toPlay != ETutorial.None) UserManager.MarkTutorialPlayed(_toPlay);
                    PopupManager.Instance.OnPopupHidden.RemoveListener(FinishState);
                    base.Exit();
                }

                public override EPlayingSubState GetNextState()
                {
                    if (IsFinished) return EPlayingSubState.WhilePlaying;

                    return base.GetNextState();
                }
            }
            #endregion

            #region While Playing State
            private class WhilePlayingState : PlayingSubState
            {
                private bool _doRevive = false;

                public WhilePlayingState(EPlayingSubState key) : base(key)
                {
                    Instance._onStartNewLevel.AddListener((_) => _doRevive = false);
                }

                public override void Enter()
                {
                    base.Enter();

                    foreach (var pillar in Instance._pillars)
                    {
                        pillar.OnPillarClicked.AddListener(BlockMovementController.Instance.OnPillarClicked);
                        pillar.OnFullMatched.AddListener(Instance.OnPillarFullMatched);
                        // BlockMovementController.Instance.OnBlocksMoved.AddListener((_) => pillar.CheckFullMatch());
                    }
                    BlockMovementController.Instance.OnBlocksMoved.AddListener(OnBlocksMoved);
                }

                public override void Exit()
                {
                    base.Exit();

                    BlockMovementController.Instance.OnBlocksMoved.RemoveListener(OnBlocksMoved);
                    foreach (var pillar in Instance._pillars)
                    {
                        pillar.OnPillarClicked.RemoveListener(BlockMovementController.Instance.OnPillarClicked);
                        pillar.OnFullMatched.RemoveListener(Instance.OnPillarFullMatched);
                    }
                }

                public override EPlayingSubState GetNextState()
                {
                    if (CheckFinsihLevel()) return EPlayingSubState.Closing;
                    return base.GetNextState();
                }

                private void OnBlocksMoved(bool byPlayer)
                {
                    if (byPlayer) Instance.ChangeMoveCount(-1);
                    foreach (var pillar in Instance._pillars) pillar.CheckFullMatch();
                    CheckFinsihLevel();
                }

                private bool CheckFinsihLevel()
                {
                    if (Instance.CurrentLevelData.MatchedGroups == Instance.CurrentLevelData.TotalGroups)
                    {
                        return true;
                    }

                    if (Instance.CurrentLevelData.MoveCount <= 0)
                    {
                        if (!_doRevive)
                        {
                            _doRevive = true;
                            Instance._gameSM.ChangeState(EGameState.Revive);
                            Debug.Log("Waiting for reviving");
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
#endregion

#region Closing State
            private class ClosingState : PlayingSubState
            {
                private Coroutine _coroutine;
                private WaitForSeconds _delayCleared = new(3f);
                private WaitForSeconds _delayFailed = new(1f);

                public ClosingState(EPlayingSubState key) : base(key)
                {
                }

                public override void Enter()
                {
                    base.Enter();

                    _coroutine = Instance.StartCoroutine(WaitAnimFinish());
                }

                public override void Exit()
                {
                    base.Exit();
                    if (_coroutine != null) Instance.StopCoroutine(_coroutine);
                }

                private bool CheckLevelCleared()
                {
                    if (Instance.CurrentLevelData.MatchedGroups == Instance.CurrentLevelData.TotalGroups)
                    {
                        return true;
                    }

                    return false;
                }

                private IEnumerator WaitAnimFinish()
                {
                    var clearState = CheckLevelCleared();
                    yield return clearState ? _delayCleared : _delayFailed;
                    // yield return ParticleManager.Instance.GetParticleDuration(EParticle.Confetti, true);
                    // yield return BlockMovementController.Instance.CompleteCoroutine;
                    FinishLevel(clearState);
                }

                private void FinishLevel(bool clearState)
                {
                    BoardController.Instance.ClearBoard();

                    if (clearState)
                    {
                        Debug.Log("Level Cleared");
                        Instance.ClearedLevel();
                    }
                    else
                    {
                        Debug.Log("Level Failed");
                        Instance.FailedLevel();
                    }
                    // if (Instance.CurrentLevelData.MatchedGroups == Instance.CurrentLevelData.TotalGroups)
                    // {
                    //     Debug.Log("Level Cleared");
                    //     Instance.ClearedLevel();
                    //     return;
                    // }

                    // if (Instance.CurrentLevelData.MoveCount <= 0)
                    // {
                    //     Debug.Log("Level Failed");
                    //     Instance.FailedLevel();
                    // }

                    // Debug.Log("HUH???");
                }
            }
#endregion
        }
        #endregion

        #region Pause State
        public class PauseState : GameState
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
        public class ReviveState : GameState
        {
            public ReviveState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();
                PopupManager.Instance.ShowBundlePopup(EPopup.Revive, BundleManager.Instance.GetReviveBundle());
            }
        }
        #endregion

        #region Win State
        public class WinState : GameState
        {
            public WinState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();

                Instance.StartCoroutine(PopupManager.Instance.ShowPopup(EPopup.Win));
                Instance.CurrentLevelData.FinishLevel();
            }
        }
        #endregion

        #region Lose State
        public class LoseState : GameState
        {
            public LoseState(EGameState key) : base(key)
            {
            }

            public override void Enter()
            {
                base.Enter();

                UserManager.LostHeart();
                Instance.StartCoroutine(PopupManager.Instance.ShowPopup(EPopup.Lose));
            }
        }
        #endregion
    }
}