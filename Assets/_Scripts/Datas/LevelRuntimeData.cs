using System.Collections.Generic;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;

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

        public bool IsCleared => UserManager.CurUser.CurrentLevelIndex > Index;

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
        }
        
        public LevelRuntimeData(LevelJSON levelData)
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
        }

        public LevelRuntimeData(LevelRuntimeData levelData)
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

        public void FinishLevel() 
        {
            UserManager.UpdateProgress(Index + 1);
        }
    }
}