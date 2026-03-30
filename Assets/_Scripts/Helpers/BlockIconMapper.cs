using System;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class BlockIconMapper
    {
        private const string BASE_PATH = "Icons";

        public static Sprite GetIcon(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var path = $"{BASE_PATH}/{id}";
            var icon = Resources.Load<Sprite>(path);
            return icon;
        }
    }
}