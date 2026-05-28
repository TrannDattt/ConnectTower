using Assets._Scripts.Datas;
using UnityEngine.Events;
using UnityEngine;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Services.APIs;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets._Scripts.Managers
{
    public static class UserManager
    {
        private const string SessionDirtyKey = "UserProgressDirty";
        public static UserRuntimeData CurUser {get; private set;}
        public static string TEST_UserID => "CgqFZoKy6BV1BU0Ny7XN";
        private static bool _remoteLoadCompleted;
        private static bool _hasUnsyncedChanges;
        private static bool _isQuitSaveInProgress;
        private static bool _allowQuitAfterSave;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (CurUser == null)
            {
                CurUser = UserDataHelper.LoadUser() ?? CreateDefaultUser();
                EnsureBoosterKeys(CurUser);
                Debug.Log("UserManager Initialized");
            }

            _hasUnsyncedChanges = PlayerPrefs.GetInt(SessionDirtyKey, 0) == 1;
            _remoteLoadCompleted = _hasUnsyncedChanges;
            _isQuitSaveInProgress = false;
            _allowQuitAfterSave = false;

            Application.wantsToQuit -= HandleWantsToQuit;
            Application.wantsToQuit += HandleWantsToQuit;

            if (_hasUnsyncedChanges)
            {
                CurUser.Id = TEST_UserID;
                Debug.LogWarning("Unsynced local user data detected. Skipping Firestore load until data is flushed on quit.");
                return;
            }

            _ = LoadRemoteUserDataAsync();
        }

        public static void SaveData()
        {
            if (CurUser == null)
            {
                return;
            }

            CurUser.Id = TEST_UserID;
            UserDataHelper.SaveUser(CurUser);
            _hasUnsyncedChanges = true;
            PlayerPrefs.SetInt(SessionDirtyKey, 1);
            PlayerPrefs.Save();
        }

        private static async Task LoadRemoteUserDataAsync()
        {
            try
            {
                var userTask = UserAPI.GetUserAsync(TEST_UserID);
                var currencyTask = UserCurrencyAPI.GetUserAsync(TEST_UserID);
                var boosterTask = UserBoosterAPI.GetUserAsync(TEST_UserID);

                await Task.WhenAll(userTask, currencyTask, boosterTask);

                var remoteUser = await userTask;
                var remoteCurrency = await currencyTask;
                var remoteBooster = await boosterTask;

                if (_hasUnsyncedChanges)
                {
                    _remoteLoadCompleted = true;
                    Debug.LogWarning("Local session data changed before Firestore load completed. Keeping local cache.");
                    return;
                }

                var hasRemoteData = remoteUser.HasValue || remoteCurrency.HasValue || remoteBooster.HasValue;
                var hasMissingRemoteSegments = !remoteUser.HasValue || !remoteCurrency.HasValue || !remoteBooster.HasValue;

                CurUser ??= CreateDefaultUser();
                CurUser.Id = TEST_UserID;

                if (remoteUser.HasValue)
                {
                    CurUser.Name = remoteUser.Value.Name;
                    CurUser.AvatarURL = remoteUser.Value.AvatarURL;
                    CurUser.CurrentLevelIndex = Mathf.Max(1, remoteUser.Value.CurrentLevelIndex);
                    CurUser.SetPlayedTutorials(ParseTutorials(remoteUser.Value.PlayedTutorials));
                }

                if (remoteCurrency.HasValue)
                {
                    CurUser.CoinCount = remoteCurrency.Value.Coin;
                    CurUser.HeartCount = remoteCurrency.Value.Heart;
                }

                if (remoteBooster.HasValue)
                {
                    CurUser.BoosterCount = ToBoosterDictionary(remoteBooster.Value.BoostersCount);
                }

                EnsureBoosterKeys(CurUser);
                _remoteLoadCompleted = true;
                UserDataHelper.SaveUser(CurUser);

                if (!hasRemoteData)
                {
                    _hasUnsyncedChanges = true;
                    PlayerPrefs.SetInt(SessionDirtyKey, 1);
                    PlayerPrefs.Save();
                    Debug.LogWarning($"No remote data found for user '{TEST_UserID}'. Using local/default snapshot and deferring Firestore creation until quit.");
                    return;
                }

                if (hasMissingRemoteSegments)
                {
                    _hasUnsyncedChanges = true;
                    PlayerPrefs.SetInt(SessionDirtyKey, 1);
                    PlayerPrefs.Save();
                    Debug.LogWarning($"Remote data for '{TEST_UserID}' is incomplete. Missing fields will be written back to Firestore on quit.");
                }
                else
                {
                    _hasUnsyncedChanges = false;
                    PlayerPrefs.SetInt(SessionDirtyKey, 0);
                    PlayerPrefs.Save();
                }

                Debug.Log($"Remote user data synced for {CurUser.Id}.");
            }
            catch (Exception ex)
            {
                _remoteLoadCompleted = true;
                _hasUnsyncedChanges = true;
                PlayerPrefs.SetInt(SessionDirtyKey, 1);
                PlayerPrefs.Save();
                Debug.LogWarning($"Failed to sync remote user data from Firestore. Using local snapshot for this session. {ex}");
            }
        }

        private static bool HandleWantsToQuit()
        {
            if (_allowQuitAfterSave)
            {
                return true;
            }

            if (_isQuitSaveInProgress)
            {
                return false;
            }

            if (!_hasUnsyncedChanges)
            {
                return true;
            }

            _isQuitSaveInProgress = true;
            _ = SaveToFirestoreAndQuitAsync();
            return false;
        }

        private static async Task SaveToFirestoreAndQuitAsync()
        {
            try
            {
                EnsureBoosterKeys(CurUser);
                CurUser.Id = TEST_UserID;

                var userTask = UserAPI.SetUserAsync(ToUserModel(CurUser));
                var currencyTask = UserCurrencyAPI.SetUserAsync(ToUserCurrencyModel(CurUser));
                var boosterTask = UserBoosterAPI.SetUserAsync(ToUserBoosterModel(CurUser));

                await Task.WhenAll(userTask, currencyTask, boosterTask);
                _hasUnsyncedChanges = false;
                PlayerPrefs.SetInt(SessionDirtyKey, 0);
                PlayerPrefs.Save();
                Debug.Log($"Saved user data to Firestore for {CurUser.Id} on quit.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save user data to Firestore on quit: {ex}");
            }
            finally
            {
                _isQuitSaveInProgress = false;
                _allowQuitAfterSave = true;
                RequestQuit();
            }
        }

        private static UserRuntimeData CreateDefaultUser()
        {
            var user = new UserRuntimeData
            {
                Id = TEST_UserID,
            };

            return user;
        }

        private static UserModel ToUserModel(UserRuntimeData user)
        {
            return new UserModel
            {
                UID = TEST_UserID,
                Name = user.Name,
                AvatarURL = user.AvatarURL,
                CurrentLevelIndex = user.CurrentLevelIndex,
                PlayedTutorials = user.GetPlayedTutorials()
                                     .Where(tutorial => tutorial != ETutorial.None)
                                     .Select(tutorial => tutorial.ToString())
                                     .ToArray(),
            };
        }

        private static UserCurrencyModel ToUserCurrencyModel(UserRuntimeData user)
        {
            return new UserCurrencyModel
            {
                UID = TEST_UserID,
                Coin = user.CoinCount,
                Heart = user.HeartCount,
            };
        }

        private static UserBoosterModel ToUserBoosterModel(UserRuntimeData user)
        {
            EnsureBoosterKeys(user);

            return new UserBoosterModel
            {
                UID = TEST_UserID,
                BoostersCount = user.BoosterCount
                                   .Where(pair => pair.Key != EBooster.None)
                                   .Select(pair => new BoosterModel
                                   {
                                       Type = pair.Key,
                                       Count = pair.Value,
                                   })
                                   .ToArray(),
            };
        }

        private static Dictionary<EBooster, int> ToBoosterDictionary(BoosterModel[] boosters)
        {
            var result = new Dictionary<EBooster, int>();
            if (boosters == null)
            {
                return result;
            }

            foreach (var booster in boosters)
            {
                if (booster.Type == EBooster.None)
                {
                    continue;
                }

                result[booster.Type] = Mathf.Max(0, booster.Count);
            }

            return result;
        }

        private static IEnumerable<ETutorial> ParseTutorials(string[] tutorialNames)
        {
            if (tutorialNames == null)
            {
                yield break;
            }

            foreach (var tutorialName in tutorialNames)
            {
                if (Enum.TryParse(tutorialName, true, out ETutorial tutorial) && tutorial != ETutorial.None)
                {
                    yield return tutorial;
                }
            }
        }

        private static void EnsureBoosterKeys(UserRuntimeData user)
        {
            if (user == null)
            {
                return;
            }

            user.BoosterCount ??= new Dictionary<EBooster, int>();

            foreach (var booster in new[] { EBooster.ExtraMove, EBooster.Shuffle, EBooster.Hint, EBooster.AddPillar, EBooster.Portal })
            {
                if (!user.BoosterCount.ContainsKey(booster))
                {
                    user.BoosterCount[booster] = 0;
                }
            }
        }

        private static void RequestQuit()
        {
            Application.wantsToQuit -= HandleWantsToQuit;
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }
#endif
            Application.Quit();
        }

