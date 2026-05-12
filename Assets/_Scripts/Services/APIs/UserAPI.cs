using Assets._Scripts.Services.Configs;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

namespace Assets._Scripts.Services.APIs
{
    public struct UserModel
    {
        public string UID;
        public string Name;
        public string AvatarURL;
    }

    public static class UserAPI
    {
        private static string _collectionName = "Users";
        private static CollectionReference _collectionRef => FirebaseConfig.Db.Collection(_collectionName);

        public static UserModel[] GetUsers(int amount = -1)
        {
            try
            {
                return new UserModel[] {};
            }
            catch (System.Exception)
            {   
                throw;
            }
        }

        public static bool GetUser(string uid, out UserModel res)
        {
            UserModel result = new() { UID = "", Name = "", AvatarURL = ""};
            
            if (!FirebaseConfig.IsReady)
            {
                Debug.LogWarning("UserAPI.GetUser called before Firebase was ready. Attempting to proceed, but it may fail.");
            }

            _collectionRef.Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"GetUser failed with exception: {task.Exception}");
                    return;
                }

                if (task.IsCanceled)
                {
                    Debug.LogWarning("GetUser was canceled.");
                    return;
                }

                var snapshot = task.Result;
                if (snapshot.Exists)
                {
                    Debug.Log($"Document data for {snapshot.Id} document:\n[");
                    var user = snapshot.ToDictionary();
                    foreach(var kvp in user)
                    {
                        Debug.Log($"{kvp.Key}: {kvp.Value}\n");
                    }
                    Debug.Log("]");
                    result = new UserModel {UID = snapshot.Id, Name = user["Name"] as string, AvatarURL = user["AvatarURL"] as string};
                }
                else
                    Debug.Log($"No document with id {uid} found!");
            });
            res = result;
            return !string.IsNullOrEmpty(res.UID);
        }

        public static void CreateUser(UserModel newData)
        {
            try
            {
                
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public static void UpdateUser(string uid, UserModel newData)
        {
            try
            {
                
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }

        public static void DeleteUser(string uid)
        {
            try
            {
                
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }
    }
}