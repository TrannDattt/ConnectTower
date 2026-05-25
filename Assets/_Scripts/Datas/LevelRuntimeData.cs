using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using UnityEngine;

namespace Assets._Scripts.Datas
{
    public class LevelRuntimeData
    {
        public int Index;
        public EDifficulty Difficulty;
        public int MoveLimit;
        public int MoveCount;

        public List<BlockGroup> BlockGroups;
        public int TotalGroups {get; private set;}
        public int MatchedGroups => _matchedGroups.Count;
        private HashSet<string> _matchedGroups = new();
        
        public List<PillarData> PillarDatas;

        public HiddenBlockData HiddenBlockDatas;
        public List<CoveredPillarData> CoveredPillarDatas;
        public List<FrozenBlockData> FrozenBlockDatas;
        public ScratchBlockData ScratchedBlockDatas;
        public StickyBlockData StickyBlockDatas;
        public List<TrapPillarData> TrapPillarDatas;

        public int HighScore;
        public int CurrentScore;

        public int CoinReward;

        public bool IsCleared => UserManager.CurUser.CurrentLevelIndex > Index;
        public bool IsLocked => UserManager.CurUser.CurrentLevelIndex < Index;

        public LevelRuntimeData()
        {
            Index = -1;
            Difficulty = EDifficulty.Normal;
            MoveLimit = 0;
            MoveCount = 0;

            _matchedGroups.Clear();
            BlockGroups = new();
            PillarDatas = new();
            TotalGroups = 0;

            HiddenBlockDatas = new();
            CoveredPillarDatas = new();
            FrozenBlockDatas = new();
            ScratchedBlockDatas = new();
            StickyBlockDatas = new();
            TrapPillarDatas = new();

            HighScore = 0;
            CurrentScore = 0;
            InitScoreBindings();

            CoinReward = 0;
        }
        
        public LevelRuntimeData(LevelJSON levelData)
        {
            if (levelData == null) return;
            Index = levelData.Index;
            Difficulty = levelData.Difficulty;
            MoveLimit = levelData.MoveLimit;
            MoveCount = MoveLimit;

            _matchedGroups.Clear();
            BlockGroups = levelData.BlockGroups;
            PillarDatas = levelData.PillarDatas;
            TotalGroups = BlockGroups.Count(bg => bg.Trackable);

            HiddenBlockDatas = levelData.HiddenBlockDatas;
            CoveredPillarDatas = levelData.CoveredPillarDatas;
            FrozenBlockDatas = levelData.FrozenBlockDatas;
            ScratchedBlockDatas = levelData.ScratchedBlockDatas;
            StickyBlockDatas = levelData.StickyBlockDatas;
            TrapPillarDatas = levelData.TrapPillarDatas;

            HighScore = levelData.HighScore;
            CurrentScore = 0;
            InitScoreBindings();

            CoinReward = levelData.CoinReward;
        }

        public LevelRuntimeData(LevelRuntimeData levelData)
        {
            if (levelData == null) return;
            Index = levelData.Index;
            Difficulty = levelData.Difficulty;
            MoveLimit = levelData.MoveLimit;
            MoveCount = MoveLimit;

            _matchedGroups.Clear();
            BlockGroups = levelData.BlockGroups;
            PillarDatas = levelData.PillarDatas;
            TotalGroups = BlockGroups.Count(bg => bg.Trackable);

            HiddenBlockDatas = levelData.HiddenBlockDatas;
            CoveredPillarDatas = levelData.CoveredPillarDatas;
            FrozenBlockDatas = levelData.FrozenBlockDatas;
            ScratchedBlockDatas = levelData.ScratchedBlockDatas;
            StickyBlockDatas = levelData.StickyBlockDatas;
            TrapPillarDatas = levelData.TrapPillarDatas;

            HighScore = levelData.HighScore;
            CurrentScore = 0;
            InitScoreBindings();

            CoinReward = levelData.CoinReward;
        }

        public void ChangeMoveAmount(int amount)
        {
            MoveCount += amount;
        }

        public void DecreaseMove()
        {
            ChangeMoveAmount(-1);
        }

        public void IncreaseMatchedPillars(string tag)
        {
            _matchedGroups.Add(tag);
        }

        public void FinishLevel() 
        {
            CompletePendingLevelClearBonus();
            UnsubscribeScoreEvent();
            UserManager.UpdateProgress(Index + 1);

            var updatedHighScore = Mathf.Max(CurrentScore, HighScore);
            if (updatedHighScore <= HighScore) return;

            HighScore = updatedHighScore;
            SaveHighScore();
        }

        private void SaveHighScore()
        {
            var levelManager = Object.FindFirstObjectByType<LevelManager>();
            var cachedLevel = levelManager != null ? levelManager.GetLevel(Index) : null;
            if (cachedLevel != null)
            {
                cachedLevel.HighScore = HighScore;
            }

            if (!LevelDataHelper.TryLoadLevel(Index, out var levelJson) || levelJson == null)
            {
                levelJson = new LevelJSON(cachedLevel ?? this);
            }

            levelJson.HighScore = HighScore;
            LevelDataHelper.SaveLevel(levelJson, overwriteExisting: true);
        }

#region SCORE CALC
        private EventBinding<BlocksMovedEvent> _blockMoveBinding;
        private EventBinding<BlocksMatchedEvent> _blockMatchedBinding;
        private EventBinding<PillarFullMatchedEvent> _pillarMatchedBinding;
        private int _resolvingPlayerMoveIndex;
        private int _lastFullMatchMoveIndex;
        private int _consecutiveFullMatchMoveCount;
        private int _currentMoveCombo;
        private bool _isResolvingPlayerMove;
        private bool _currentMoveHasFullMatch;
        private bool _hasAppliedLevelClearBonus;
        private int _levelClearBonusPerMove;

