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

        public static void UpdateCounterOnLostLife()
        {
            if (UserManager.CurUser.HeartCount == MAX_LIFE - 1) 
                _lostLifeTime = DateTime.UtcNow.ToBinary().ToString();
        }

        public static void UpdateCounterOnRecovered()
        {
            if (UserManager.CurUser.HeartCount < MAX_LIFE)
            {
                // Thay vì đặt lại UtcNow, ta cộng thêm RECOVERTY_TIME vào mốc cũ để chính xác hơn
                if (!string.IsNullOrEmpty(_lostLifeTime) && long.TryParse(_lostLifeTime, out long binary))
                {
                    var lastTime = DateTime.FromBinary(binary);
                    _lostLifeTime = lastTime.AddSeconds(RECOVERTY_TIME).ToBinary().ToString();
                }
                else
                {
                    _lostLifeTime = DateTime.UtcNow.ToBinary().ToString();
                }
            }
            else
            {
                _lostLifeTime = string.Empty;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            _lostLifeTime = PlayerPrefs.GetString(_savedPath, string.Empty);
            Application.quitting += SaveLostLifeTime;
            
            CheckOfflineRecovery();
        }

        private static void CheckOfflineRecovery()
        {
            if (UserManager.CurUser == null) return;
            if (UserManager.CurUser.HeartCount >= MAX_LIFE) return;
            if (string.IsNullOrEmpty(_lostLifeTime) || !long.TryParse(_lostLifeTime, out long binary)) return;

            var lastTime = DateTime.FromBinary(binary);
            var now = DateTime.UtcNow;
            var elapsed = (float)(now - lastTime).TotalSeconds;

            int recovered = (int)(elapsed / RECOVERTY_TIME);
            if (recovered > 0)
            {
                for (int i = 0; i < recovered; i++)
                {
                    if (UserManager.CurUser.HeartCount >= MAX_LIFE) break;
                    // RecoverHeart sẽ tự gọi OnRecovered và cập nhật _lostLifeTime chuẩn xác
                    UserManager.RecoverHeart();
                }
            }
        }

        public static void SaveLostLifeTime()
        {
            PlayerPrefs.SetString(_savedPath, _lostLifeTime);
            PlayerPrefs.Save();
        }
    }
}