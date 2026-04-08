using System;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class UserLifeHelper
    {
        public const int MAX_LIFE = 5;
        public const float RECOVERTY_TIME = 480;

        private static string _lostLifeTime = string.Empty;
        private static string _savedPath = "LostLifeTime";

        public static float TimeToRecorver
        {
            get
            {
                if (string.IsNullOrEmpty(_lostLifeTime) || !long.TryParse(_lostLifeTime, out long binary)) return 0;
                var lostTime = DateTime.FromBinary(binary);
                var elapsed = (float)(DateTime.UtcNow - lostTime).TotalSeconds;
                return Mathf.Max(0, RECOVERTY_TIME - elapsed);
            }
        }

        public static void OnLostLife()
        {
            if (UserManager.CurUser.HeartCount == 4) _lostLifeTime = DateTime.UtcNow.ToBinary().ToString();
        }

        public static void OnRecovered()
        {
            if (UserManager.CurUser.HeartCount < MAX_LIFE)
            {
                _lostLifeTime = DateTime.UtcNow.ToBinary().ToString();
            }
            else
            {
                _lostLifeTime = string.Empty;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            _lostLifeTime = PlayerPrefs.GetString(_savedPath, string.Empty);
            Application.quitting += SaveLostLifeTime;
        }

        public static void SaveLostLifeTime()
        {
            PlayerPrefs.SetString(_savedPath, _lostLifeTime);
            PlayerPrefs.Save();
        }
    }
}