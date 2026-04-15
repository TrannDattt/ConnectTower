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
                Debug.Log("Create new user.");
                return new UserRuntimeData();
            }
            Debug.Log("Loading JSON: " + json);
            return JsonConvert.DeserializeObject<UserRuntimeData>(json);
        }

        public static void SaveUser(UserRuntimeData user)
        {
            if (user == null) return;
            var json = JsonConvert.SerializeObject(user, Formatting.Indented);
            Debug.Log("Saving JSON: " + json);
            PlayerPrefs.SetString(PlayerDataKey, json);
            PlayerPrefs.Save();
        }
    }
}