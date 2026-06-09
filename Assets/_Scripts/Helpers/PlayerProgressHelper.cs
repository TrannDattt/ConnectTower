using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class PlayerProgressHelper
    {
        public const int ExtraMoveMilestone = 8;
        public const int ShuffleMilestone = 18;
        public const int HintMilestone = 33;
        public const int AddPillarMilestone = 46;
        public const int HiddenBlockMilestone = 4;
        public const int CoveredPillarMilestone = 13;
        public const int FrozenBlockMilestone = 26;
        public const int StickyBlockMilestone = 31;
        public const int ScratchBlockMilestone = 39;
        public const int TrapPillarMilestone = 43;

        public static bool CheckUnlockBooster(EBooster type, bool exactLevel = false, bool passMilestone = false)
        {
            var curIndex = LevelManager.PlayingLevel != null ? LevelManager.PlayingLevel.Index : UserManager.CurUser.CurrentLevelIndex;
            var toCompare = type switch
            {
                EBooster.ExtraMove => ExtraMoveMilestone,
                EBooster.Shuffle => ShuffleMilestone,
                EBooster.Hint => HintMilestone,
                EBooster.AddPillar => AddPillarMilestone,
                _ => Mathf.Infinity
            };
            return exactLevel ? curIndex == toCompare : curIndex >= toCompare + (passMilestone ? 1 : 0);
        }

        public static bool CheckUnlockMechanic(EMechanic type, bool exactLevel = false, bool passMilestone = false)
        {
            var toCompare = type switch
            {
                EMechanic.HiddenBlock => HiddenBlockMilestone,
                EMechanic.CoveredPillar => CoveredPillarMilestone,
                EMechanic.FrozenBlock => FrozenBlockMilestone,
                EMechanic.StickyBlock => StickyBlockMilestone,
                EMechanic.ScratchBlock => ScratchBlockMilestone,
                EMechanic.TrapPillar => TrapPillarMilestone,
                _ => Mathf.Infinity
            };
            return exactLevel ? LevelManager.PlayingLevel.Index == toCompare : LevelManager.PlayingLevel.Index >= toCompare + (passMilestone ? 1 : 0);
        }
    }
}