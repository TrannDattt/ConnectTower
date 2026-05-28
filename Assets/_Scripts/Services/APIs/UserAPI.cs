using Assets._Scripts.Services.Configs;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts.Services.APIs
{
    public struct UserModel
    {
        public string UID;
        public string Name;
        public string AvatarURL;
        public int CurrentLevelIndex;
        public string[] PlayedTutorials;
    }

    public static class UserAPI
    {
        private const string NameField = "Name";
        private const string AvatarUrlField = "AvatarURL";
        private const string CurrentLevelIndexField = "CurrentLevelIndex";
        private const string PlayedTutorialsField = "PlayedTutorials";
        private static readonly string _collectionName = "Users";
        private static CollectionReference _collectionRef => FirebaseConfig.Db.Collection(_collectionName);

        public static UserModel[] GetUsers(int amount = -1)
        {
            if (FirebaseConfig.IsUnityMainThread)
            {
                Debug.LogError("UserAPI.GetUsers cannot run synchronously on the Unity main thread. Use GetUsersAsync instead.");
                return Array.Empty<UserModel>();
            }

            try
            {
                return GetUsersAsync(amount).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetUsers failed: {ex}");
                return Array.Empty<UserModel>();
            }
        }

        public static bool GetUser(string uid, out UserModel res)
        {
            if (FirebaseConfig.IsUnityMainThread)
            {
                Debug.LogError("UserAPI.GetUser cannot run synchronously on the Unity main thread. Use GetUserAsync instead.");
                res = default;
                return false;
            }

            try
            {
                var user = GetUserAsync(uid).GetAwaiter().GetResult();
                if (user.HasValue)
                {
                    res = user.Value;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetUser failed: {ex}");
            }

            res = default;
            return false;
        }

        public static async Task<UserModel[]> GetUsersAsync(int amount = -1)
        {
            await FirebaseConfig.EnsureReadyAsync();

            Query query = _collectionRef;
            if (amount > 0)
            {
                query = query.Limit(amount);
            }

            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents
                           .Where(document => document.Exists)
                           .Select(MapUser)
                           .ToArray();
        }

        public static async Task<UserModel[]> GetTopUsersByCurrentLevelAsync(int amount)
        {
            if (amount <= 0)
            {
                return Array.Empty<UserModel>();
            }

            await FirebaseConfig.EnsureReadyAsync();

            var snapshot = await _collectionRef
                .OrderByDescending(CurrentLevelIndexField)
                .Limit(amount)
                .GetSnapshotAsync();

            return snapshot.Documents
                           .Where(document => document.Exists)
                           .Select(MapUser)
                           .OrderByDescending(user => user.CurrentLevelIndex)
                           .ThenBy(user => user.Name)
                           .ToArray();
        }

        public static async Task<UserModel?> GetUserAsync(string uid)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new ArgumentException("UID cannot be null or empty.", nameof(uid));
            }

            await FirebaseConfig.EnsureReadyAsync();

            var snapshot = await _collectionRef.Document(uid).GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                return null;
            }

            return MapUser(snapshot);
        }

        public static async Task<bool> UserExistsAsync(string uid)
        {
            var user = await GetUserAsync(uid);
            return user.HasValue;
        }

        public static void CreateUser(UserModel newData)
        {
            _ = CreateUserAsync(newData).ContinueWithOnMainThread(LogTaskFailure(nameof(CreateUserAsync)));
        }

        public static void UpdateUser(string uid, UserModel newData)
        {
            _ = UpdateUserAsync(uid, newData).ContinueWithOnMainThread(LogTaskFailure(nameof(UpdateUserAsync)));
        }

        public static void DeleteUser(string uid)
        {
            _ = DeleteUserAsync(uid).ContinueWithOnMainThread(LogTaskFailure(nameof(DeleteUserAsync)));
        }

        public static async Task CreateUserAsync(UserModel newData)
        {
            await FirebaseConfig.EnsureReadyAsync();

            var uid = ResolveUid(newData.UID, requireExisting: false);
            var payload = ToDictionary(newData);
            await _collectionRef.Document(uid).SetAsync(payload);
        }

        public static async Task SetUserAsync(UserModel newData)
        {
            await CreateUserAsync(newData);
        }

        public static async Task UpdateUserAsync(string uid, UserModel newData)
        {
            await FirebaseConfig.EnsureReadyAsync();

            var resolvedUid = ResolveUid(uid, requireExisting: true);
            var payload = ToDictionary(newData);
            await _collectionRef.Document(resolvedUid).SetAsync(payload, SetOptions.MergeAll);
        }

        public static async Task DeleteUserAsync(string uid)
        {
            await FirebaseConfig.EnsureReadyAsync();

            var resolvedUid = ResolveUid(uid, requireExisting: true);
            await _collectionRef.Document(resolvedUid).DeleteAsync();
        }

        private static UserModel MapUser(DocumentSnapshot snapshot)
        {
            var data = snapshot.ToDictionary();
            return new UserModel
            {
                UID = snapshot.Id,
                Name = ReadString(data, NameField),
                AvatarURL = ReadString(data, AvatarUrlField),
                CurrentLevelIndex = ReadInt(data, CurrentLevelIndexField, 1),
                PlayedTutorials = ReadStringArray(data, PlayedTutorialsField),
            };
        }

        private static Dictionary<string, object> ToDictionary(UserModel user)
        {
            return new Dictionary<string, object>
            {
                [NameField] = user.Name ?? string.Empty,
                [AvatarUrlField] = user.AvatarURL ?? string.Empty,
                [CurrentLevelIndexField] = Mathf.Max(1, user.CurrentLevelIndex),
                [PlayedTutorialsField] = user.PlayedTutorials ?? Array.Empty<string>(),
            };
        }

        private static string ReadString(IReadOnlyDictionary<string, object> data, string key)
        {
            if (!data.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return string.Empty;
            }

            return rawValue.ToString() ?? string.Empty;
        }

        private static int ReadInt(IReadOnlyDictionary<string, object> data, string key, int fallback = 0)
        {
            if (!data.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return fallback;
            }

            return Convert.ToInt32(rawValue);
        }

        private static string[] ReadStringArray(IReadOnlyDictionary<string, object> data, string key)
        {
            if (!data.TryGetValue(key, out var rawValue) || rawValue is not IEnumerable<object> rawItems)
            {
                return Array.Empty<string>();
            }

            return rawItems
                .Where(item => item != null)
                .Select(item => item.ToString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray();
        }

        private static string ResolveUid(string uid, bool requireExisting)
        {
            if (!string.IsNullOrWhiteSpace(uid))
            {
                return uid;
            }

            if (!requireExisting && !string.IsNullOrWhiteSpace(FirebaseConfig.CurrentUser?.UserId))
            {
                return FirebaseConfig.CurrentUser.UserId;
            }

            throw new ArgumentException("UID cannot be null or empty.", nameof(uid));
        }

        private static Action<Task> LogTaskFailure(string operationName)
        {
            return task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"{operationName} failed: {task.Exception}");
                }
                else if (task.IsCanceled)
                {
                    Debug.LogWarning($"{operationName} was canceled.");
                }
            };
        }
    }
}
