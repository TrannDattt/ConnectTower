using System.Collections.Generic;
using Assets._Scripts.Enums;

namespace Assets._Scripts.Datas
{
    public class LevelJSON
    {
        public int Index;
        public EDifficulty Difficulty;
        public int MoveLimit;
        public List<BlockGroup> BlockGroups;
        public List<PillarData> PillarDatas;
        public HiddenBlockData HiddenBlockDatas;
        public List<CoveredPillarData> CoveredPillarDatas;
        public List<FrozenBlockData> FrozenBlockDatas;
        public int CoinReward = 20;

        public LevelJSON()
        {
            Index = -1;
            Difficulty = EDifficulty.Normal;
            MoveLimit = 0;

            BlockGroups = new List<BlockGroup>();
            PillarDatas = new List<PillarData>();

            HiddenBlockDatas = new HiddenBlockData();
            CoveredPillarDatas = new List<CoveredPillarData>();
            FrozenBlockDatas = new List<FrozenBlockData>();
            
            CoinReward = 0;
        }
    }
}