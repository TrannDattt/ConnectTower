using System.Collections.Generic;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    public static class TutorialManager
    {
        private static Dictionary<ETutorial, TutorialSO> _tutorialDict = new();
        private static string _path = "Tutorials";

        // ─── PUBLIC API ───────────────────────────────────────────────

        public static bool CheckCanPlayTutorial(out ETutorial toPlay)
        {
            toPlay = ETutorial.None;

            if (CheckCanPlayMechanicTutorial(EMechanic.FrozenBlock)) toPlay = ETutorial.FrozenBlock;
            if (CheckCanPlayBoosterTutorial(EBooster.Hint)) toPlay = ETutorial.Hint;
            if (CheckCanPlayMechanicTutorial(EMechanic.CoveredPillar)) toPlay = ETutorial.CoveredPillar;
            if (CheckCanPlayBoosterTutorial(EBooster.Shuffle)) toPlay = ETutorial.Shuffle;
            if (CheckCanPlayMechanicTutorial(EMechanic.HiddenBlock)) toPlay = ETutorial.HiddenBlock;
            if (CheckCanPlayBoosterTutorial(EBooster.ExtraMove)) toPlay = ETutorial.ExtraMove;

            return toPlay != ETutorial.None;
        }

        /// <summary>Kiểm tra có thể phát tutorial của Booster không (chưa unlock hoặc đã chơi rồi thì bỏ qua).</summary>
        public static bool CheckCanPlayBoosterTutorial(EBooster type)
        {
            if (!PlayerProgressHelper.CheckUnlockBooster(type)) return false;
            if (CheckPlayBoosterTutorialBefore(type)) return false;
            return true;
        }

        /// <summary>Kiểm tra có thể phát tutorial của Mechanic không (chưa unlock hoặc đã chơi rồi thì bỏ qua).</summary>
        public static bool CheckCanPlayMechanicTutorial(EMechanic type)
        {
            if (!PlayerProgressHelper.CheckUnlockMechanic(type)) return false;
            if (CheckPlayMechanicTutorialBefore(type)) return false;
            return true;
        }
        
        public static TutorialSO GetTutorialData(ETutorial type) => _tutorialDict.TryGetValue(type, out var tutorial) ? tutorial : null; 

        /// <summary>Đánh dấu một tutorial Booster đã được phát.</summary>
        public static void MarkBoosterTutorialPlayed(EBooster type)
        {
            var key = BoosterToTutorial(type);
            if (key.HasValue) UserManager.MarkTutorialPlayed(key.Value);
        }

        /// <summary>Đánh dấu một tutorial Mechanic đã được phát.</summary>
        public static void MarkMechanicTutorialPlayed(EMechanic type)
        {
            var key = MechanicToTutorial(type);
            if (key.HasValue) UserManager.MarkTutorialPlayed(key.Value);
        }

        // ─── PRIVATE CHECKS ───────────────────────────────────────────

        private static bool CheckPlayBoosterTutorialBefore(EBooster type)
        {
            var key = BoosterToTutorial(type);
            return key.HasValue && UserManager.HasPlayedTutorial(key.Value);
        }

        private static bool CheckPlayMechanicTutorialBefore(EMechanic type)
        {
            var key = MechanicToTutorial(type);
            return key.HasValue && UserManager.HasPlayedTutorial(key.Value);
        }

        // ─── MAPPINGS ─────────────────────────────────────────────────

        public static ETutorial? BoosterToTutorial(EBooster type) => type switch
        {
            EBooster.ExtraMove => ETutorial.ExtraMove,
            EBooster.Shuffle   => ETutorial.Shuffle,
            EBooster.Hint      => ETutorial.Hint,
            _                  => null
        };

        public static ETutorial? MechanicToTutorial(EMechanic type) => type switch
        {
            EMechanic.HiddenBlock    => ETutorial.HiddenBlock,
            EMechanic.CoveredPillar  => ETutorial.CoveredPillar,
            EMechanic.FrozenBlock    => ETutorial.FrozenBlock,
            _                        => null
        };

        public static EBooster? TutorialToBooster(ETutorial type) => type switch
        {
            ETutorial.ExtraMove => EBooster.ExtraMove,
            ETutorial.Shuffle   => EBooster.Shuffle,
            ETutorial.Hint      => EBooster.Hint,
            _                   => null
        };

        public static EMechanic? TutorialToMechanic(ETutorial type) => type switch
        {
            ETutorial.HiddenBlock   => EMechanic.HiddenBlock,
            ETutorial.CoveredPillar   => EMechanic.CoveredPillar,
            ETutorial.FrozenBlock      => EMechanic.FrozenBlock,
            _ => null
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var datas = Resources.LoadAll<TutorialSO>(_path);
            if (datas.Length == 0) return;

            foreach(var data in datas)
            {
                _tutorialDict[data.Type] = data;
            }
        }
    }
}