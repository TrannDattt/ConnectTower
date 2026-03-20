using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets._Scripts.Controllers
{
    public static class BoosterController
    // public class BoosterController : Singleton<BoosterController>
    {
        static private ExtraMoveBoosterRuntimeData _extraMoveBoosterData;
        static private ShuffleBoosterRuntimeData _shuffleBoosterData;
        static private HintBoosterRuntimeData _hintBoosterData;

        //TODO: Get booster datas from player data
        // public void GetBoosterData()
        // {

        // }

        public static int GetUseCount(EBooster type)
        {
            return type switch
            {
                EBooster.ExtraMove => _extraMoveBoosterData.UseCount,
                EBooster.Shuffle => _shuffleBoosterData.UseCount,
                EBooster.Hint => _hintBoosterData.UseCount,
                _ => -1
            };
        }

        public static bool GetLockStatus(EBooster type)
        {
            return type switch
            {
                EBooster.ExtraMove => _extraMoveBoosterData.LockStatus,
                EBooster.Shuffle => _shuffleBoosterData.LockStatus,
                EBooster.Hint => _hintBoosterData.LockStatus,
                _ => true
            };
        }

        public static void UseBooster(EBooster type)
        {
            switch (type)
            {
                case EBooster.ExtraMove:
                    _extraMoveBoosterData.Do();
                    break;
                case EBooster.Shuffle:
                    _shuffleBoosterData.Do();
                    break;
                case EBooster.Hint:
                    _hintBoosterData.Do();
                    break;
            }
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        static void Start()
        {
            //---------------------
            _extraMoveBoosterData = new(10, false, 5);
            _shuffleBoosterData = new(10, false);
            _hintBoosterData = new(10, false);
            //---------------------
        }
#endif
    }
}