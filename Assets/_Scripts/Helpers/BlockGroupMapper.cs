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

        public static List<Sprite> GetGroupIcons(string name)
        {
            int hash = name.GetHashCode();
            if (_groupDict.TryGetValue(hash, out BlockGroupSO group))
            {
                return group.Icons;
            }
            Debug.LogWarning($"[BlockGroupMapper] Group with name '{name}' not found.");
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadBlockGroups()
        {
            _groupDict.Clear();
            BlockGroupSO[] loadedGroups = Resources.LoadAll<BlockGroupSO>(_blockGroupPath);
            foreach (var group in loadedGroups)
            {
                _groupDict.Add(group.Name.GetHashCode(), group);
            }
            Debug.Log($"[BlockGroupMapper] Loaded {_groupDict.Count} block groups from: Resources/{_blockGroupPath}");
        }
    }
}
