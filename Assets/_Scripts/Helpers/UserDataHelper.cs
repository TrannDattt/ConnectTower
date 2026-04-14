using Assets._Scripts.Datas;
using UnityEngine;
using Newtonsoft.Json;

namespace Assets._Scripts.Helpers
{
    public static class UserDataHelper
    {
        private const string PlayerDataKey = "UserProgress";

        public static UserRuntimeData LoadUser()
        {
            var json = PlayerPrefs.GetString(PlayerDataKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return new UserRuntimeData();
            }
            return JsonConvert.DeserializeObject<UserRuntimeData>(json);
        }

        public static void SaveUser(UserRuntimeData user)
        {
            if (user == null) return;
            var json = JsonConvert.SerializeObject(user);
            PlayerPrefs.SetString(PlayerDataKey, json);
            PlayerPrefs.Save();
        }
    }
}