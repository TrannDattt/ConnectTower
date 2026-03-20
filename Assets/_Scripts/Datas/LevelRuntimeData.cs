using System.Collections.Generic;
using Assets._Scripts.Enums;

namespace Assets._Scripts.Datas
{
    public class LevelRuntimeData
    {
        public int Index;
        public EDifficulty Difficulty;
        public int MoveCount;

        public List<BlockGroup> BlockGroups;
        public int MatchedGroups { get; private set; }
        public int TotalGroups => BlockGroups.Count;
        
        public LevelRuntimeData(LevelSO levelData)
        {
            Index = levelData.Index;
            Difficulty = levelData.Difficulty;
            MoveCount = levelData.MoveLimit;
            BlockGroups = levelData.BlockGroups;
            MatchedGroups = 0;
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
    }
}