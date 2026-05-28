using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts.Services.Configs
{
    public static class FirebaseConfig
    {
        private static FirebaseApp _app;
        private static FirebaseAuth _auth;
        private static FirebaseFirestore _db;
        private static Task _initializeTask;
        private static Task<bool> _anonymousSignInTask;
        public static int MainThreadId { get; private set; }

        public static FirebaseApp App
        {
            get
            {
                if (_app == null) _app = FirebaseApp.DefaultInstance;
                return _app;
            }
        }

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

        public static FirebaseUser CurrentUser => _auth?.CurrentUser;
        public static bool IsReady { get; private set; } = false;
        public static bool IsAuthenticated => CurrentUser != null;
        public static bool IsUnityMainThread => MainThreadId != 0 && Environment.CurrentManagedThreadId == MainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            MainThreadId = Environment.CurrentManagedThreadId;
            _ = InitializeAsync();
        }

        public static async Task InitializeAsync()
        {
            if (IsReady)
            {
                return;
            }

            if (_initializeTask != null)
            {
                await _initializeTask;
                return;
            }

            _initializeTask = InitializeInternalAsync();

            try
            {
                await _initializeTask;
            }
            finally
            {
                if (!IsReady)
                {
                    _initializeTask = null;
                }
            }
        }

        public static async Task EnsureReadyAsync()
        {
            await InitializeAsync();

            if (!IsReady)
            {
                throw new InvalidOperationException("Firebase Firestore is not ready.");
            }
        }

        public static async Task<bool> EnsureAnonymousAuthAsync()
        {
            await EnsureReadyAsync();

            if (CurrentUser != null)
            {
                return true;
            }

            if (_anonymousSignInTask != null)
            {
                return await _anonymousSignInTask;
            }

            _anonymousSignInTask = SignInAnonymouslyInternalAsync();

            try
            {
                return await _anonymousSignInTask;
            }
            finally
            {
                if (CurrentUser == null)
                {
                    _anonymousSignInTask = null;
                }
            }
        }

        private static async Task InitializeInternalAsync()
        {
            Debug.Log("Initializing Firebase...");

            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus != DependencyStatus.Available)
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    return;
                }

                _app = FirebaseApp.DefaultInstance;
                _auth = FirebaseAuth.DefaultInstance;
                _db = FirebaseFirestore.DefaultInstance;
                IsReady = true;
                Debug.Log("Firebase initialized. Firestore is ready.");

                // Do not block Firebase startup on auth. Anonymous auth can fail on some
                // networks/certificate chains while Firestore may still be usable.
                _ = EnsureAnonymousAuthAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Firebase initialization failed: {ex}");
                throw;
            }
        }

        private static async Task<bool> SignInAnonymouslyInternalAsync()
        {
            try
            {
                if (_auth == null)
                {
                    _auth = FirebaseAuth.DefaultInstance;
                }

                if (_auth.CurrentUser != null)
                {
                    return true;
                }

                Debug.Log("Firebase Firestore ready. Attempting anonymous sign-in...");
                await _auth.SignInAnonymouslyAsync();
                Debug.Log($"Firebase anonymous sign-in succeeded. UserId: {_auth.CurrentUser?.UserId ?? "None"}");
                return _auth.CurrentUser != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "Firebase anonymous sign-in failed. Firestore remains initialized, but auth-dependent rules may block reads/writes.\n" +
                    ex);
                return false;
            }
        }
    }
}
