using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class PlayerProgressHelper
    {
        public const int ExtraMoveMilestone = 5;
        public const int HiddenBlockMilestone = 7;
        public const int ShuffleMilestone = 15;
        public const int CoveredPillarMilestone = 19;
        public const int HintMilestone = 25;
        public const int FrozenBlockMilestone = 100;

        public static bool CheckUnlockBooster(EBooster type, bool exactLevel = false)
        {
            var curIndex = UserManager.CurUser.CurrentLevelIndex;
            var toCompare = type switch
            {
                EBooster.ExtraMove => ExtraMoveMilestone,
                EBooster.Shuffle => ShuffleMilestone,
                EBooster.Hint => HintMilestone,
                _ => Mathf.Infinity
            };
            return exactLevel ? curIndex == toCompare : curIndex >= toCompare;
        }

        public static bool CheckUnlockMechanic(EMechanic type)
        {
            var toCompare = type switch
            {
                EMechanic.HiddenBlock => HiddenBlockMilestone,
                EMechanic.CoveredPillar => CoveredPillarMilestone,
                EMechanic.FrozenBlock => FrozenBlockMilestone,
                _ => Mathf.Infinity
            };
            return UserManager.CurUser.CurrentLevelIndex >= toCompare;
        }
    }
}