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
    public struct UserCurrencyModel
    {
        public string UID;
        public int Coin;
        public int Heart;
    }

    public static class UserCurrencyAPI
    {
        private const string UidField = "UID";
        private const string CoinCountField = "CoinCount";
        private const string HeartCountField = "HeartCount";
        private static readonly string _collectionName = "UserCurrencies";
        private static CollectionReference _collectionRef => FirebaseConfig.Db.Collection(_collectionName);

        public static UserCurrencyModel[] GetUsers(int amount = -1)
        {
            if (FirebaseConfig.IsUnityMainThread)
            {
                Debug.LogError("UserCurrencyAPI.GetUsers cannot run synchronously on the Unity main thread. Use GetUsersAsync instead.");
                return Array.Empty<UserCurrencyModel>();
            }

            try
            {
                return GetUsersAsync(amount).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Debug.LogError($"UserCurrencyAPI.GetUsers failed: {ex}");
                return Array.Empty<UserCurrencyModel>();
            }
        }

        public static bool GetUser(string uid, out UserCurrencyModel res)
        {
            if (FirebaseConfig.IsUnityMainThread)
            {
                Debug.LogError("UserCurrencyAPI.GetUser cannot run synchronously on the Unity main thread. Use GetUserAsync instead.");
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
                Debug.LogError($"UserCurrencyAPI.GetUser failed: {ex}");
            }

            res = default;
            return false;
        }

        public static async Task<UserCurrencyModel[]> GetUsersAsync(int amount = -1)
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
                           .Select(MapUserCurrency)
                           .ToArray();
        }

        public static async Task<UserCurrencyModel?> GetUserAsync(string uid)
        {
            var resolvedUid = ResolveUid(uid, requireExisting: true);
            await FirebaseConfig.EnsureReadyAsync();

            var snapshot = await FindDocumentByUidAsync(resolvedUid);
            if (snapshot == null || !snapshot.Exists)
            {
                return null;
            }

            return MapUserCurrency(snapshot);
        }

        public static async Task<bool> UserExistsAsync(string uid)
        {
            var user = await GetUserAsync(uid);
            return user.HasValue;
        }

        public static void CreateUser(UserCurrencyModel newData)
        {
            _ = CreateUserAsync(newData).ContinueWithOnMainThread(LogTaskFailure(nameof(CreateUserAsync)));
        }

        public static void UpdateUser(string uid, UserCurrencyModel newData)
        {
            _ = UpdateUserAsync(uid, newData).ContinueWithOnMainThread(LogTaskFailure(nameof(UpdateUserAsync)));
        }

        public static void DeleteUser(string uid)
        {
            _ = DeleteUserAsync(uid).ContinueWithOnMainThread(LogTaskFailure(nameof(DeleteUserAsync)));
        }

        public static async Task CreateUserAsync(UserCurrencyModel newData)
        {
            await FirebaseConfig.EnsureReadyAsync();

            var uid = ResolveUid(newData.UID, requireExisting: false);
            var documentRef = await ResolveDocumentReferenceAsync(uid, requireExisting: false);
            await documentRef.SetAsync(ToDictionary(uid, newData));
        }

        public static async Task SetUserAsync(UserCurrencyModel newData)
        {
            await CreateUserAsync(newData);
        }

        public static async Task UpdateUserAsync(string uid, UserCurrencyModel newData)
        {
            await FirebaseConfig.EnsureReadyAsync();

            var resolvedUid = ResolveUid(uid, requireExisting: true);
            var documentRef = await ResolveDocumentReferenceAsync(resolvedUid, requireExisting: true);
            await documentRef.SetAsync(ToDictionary(resolvedUid, newData), SetOptions.MergeAll);
        }

        public static async Task DeleteUserAsync(string uid)
        {
            await FirebaseConfig.EnsureReadyAsync();

            var resolvedUid = ResolveUid(uid, requireExisting: true);
            var documentRef = await ResolveDocumentReferenceAsync(resolvedUid, requireExisting: true);
            await documentRef.DeleteAsync();
        }

        private static UserCurrencyModel MapUserCurrency(DocumentSnapshot snapshot)
        {
            var data = snapshot.ToDictionary();
            return new UserCurrencyModel
            {
                UID = ReadString(data, UidField),
                Coin = ReadInt(data, CoinCountField),
                Heart = ReadInt(data, HeartCountField),
            };
        }

        private static Dictionary<string, object> ToDictionary(string uid, UserCurrencyModel user)
        {
            return new Dictionary<string, object>
            {
                [UidField] = uid,
                [CoinCountField] = Mathf.Max(0, user.Coin),
                [HeartCountField] = Mathf.Max(0, user.Heart),
            };
        }

        private static async Task<DocumentSnapshot> FindDocumentByUidAsync(string uid)
        {
            var querySnapshot = await _collectionRef.WhereEqualTo(UidField, uid).Limit(1).GetSnapshotAsync();
            return querySnapshot.Documents.FirstOrDefault();
        }

        private static async Task<DocumentReference> ResolveDocumentReferenceAsync(string uid, bool requireExisting)
        {
            var snapshot = await FindDocumentByUidAsync(uid);
            if (snapshot != null && snapshot.Exists)
            {
                return snapshot.Reference;
            }

            if (requireExisting)
            {
                throw new InvalidOperationException($"No user currency document found for UID '{uid}'.");
            }

            return _collectionRef.Document();
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

        private static string ReadString(IReadOnlyDictionary<string, object> data, string key)
        {
            if (!data.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return string.Empty;
            }

            return rawValue.ToString() ?? string.Empty;
        }

        private static int ReadInt(IReadOnlyDictionary<string, object> data, string key)
        {
            if (!data.TryGetValue(key, out var rawValue) || rawValue == null)
            {
                return 0;
            }

            return Convert.ToInt32(rawValue);
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
