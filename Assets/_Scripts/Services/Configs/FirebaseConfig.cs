using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts.Services.Configs
{
    public static class FirebaseConfig
    {
        private static FirebaseAuth _auth;
        private static FirebaseFirestore _db;

        public static FirebaseAuth Auth 
        { 
            get 
            {
                if (_auth == null) _auth = FirebaseAuth.DefaultInstance;
                return _auth;
            }
        }

        public static FirebaseFirestore Db 
        { 
            get 
            {
                if (_db == null) _db = FirebaseFirestore.DefaultInstance;
                return _db;
            }
        }

        public static bool IsReady { get; private set; } = false;

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static async Task InitializeAsync()
        {
            if (IsReady) return;

            Debug.Log("Initializing Firebase...");
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Accessing these initializes the internal App instance if not already done
                _auth = FirebaseAuth.DefaultInstance;
                _db = FirebaseFirestore.DefaultInstance;
                
                Debug.Log("Firebase dependencies OK. Signing in anonymously...");
                await _auth.SignInAnonymouslyAsync();

                IsReady = true;
                Debug.Log("Firebase Initialized & Signed In Anonymously");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        }
    }
}