#region INFO
        public static void ChangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) && string.Equals(CurUser.Name, name))
            {
                return;
            }
            CurUser.Name = name;
            SaveData();
        }

        public static void ChangeAvatar(string avatarUrl)
        {
            avatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? "*" : avatarUrl;

            if (CurUser != null && string.Equals(CurUser.AvatarURL, avatarUrl, StringComparison.Ordinal))
            {
                return;
            }

            CurUser.AvatarURL = avatarUrl;
            SaveData();
        }
        #endregion

        #region COIN
        private static void ChangeCoin(int amount)
        {
            if (amount == 0) return;
            CurUser.CoinCount += amount;
            EventBus<CurrencyChangedEvent>.Publish(new CurrencyChangedEvent
            {
                CoinChanged = amount,
                HeartChanged = 0,
                BoostersChanged = Array.Empty<Tuple<EBooster, int>>()
            });
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
        private static void ChangeHeartCount(int amount)
        {
            Debug.Log($"Player heart changed by {amount}");
            CurUser.HeartCount = Mathf.Clamp(CurUser.HeartCount + amount, 0, UserLifeHelper.MAX_LIFE);
            EventBus<CurrencyChangedEvent>.Publish(new CurrencyChangedEvent
            {
                CoinChanged = 0,
                HeartChanged = amount,
                BoostersChanged = Array.Empty<Tuple<EBooster, int>>()
            });
            SaveData();
        }

        public static void LostHeart()
        {
            ChangeHeartCount(-1);
            UserLifeHelper.UpdateCounterOnLostLife();
        }

        public static void RecoverHeart()
        {
            ChangeHeartCount(1);
            UserLifeHelper.UpdateCounterOnRecovered();
        }
#endregion

#region Booster
        private static void ChangeBoosterAmount(EBooster type, int amount)
        {
            if (amount == 0) return;
            EnsureBoosterKeys(CurUser);
            CurUser.BoosterCount[type] += amount;
            EventBus<CurrencyChangedEvent>.Publish(new CurrencyChangedEvent
            {
                CoinChanged = 0,
                HeartChanged = 0,
                BoostersChanged = new[] { Tuple.Create(type, amount) }
            });
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
            EnsureBoosterKeys(CurUser);
            if (CurUser.BoosterCount[type] < toDecrease)
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
            //TODO: Add logic with heart and Ads
            ChangeHeartCount(reward.HeartAmount);
            foreach (var boosterReward in reward.BoosterRewards) 
                GainBooster(boosterReward.Type, boosterReward.Amount);
            // GainBooster(EBooster.ExtraMove, reward.ExtraMoveAmount);
            // GainBooster(EBooster.Shuffle, reward.ShuffleAmount);
            // GainBooster(EBooster.Hint, reward.HintAmount);
        }

        public static void UpdateProgress(int levelIndex, bool forceUpdate = false)
        {
            if (levelIndex > CurUser.CurrentLevelIndex || forceUpdate)
            {
                CurUser.CurrentLevelIndex = levelIndex;
                Debug.Log($"Update progress to {CurUser.CurrentLevelIndex}");
                SaveData();
            }
        }

#region Tutorial
        public static bool HasPlayedTutorial(ETutorial tutorial) => CurUser.HasPlayedTutorial(tutorial);

        public static void MarkTutorialPlayed(ETutorial tutorial)
        {
            CurUser.MarkTutorialPlayed(tutorial);
            SaveData();
        }
#endregion
    }

    public struct CurrencyChangedEvent : IEvent
    {
        public int CoinChanged;
        public int HeartChanged;
        public Tuple<EBooster, int>[] BoostersChanged;
    }
}
