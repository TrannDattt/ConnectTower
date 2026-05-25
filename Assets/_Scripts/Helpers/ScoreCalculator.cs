using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class ScoreCalculator
    {
        private const int BlocksMatchedBaseScore = 10;
        private const int PillarFullMatchedBaseScore = 25;
        private const int MoveLeftBaseScore = 50;

        public static int CalculateBlocksMatchedScore(int matchCount)
        {
            if (BlocksMatchedBaseScore <= 0 || matchCount <= 0) return 0;

            var multFactor = GetMatchMultiplier(matchCount);
            return BlocksMatchedBaseScore * matchCount * multFactor;
        }

        public static int CalculatePillarFullMatchedScore(int matchCount, int combo)
        {
            if (PillarFullMatchedBaseScore <= 0 || matchCount <= 0) return 0;

            var multFactor = GetMatchMultiplier(matchCount);
            var comboFactor = Mathf.Max(1, combo);
            return PillarFullMatchedBaseScore * matchCount * multFactor * comboFactor;
        }

        public static int CalculateLevelClearScore(int moveCountLeft)
        {
            if (moveCountLeft <= 0) return 0;

            var multFactor = GetMoveLeftMultiplier(moveCountLeft);
            return moveCountLeft * multFactor;
        }

        public static int GetMatchMultiplier(int matchCount)
        {
            if (matchCount <= 1) return 1;

            return Mathf.Max(1, matchCount - 1);
        }

        public static int GetMoveLeftMultiplier(int moveCountLeft)
        {
            if (moveCountLeft <= 0) return 0;

            var multFactor = 1 + Mathf.CeilToInt(moveCountLeft / 5f);
            return MoveLeftBaseScore * moveCountLeft * multFactor;
        }
    }
}
