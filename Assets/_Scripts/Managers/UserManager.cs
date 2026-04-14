using Assets._Scripts.Datas;
using UnityEngine.Events;
using UnityEngine;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;

namespace Assets._Scripts.Managers
{
    public static class UserManager
    {
        public static UserRuntimeData CurUser {get; private set;}
        public static UnityEvent<int> OnCoinChanged = new();
        public static UnityEvent<EBooster, int> OnBoosterChanged = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (CurUser == null)
            {
                CurUser = UserDataHelper.LoadUser();
                Debug.Log("UserManager Initialized");
            }

            Application.quitting += SaveData;
        }

        public static void SaveData()
        {
            UserDataHelper.SaveUser(CurUser);
        }

#region COIN
        private static void ChangeCoin(int amount)
        {
            if (amount == 0) return;
            CurUser.CoinCount += amount;
            OnCoinChanged?.Invoke(amount);
            SaveData();
        }

        public static void GainCoin(int amount)
        {
            ChangeCoin(Mathf.Abs(amount));
        }

        public static bool TryLoseCoin(int amount)
        {
            int toDecrease = Mathf.Abs(amount);
            if (CurUser.CoinCount < toDecrease)
            {
                return false;
            }

            ChangeCoin(-toDecrease);
            return true;
        }
#endregion

#region Heart
        public static UnityEvent<int> OnHeartChanged = new();
        private static void ChangeHeartCount(int amount)
        {
            Debug.Log($"Player heart changed by {amount}");
            CurUser.HeartCount = Mathf.Clamp(CurUser.HeartCount + amount, 0, UserLifeHelper.MAX_LIFE);
            OnHeartChanged?.Invoke(amount);
            SaveData();
        }

        public static void LostHeart()
        {
            ChangeHeartCount(-1);
            UserLifeHelper.OnLostLife();
        }

        public static void RecoverHeart()
        {
            ChangeHeartCount(1);
            UserLifeHelper.OnRecovered();
        }
#endregion

#region Booster
        private static void ChangeBoosterAmount(EBooster type, int amount)
        {
            if (amount == 0) return;
            switch (type)
            {
                case EBooster.ExtraMove:
                    CurUser.ExtraMoveCount += amount;
                    break;
                case EBooster.Shuffle:
                    CurUser.ShuffleCount += amount;
                    break;
                case EBooster.Hint:
                    CurUser.HintCount += amount;
                    break;
            }
            OnBoosterChanged?.Invoke(type, amount);
            SaveData();
        }

        public static void GainBooster(EBooster type, int amount)
        {
            // Debug.Log($"Gain {amount} booster {type}");
            ChangeBoosterAmount(type, Mathf.Abs(amount));
        }

        public static bool TryLoseBooster(EBooster type, int amount)
        {
            int toDecrease = Mathf.Abs(amount);
            if ((type == EBooster.ExtraMove && CurUser.ExtraMoveCount < toDecrease)
                || (type == EBooster.Shuffle && CurUser.ShuffleCount < toDecrease)
                || (type == EBooster.Hint && CurUser.HintCount < toDecrease))
            {
                return false;
            }

            ChangeBoosterAmount(type, -toDecrease);
            return true;
        }
#endregion

        public static void GetBundle(BundleSO bundle)
        {
            var reward = bundle.Reward;
            GainCoin(reward.CoinAmount);
            //TODO: Logic with heart and Ads
            GainBooster(EBooster.ExtraMove, reward.ExtraMoveAmount);
            GainBooster(EBooster.Shuffle, reward.ShuffleAmount);
            GainBooster(EBooster.Hint, reward.HintAmount);
        }

        public static void UpdateProgress(int levelIndex)
        {
            if (levelIndex > CurUser.CurrentLevelIndex)
            {
                CurUser.CurrentLevelIndex = levelIndex;
                Debug.Log($"Update progress to {CurUser.CurrentLevelIndex}");
            }
        }

#region Tutorial
        public static bool HasPlayedTutorial(ETutorial tutorial) => CurUser.HasPlayedTutorial(tutorial);

        public static void MarkTutorialPlayed(ETutorial tutorial) => CurUser.MarkTutorialPlayed(tutorial);
#endregion
    }
}