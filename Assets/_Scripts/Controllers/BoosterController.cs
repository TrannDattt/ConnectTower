using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using DG.Tweening;
using UnityEngine;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using UnityEngine.UI;




#if UNITY_EDITOR
using UnityEditor;
#endif

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
            var extraMoveCount = UserManager.CurUser.ExtraMoveCount;
            var shuffleCount = UserManager.CurUser.ShuffleCount;
            var hintCount = UserManager.CurUser.HintCount;

            _extraMoveBoosterData = new(extraMoveCount, false, 5);
            _shuffleBoosterData = new(shuffleCount, false, new(0, 2, 0));
            _hintBoosterData = new(hintCount, false);
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
                EBooster.ExtraMove => _extraMoveBoosterData.UseCount,
                EBooster.Shuffle => _shuffleBoosterData.UseCount,
                EBooster.Hint => _hintBoosterData.UseCount,
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

            //---------------------
            _extraMoveBoosterData = new(3, false, 5);
            _shuffleBoosterData = new(3, false, new(0, 2, 0));
            _hintBoosterData = new(3, false);
            //---------------------
        }
// #endif
    }
}