using System.Collections.Generic;
using Assets._Scripts.Enums;

namespace Assets._Scripts.Datas
{
    public class LevelRuntimeData
    {
        public int Index;
        public EDifficulty Difficulty;
        public int MoveLimit;
        public int MoveCount;

        public List<BlockGroup> BlockGroups;
        public int MatchedGroups { get; private set; }
        public int TotalGroups => BlockGroups.Count;
        
        public List<PillarData> PillarDatas;


        public HiddenBlockData HiddenBlockDatas;
        public List<CoveredPillarData> CoveredPillarDatas;
        public List<FrozenBlockData> FrozenBlockDatas;

        public int CoinReward;

        public bool IsCleared {get; private set;}

        public LevelRuntimeData()
        {
            Index = -1;
            Difficulty = EDifficulty.Normal;
            MoveLimit = 0;
            MoveCount = 0;

            BlockGroups = new List<BlockGroup>();
            MatchedGroups = 0;
            PillarDatas = new List<PillarData>();

            HiddenBlockDatas = new HiddenBlockData();
            CoveredPillarDatas = new List<CoveredPillarData>();
            FrozenBlockDatas = new List<FrozenBlockData>();
            
            CoinReward = 0;
            IsCleared = false;
        }
        
        public LevelRuntimeData(LevelSO levelData)
        {
            if (levelData == null) return;
            Index = levelData.Index;
            Difficulty = levelData.Difficulty;
            MoveLimit = levelData.MoveLimit;
            MoveCount = MoveLimit;

            BlockGroups = levelData.BlockGroups;
            MatchedGroups = 0;
            PillarDatas = levelData.PillarDatas;

            HiddenBlockDatas = levelData.HiddenBlockDatas;
            CoveredPillarDatas = levelData.CoveredPillarDatas;
            FrozenBlockDatas = levelData.FrozenBlockDatas;

            CoinReward = levelData.CoinReward;
            //TODO: IsCleared ???
        }

        public LevelRuntimeData(LevelRuntimeData levelData)
        {
            if (levelData == null) return;
            Index = levelData.Index;
            Difficulty = levelData.Difficulty;
            MoveLimit = levelData.MoveLimit;
            MoveCount = MoveLimit;

            BlockGroups = levelData.BlockGroups;
            MatchedGroups = levelData.MatchedGroups;
            PillarDatas = levelData.PillarDatas;

            HiddenBlockDatas = levelData.HiddenBlockDatas;
            CoveredPillarDatas = levelData.CoveredPillarDatas;
            FrozenBlockDatas = levelData.FrozenBlockDatas;

            CoinReward = levelData.CoinReward;
            IsCleared = levelData.IsCleared;
        }

        public void ChangeMoveAmount(int amount)
        {
            MoveCount += amount;
        }

        public void DecreaseMove()
        {
            ChangeMoveAmount(-1);
        }

        public void IncreaseMatchedPillars()
        {
            MatchedGroups++;
        }

        public void FinishLevel() => IsCleared = true;
    }
}