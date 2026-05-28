using Assets._Scripts.Enums;
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
    public struct BoosterModel
    {
        public EBooster Type;
        public int Count;
    }

    public struct UserBoosterModel
    {
        public string UID;
        public BoosterModel[] BoostersCount;
    }

    public static class UserBoosterAPI
    {
        private const string UidField = "UID";
        private const string BoosterCountField = "BoosterCount";
        private const string TypeField = "Type";
        private const string AmountField = "Amount";
        private static readonly string _collectionName = "UserBoosters";
        private static CollectionReference _collectionRef => FirebaseConfig.Db.Collection(_collectionName);

        public static UserBoosterModel[] GetUsers(int amount = -1)
        {
            if (FirebaseConfig.IsUnityMainThread)
            {
                Debug.LogError("UserBoosterAPI.GetUsers cannot run synchronously on the Unity main thread. Use GetUsersAsync instead.");
                return Array.Empty<UserBoosterModel>();
            }

            try
            {
                return GetUsersAsync(amount).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Debug.LogError($"UserBoosterAPI.GetUsers failed: {ex}");
                return Array.Empty<UserBoosterModel>();
            }
        }

        public static bool GetUser(string uid, out UserBoosterModel res)
        {
            if (FirebaseConfig.IsUnityMainThread)
            {
                Debug.LogError("UserBoosterAPI.GetUser cannot run synchronously on the Unity main thread. Use GetUserAsync instead.");
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
                Debug.LogError($"UserBoosterAPI.GetUser failed: {ex}");
            }

            res = default;
            return false;
        }

        public static async Task<UserBoosterModel[]> GetUsersAsync(int amount = -1)
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
                           .Select(MapUserBooster)
                           .ToArray();
        }

        public static async Task<UserBoosterModel?> GetUserAsync(string uid)
        {
            var resolvedUid = ResolveUid(uid, requireExisting: true);
            await FirebaseConfig.EnsureReadyAsync();

            var snapshot = await FindDocumentByUidAsync(resolvedUid);
            if (snapshot == null || !snapshot.Exists)
            {
                return null;
            }

            return MapUserBooster(snapshot);
        }

        public static async Task<bool> UserExistsAsync(string uid)
        {
            var user = await GetUserAsync(uid);
            return user.HasValue;
        }

        public static void CreateUser(UserBoosterModel newData)
        {
            _ = CreateUserAsync(newData).ContinueWithOnMainThread(LogTaskFailure(nameof(CreateUserAsync)));
        }

        public static void UpdateUser(string uid, UserBoosterModel newData)
        {
            _ = UpdateUserAsync(uid, newData).ContinueWithOnMainThread(LogTaskFailure(nameof(UpdateUserAsync)));
        }

        public static void DeleteUser(string uid)
        {
            _ = DeleteUserAsync(uid).ContinueWithOnMainThread(LogTaskFailure(nameof(DeleteUserAsync)));
        }

        public static async Task CreateUserAsync(UserBoosterModel newData)
        {
            await FirebaseConfig.EnsureReadyAsync();

            var uid = ResolveUid(newData.UID, requireExisting: false);
            var documentRef = await ResolveDocumentReferenceAsync(uid, requireExisting: false);
            await documentRef.SetAsync(ToDictionary(uid, newData));
        }

        public static async Task SetUserAsync(UserBoosterModel newData)
        {
            await CreateUserAsync(newData);
        }

        public static async Task UpdateUserAsync(string uid, UserBoosterModel newData)
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

        private static UserBoosterModel MapUserBooster(DocumentSnapshot snapshot)
        {
            var data = snapshot.ToDictionary();
            return new UserBoosterModel
            {
                UID = ReadString(data, UidField),
                BoostersCount = ReadBoosters(data),
            };
        }

        private static Dictionary<string, object> ToDictionary(string uid, UserBoosterModel user)
        {
            return new Dictionary<string, object>
            {
                [UidField] = uid,
                [BoosterCountField] = ToBoosterPayload(user.BoostersCount),
            };
        }

        private static object[] ToBoosterPayload(BoosterModel[] boosters)
        {
            if (boosters == null || boosters.Length == 0)
            {
                return Array.Empty<object>();
            }

            return boosters
                .Where(booster => booster.Type != EBooster.None)
                .Select(booster => (object)new Dictionary<string, object>
                {
                    [TypeField] = booster.Type.ToString(),
                    [AmountField] = Mathf.Max(0, booster.Count),
                })
                .ToArray();
        }

        private static BoosterModel[] ReadBoosters(IReadOnlyDictionary<string, object> data)
        {
            if (!data.TryGetValue(BoosterCountField, out var rawValue) || rawValue is not IEnumerable<object> rawBoosters)
            {
                return Array.Empty<BoosterModel>();
            }

            var result = new List<BoosterModel>();
            foreach (var rawBooster in rawBoosters)
            {
                if (rawBooster is not IReadOnlyDictionary<string, object> boosterData)
                {
                    continue;
                }

                var typeName = ReadString(boosterData, TypeField);
                if (!Enum.TryParse(typeName, true, out EBooster boosterType) || boosterType == EBooster.None)
                {
                    continue;
                }

                result.Add(new BoosterModel
                {
                    Type = boosterType,
                    Count = ReadInt(boosterData, AmountField),
                });
            }

            return result.ToArray();
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
                throw new InvalidOperationException($"No user booster document found for UID '{uid}'.");
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
