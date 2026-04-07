using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;
using Assets._Scripts.Managers;

namespace Assets._Scripts.Controllers
{
    public class BoosterController : Singleton<BoosterController>
    {
        [SerializeField] private Sprite _extraMoveBoosterIcon;
        [SerializeField] private Sprite _shuffleBoosterIcon;
        [SerializeField] private Sprite _hintBoosterIcon;
        [SerializeField] private Transform _gatherPoint;

        private ExtraMoveBoosterRuntimeData _extraMoveBoosterData;
        private ShuffleBoosterRuntimeData _shuffleBoosterData;
        private HintBoosterRuntimeData _hintBoosterData;

        private bool _isUsingBooster = false;

        public void InitData()
        {
            _extraMoveBoosterData = new(false, 5);
            _shuffleBoosterData = new(false, new(0, 2, 0));
            _hintBoosterData = new(false);
        }

        public Sprite GetBoosterIcon(EBooster type)
        {
            return type switch
            {
                EBooster.ExtraMove => _extraMoveBoosterIcon,
                EBooster.Shuffle => _shuffleBoosterIcon,
                EBooster.Hint => _hintBoosterIcon,
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
            if (_isUsingBooster) return;

            UserManager.TryLoseBooster(type, 1);
            BoosterRuntimeData data = type switch
            {
                EBooster.ExtraMove => _extraMoveBoosterData,
                EBooster.Shuffle => _shuffleBoosterData,
                EBooster.Hint => _hintBoosterData,
                _ => null
            };

            if (data == null) return;
            _isUsingBooster = true;

            var boosterSFX = type switch
            {
                EBooster.ExtraMove => ESfx.ExtraMove,
                EBooster.Shuffle => ESfx.Shuffle,
                EBooster.Hint => ESfx.Hint,
                _ => ESfx.None
            };
            SoundManager.Instance.PlayRandomSFX(boosterSFX);

            data.Do();
            var anim = data.DoMechanicAnim();
            if (anim != null)
            {
                anim.OnComplete(() => _isUsingBooster = false);
            }
            else
            {
                _isUsingBooster = false;
            }
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