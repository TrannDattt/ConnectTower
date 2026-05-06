using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class BlockGroupMapper
    {

        private static string _blockGroupPath = "BlockGroups";
        private static Dictionary<int, BlockGroupSO> _groupDict = new();

        public static List<string> GetAllGroups() => _groupDict.Select(kvp => kvp.Value.Name).ToList();

        public static BlockGroupSO GetGroupData(string tag)
        {
            int hash = tag.GetHashCode();
            if (_groupDict.TryGetValue(hash, out BlockGroupSO group))
            {
                return group;
            }
            Debug.LogWarning($"[BlockGroupMapper] Group with name '{tag}' not found.");
            return null;
        }

        public static List<Sprite> GetGroupIcons(string tag)
        {
            var data = GetGroupData(tag);
            if (data == null) return null;
            return data.Icons;
        }

        public static Sprite GetIcon(string tag, string iconId)
        {
            var icons = GetGroupIcons(tag);
            if (icons == null) return null;
            return icons.FirstOrDefault(i => string.Equals(i.name, iconId));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadBlockGroups()
        {
            _groupDict.Clear();
            BlockGroupSO[] loadedGroups = Resources.LoadAll<BlockGroupSO>(_blockGroupPath);
            foreach (var group in loadedGroups)
            {
                _groupDict[group.Name.GetHashCode()] = group;
            }

            Debug.Log($"[BlockGroupMapper] Loaded {_groupDict.Count} block groups from: Resources/{_blockGroupPath}");
        }
    }
}
