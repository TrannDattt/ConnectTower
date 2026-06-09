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
        private const string LifecycleObjectName = "__UserManagerLifecycle";
        public static UserRuntimeData CurUser {get; private set;}
        public static bool IsRemoteLoadCompleted => _remoteLoadCompleted;
        public static string TEST_UserID => "CgqFZoKy6BV1BU0Ny7XN";
        private static bool _remoteLoadCompleted;
        private static bool _hasUnsyncedChanges;
        private static bool _isQuitSaveInProgress;
        private static bool _allowQuitAfterSave;
        private static bool _pendingQuitAfterSave;
        private static UserManagerLifecycleProxy _lifecycleProxy;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            EnsureLifecycleProxy();

            if (CurUser == null)
            {
                CurUser = LoadCachedUser() ?? CreateDefaultUser();
                EnsureBoosterKeys(CurUser);
                Debug.Log("UserManager Initialized");
            }

            _hasUnsyncedChanges = HasPersistedUnsyncedChanges();
            _remoteLoadCompleted = false;
            _isQuitSaveInProgress = false;
            _allowQuitAfterSave = false;
            _pendingQuitAfterSave = false;

            Application.wantsToQuit -= HandleWantsToQuit;
            Application.wantsToQuit += HandleWantsToQuit;

            if (_hasUnsyncedChanges)
            {
                _remoteLoadCompleted = true;
                Debug.LogWarning("Unsynced cached user data detected. Skipping Firestore load to avoid overwriting newer PlayerPrefs data.");
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

            PersistLocalSnapshot(markUnsynced: true);
        }

        private static async Task LoadRemoteUserDataAsync()
        {
            try
            {
                var cachedUser = LoadCachedUser();
                var userTask = UserAPI.GetUserAsync(TEST_UserID);
                var currencyTask = UserCurrencyAPI.GetUserAsync(TEST_UserID);
                var boosterTask = UserBoosterAPI.GetUserAsync(TEST_UserID);

                await Task.WhenAll(userTask, currencyTask, boosterTask);

                var remoteUser = await userTask;
                var remoteCurrency = await currencyTask;
                var remoteBooster = await boosterTask;

                if (_hasUnsyncedChanges)
                {
                    Debug.LogWarning("Local user data changed before Firestore load completed. Keeping runtime snapshot.");
                    return;
                }

                var hasRemoteData = remoteUser.HasValue || remoteCurrency.HasValue || remoteBooster.HasValue;
                var hasMissingRemoteSegments = !remoteUser.HasValue || !remoteCurrency.HasValue || !remoteBooster.HasValue;

                if (!hasRemoteData)
                {
                    CurUser = cachedUser ?? CreateAndCacheNewUser();
                    MarkLocalStateDirty();
                    Debug.LogWarning(
                        cachedUser != null
                            ? $"No remote data found for user '{TEST_UserID}'. Loaded cached PlayerPrefs user and will sync it back to Firestore on quit."
                            : $"No remote data or PlayerPrefs cache found for user '{TEST_UserID}'. Created a new user and cached it locally.");
                    return;
                }

                CurUser = BuildUserFromSources(remoteUser, remoteCurrency, remoteBooster, cachedUser);
                UserDataHelper.SaveUser(CurUser);

                _hasUnsyncedChanges = hasMissingRemoteSegments;
                SetPersistedUnsyncedChanges(hasMissingRemoteSegments);

                if (hasMissingRemoteSegments)
                {
                    Debug.LogWarning(
                        cachedUser != null
                            ? $"Remote data for '{TEST_UserID}' is incomplete. Missing fields were filled from PlayerPrefs and will be written back to Firestore on quit."
                            : $"Remote data for '{TEST_UserID}' is incomplete. Missing fields were filled with defaults and will be written back to Firestore on quit.");
                }

                Debug.Log($"Remote user data synced for {CurUser.Id}.");
            }
            catch (Exception ex)
            {
                var cachedUser = LoadCachedUser();
                var usedCachedUser = cachedUser != null;
                CurUser = cachedUser ?? CreateAndCacheNewUser();
                MarkLocalStateDirty();
                Debug.LogWarning($"Failed to sync remote user data from Firestore. Falling back to {(usedCachedUser ? "cached PlayerPrefs data" : "a newly created user")} for this session. {ex}");
            }
            finally
            {
                _remoteLoadCompleted = true;
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
                _pendingQuitAfterSave = true;
                return false;
            }

            if (!_hasUnsyncedChanges)
            {
                return true;
            }

            QueueFirestoreSave("quit", quitAfterSave: true);
            return false;
        }

        private static void QueueFirestoreSave(string reason, bool quitAfterSave)
        {
            if (quitAfterSave)
            {
                _pendingQuitAfterSave = true;
            }

            if (_isQuitSaveInProgress)
            {
                return;
            }

            if (!_hasUnsyncedChanges || CurUser == null)
            {
                if (quitAfterSave)
                {
                    _allowQuitAfterSave = true;
                    RequestQuit();
                }

                return;
            }

            PersistLocalSnapshot(markUnsynced: true);

            _isQuitSaveInProgress = true;
            _ = SaveToFirestoreAsync(reason);
        }

        private static async Task SaveToFirestoreAsync(string reason)
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
                SetPersistedUnsyncedChanges(false);
                Debug.Log($"Saved user data to Firestore for {CurUser.Id} during {reason}.");
            }
            catch (Exception ex)
            {
                MarkLocalStateDirty();
                Debug.LogError($"Failed to save user data to Firestore during {reason}: {ex}");
            }
            finally
            {
                _isQuitSaveInProgress = false;

                if (_pendingQuitAfterSave)
                {
                    _pendingQuitAfterSave = false;
                    _allowQuitAfterSave = true;
                    RequestQuit();
                }
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

        private static UserRuntimeData LoadCachedUser()
        {
            var cachedUser = UserDataHelper.LoadUser();
            if (cachedUser == null)
            {
                return null;
            }

            cachedUser.Id = TEST_UserID;
            EnsureBoosterKeys(cachedUser);
            return cachedUser;
        }

        private static UserRuntimeData CreateAndCacheNewUser()
        {
            var user = CreateDefaultUser();
            EnsureBoosterKeys(user);
            UserDataHelper.SaveUser(user);
            SetPersistedUnsyncedChanges(true);
            return user;
        }

        private static void EnsureLifecycleProxy()
        {
            if (_lifecycleProxy != null)
            {
                return;
            }

            var lifecycleObject = new GameObject(LifecycleObjectName);
            UnityEngine.Object.DontDestroyOnLoad(lifecycleObject);
            _lifecycleProxy = lifecycleObject.AddComponent<UserManagerLifecycleProxy>();
        }

        private static bool HasPersistedUnsyncedChanges()
        {
            return PlayerPrefs.GetInt(SessionDirtyKey, 0) == 1;
        }

        private static void SetPersistedUnsyncedChanges(bool hasUnsyncedChanges)
        {
            PlayerPrefs.SetInt(SessionDirtyKey, hasUnsyncedChanges ? 1 : 0);
            PlayerPrefs.Save();
        }

        private static void MarkLocalStateDirty()
        {
            _hasUnsyncedChanges = true;
            SetPersistedUnsyncedChanges(true);
        }

        private static void PersistLocalSnapshot(bool markUnsynced)
        {
            if (CurUser == null)
            {
                return;
            }

            CurUser.Id = TEST_UserID;
            EnsureBoosterKeys(CurUser);
            UserDataHelper.SaveUser(CurUser);

            if (markUnsynced)
            {
                MarkLocalStateDirty();
            }
        }

        private static void HandleApplicationBackgrounded(string reason)
        {
            PersistLocalSnapshot(markUnsynced: _hasUnsyncedChanges);

            if (_hasUnsyncedChanges)
            {
                QueueFirestoreSave(reason, quitAfterSave: false);
            }
        }

        private static UserRuntimeData BuildUserFromSources(
            UserModel? remoteUser,
            UserCurrencyModel? remoteCurrency,
            UserBoosterModel? remoteBooster,
            UserRuntimeData cachedUser)
        {
            var resolvedUser = cachedUser ?? CreateDefaultUser();
            resolvedUser.Id = TEST_UserID;

            if (remoteUser.HasValue)
            {
                resolvedUser.Name = remoteUser.Value.Name;
                resolvedUser.AvatarURL = remoteUser.Value.AvatarURL;
                resolvedUser.CurrentLevelIndex = Mathf.Max(1, remoteUser.Value.CurrentLevelIndex);
                resolvedUser.SetPlayedTutorials(ParseTutorials(remoteUser.Value.PlayedTutorials));
            }

            if (remoteCurrency.HasValue)
            {
                resolvedUser.CoinCount = remoteCurrency.Value.Coin;
                resolvedUser.HeartCount = remoteCurrency.Value.Heart;
            }

            if (remoteBooster.HasValue)
            {
                resolvedUser.BoosterCount = ToBoosterDictionary(remoteBooster.Value.BoostersCount);
            }

            EnsureBoosterKeys(resolvedUser);
            return resolvedUser;
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

        private sealed class UserManagerLifecycleProxy : MonoBehaviour
        {
            private void OnApplicationPause(bool pauseStatus)
            {
                if (pauseStatus)
                {
                    HandleApplicationBackgrounded("pause");
                }
            }

            private void OnApplicationFocus(bool hasFocus)
            {
                if (!hasFocus)
                {
                    HandleApplicationBackgrounded("focus-lost");
                }
            }

            private void OnApplicationQuit()
            {
                HandleApplicationBackgrounded("quit-event");
            }
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