        public void SubscribeScoreEvent()
        {
            EventBus<BlocksMovedEvent>.Subscribe(_blockMoveBinding);
            EventBus<BlocksMatchedEvent>.Subscribe(_blockMatchedBinding);
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarMatchedBinding);
        }

        public void UnsubscribeScoreEvent()
        {
            EventBus<BlocksMovedEvent>.Unsubscribe(_blockMoveBinding);
            EventBus<BlocksMatchedEvent>.Unsubscribe(_blockMatchedBinding);
            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarMatchedBinding);
        }

        private void InitScoreBindings()
        {
            _resolvingPlayerMoveIndex = 0;
            _lastFullMatchMoveIndex = -1;
            _consecutiveFullMatchMoveCount = 0;
            _currentMoveCombo = 1;
            _isResolvingPlayerMove = false;
            _currentMoveHasFullMatch = false;
            _hasAppliedLevelClearBonus = false;
            _levelClearBonusPerMove = 0;

            _blockMoveBinding = new((e) =>
            {
                if (e.MovedByPlayer)
                {
                    ResetComboIfNeeded();
                }
            });

            _blockMatchedBinding = new((evt) =>
            {
                UpdateScore(ScoreCalculator.CalculateBlocksMatchedScore(evt.MatchCount));
            });

            _pillarMatchedBinding = new((evt) =>
            {
                var matchCount = evt.Pillar != null ? evt.Pillar.GetBlockCount() : 0;
                var combo = ResolveCurrentFullMatchCombo();
                UpdateScore(ScoreCalculator.CalculatePillarFullMatchedScore(matchCount, combo));
            });
        }

        public void BeginPlayerMoveScoreResolution()
        {
            _resolvingPlayerMoveIndex++;
            _currentMoveCombo = 1;
            _currentMoveHasFullMatch = false;
            _isResolvingPlayerMove = true;
        }

        public void EndPlayerMoveScoreResolution()
        {
            if (!_currentMoveHasFullMatch)
                _consecutiveFullMatchMoveCount = 0;

            _isResolvingPlayerMove = false;
        }

        public void ApplyLevelClearBonus()
        {
            if (_hasAppliedLevelClearBonus) return;

            _hasAppliedLevelClearBonus = true;
            UpdateScore(ScoreCalculator.CalculateLevelClearScore(MoveCount));
        }

        public bool BeginLevelClearBonusSequence()
        {
            if (_hasAppliedLevelClearBonus) return false;

            _hasAppliedLevelClearBonus = true;
            if (MoveCount <= 0)
            {
                _levelClearBonusPerMove = 0;
                return false;
            }

            _levelClearBonusPerMove = ScoreCalculator.GetMoveLeftMultiplier(MoveCount);
            return _levelClearBonusPerMove > 0;
        }

        public bool ApplyNextLevelClearBonusStep()
        {
            if (!_hasAppliedLevelClearBonus || MoveCount <= 0 || _levelClearBonusPerMove <= 0)
                return false;

            DecreaseMove();
            UpdateScore(_levelClearBonusPerMove);
            if (MoveCount <= 0)
                _levelClearBonusPerMove = 0;
            return true;
        }

        public void CompletePendingLevelClearBonus(bool notify = false)
        {
            if (!_hasAppliedLevelClearBonus || MoveCount <= 0 || _levelClearBonusPerMove <= 0)
                return;

            var remainingMoveCount = MoveCount;
            MoveCount = 0;
            var remainingScore = remainingMoveCount * _levelClearBonusPerMove;
            _levelClearBonusPerMove = 0;
            UpdateScore(remainingScore, notify);
        }

        private int ResolveCurrentFullMatchCombo()
        {
            if (!_isResolvingPlayerMove)
                return 1;

            if (_currentMoveHasFullMatch)
                return _currentMoveCombo;

            _currentMoveHasFullMatch = true;
            _currentMoveCombo = _lastFullMatchMoveIndex == _resolvingPlayerMoveIndex - 1
                ? _consecutiveFullMatchMoveCount + 1
                : 1;

            _consecutiveFullMatchMoveCount = _currentMoveCombo;
            _lastFullMatchMoveIndex = _resolvingPlayerMoveIndex;

            return _currentMoveCombo;
        }

        private void ResetComboIfNeeded()
        {
            if (_isResolvingPlayerMove || _currentMoveHasFullMatch)
                return;

            _consecutiveFullMatchMoveCount = 0;
        }

        public void UpdateScore(int amount, bool notify = true)
        {
            if (amount == 0) return;

            CurrentScore += amount;
            Debug.Log($"Score updated by {amount} => {CurrentScore}");
            if (notify)
            {
                EventBus<UpdateScoreEvent>.Publish(new UpdateScoreEvent());
            }
        }
#endregion
    }

    public struct UpdateScoreEvent : IEvent
    {
        
    }
}
