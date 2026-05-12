using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;
using Assets._Scripts.Managers;
using Assets._Scripts.Helpers;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Assets._Scripts.Controllers
{
    public class BoosterController : Singleton<BoosterController>
    {
#if UNITY_EDITOR
        [SerializeField] private bool _ignoreMilestone;
#endif

        private Dictionary<EBooster, BoosterRuntimeData> _boosterDict = new();
        public bool IsInMechanic {get; private set;}

        public void InitData()
        {
            foreach(EBooster type in Enum.GetValues(typeof(EBooster)))
            {
                _boosterDict[type] = type switch
                {
                    EBooster.ExtraMove => new ExtraMoveBoosterRuntimeData(!PlayerProgressHelper.CheckUnlockBooster(EBooster.ExtraMove), 5),
                    EBooster.Shuffle => new ShuffleBoosterRuntimeData(!PlayerProgressHelper.CheckUnlockBooster(EBooster.Shuffle)),
                    EBooster.Hint => new HintBoosterRuntimeData(!PlayerProgressHelper.CheckUnlockBooster(EBooster.Hint)),
                    EBooster.AddPillar => new AddPillarRuntimeData(!PlayerProgressHelper.CheckUnlockBooster(EBooster.AddPillar), 1),
                    _ => null
                };
            }
        }

        public BoosterRuntimeData GetBoosterData(EBooster type)
        {
            return _boosterDict.ContainsKey(type) ? _boosterDict[type] : null;
        }

        public string GetBoosterDetail(EBooster type)
        {
            var booster = GetBoosterData(type);
            return booster != null ? booster.GetDetail() : "";
        }

        public int GetUseCount(EBooster type)
        {
            return UserManager.CurUser.BoosterCount[type];
        }

        public bool GetLockStatus(EBooster type)
        {
#if UNITY_EDITOR
            if (_ignoreMilestone) return false;
#endif
            var booster = GetBoosterData(type);
            if (booster == null) return false;

            return booster.LockStatus;
        }

        public void UseBooster(EBooster type)
        {
            UserManager.TryLoseBooster(type, 1);
            var data = GetBoosterData(type);
            if (data == null) return;
            IsInMechanic = true;
            data.Do();
        }

        public void FinishBooster() => IsInMechanic = false;

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