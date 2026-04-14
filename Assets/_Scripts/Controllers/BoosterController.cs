using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;
using Assets._Scripts.Managers;
using Assets._Scripts.Helpers;
using System.Collections;

namespace Assets._Scripts.Controllers
{
    public class BoosterController : Singleton<BoosterController>
    {
#if UNITY_EDITOR
        [SerializeField] private bool _ignoreMilestone;
#endif

        private ExtraMoveBoosterRuntimeData _extraMoveBoosterData;
        private ShuffleBoosterRuntimeData _shuffleBoosterData;
        private HintBoosterRuntimeData _hintBoosterData;

        public void InitData()
        {
            _extraMoveBoosterData = new(!PlayerProgressHelper.CheckUnlockBooster(EBooster.ExtraMove), 5);
            _shuffleBoosterData = new(!PlayerProgressHelper.CheckUnlockBooster(EBooster.Shuffle), new(0, 2, 0));
            _hintBoosterData = new(!PlayerProgressHelper.CheckUnlockBooster(EBooster.Hint));
        }

        public BoosterRuntimeData GetBoosterData(EBooster type)
        {
            return type switch
            {
                EBooster.ExtraMove => _extraMoveBoosterData,
                EBooster.Shuffle => _shuffleBoosterData,
                EBooster.Hint => _hintBoosterData,
                _ => null
            };
        }

        public string GetBoosterDetail(EBooster type)
        {
            return type switch
            {
                EBooster.ExtraMove => _extraMoveBoosterData.GetDetail(),
                EBooster.Shuffle => _shuffleBoosterData.GetDetail(),
                EBooster.Hint => _hintBoosterData.GetDetail(),
                _ => null
            };
        }

        public int GetUseCount(EBooster type)
        {
            return type switch
            {
                EBooster.ExtraMove => UserManager.CurUser.ExtraMoveCount,
                EBooster.Shuffle => UserManager.CurUser.ShuffleCount,
                EBooster.Hint => UserManager.CurUser.HintCount,
                _ => -1
            };
        }

        public bool GetLockStatus(EBooster type)
        {
#if UNITY_EDITOR
            if (_ignoreMilestone) return false;
#endif

            return type switch
            {
                EBooster.ExtraMove => _extraMoveBoosterData.LockStatus,
                EBooster.Shuffle => _shuffleBoosterData.LockStatus,
                EBooster.Hint => _hintBoosterData.LockStatus,
                _ => true
            };
        }

        public void UseBooster(EBooster type)
        {
            UserManager.TryLoseBooster(type, 1);
            BoosterRuntimeData data = type switch
            {
                EBooster.ExtraMove => _extraMoveBoosterData,
                EBooster.Shuffle => _shuffleBoosterData,
                EBooster.Hint => _hintBoosterData,
                _ => null
            };

            if (data == null) return;
            data.Do();
        }

        // #if UNITY_EDITOR
        //         [InitializeOnEnterPlayMode]
        protected override void Awake()
        {
            base.Awake();

            InitData();
        }
// #endif
    }
}