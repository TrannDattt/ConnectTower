using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
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
            UserManager.UpdateProgress(Index + 1);
        }
    }
}