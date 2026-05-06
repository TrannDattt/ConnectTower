#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets._Scripts.Datas;
using UnityEditor;
using UnityEngine;

namespace Assets._Scripts.Editor
{
    public class BlockGroupSyncUtility : EditorWindow
    {
        [MenuItem("Tools/Sync Block Groups")]
        public static void SyncBlockGroupsFromIconFolders()
        {
            const string iconRootPath = "Assets/Sprite/Resources/Icons";
            const string groupAssetRootPath = "Assets/GameDatas/Resources/BlockGroups";

            if (!AssetDatabase.IsValidFolder(iconRootPath))
            {
                Debug.LogWarning($"[BlockGroupSyncUtility] Icon root folder not found: {iconRootPath}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(groupAssetRootPath))
            {
                Directory.CreateDirectory(groupAssetRootPath);
                AssetDatabase.Refresh();
            }

            string[] subFolderGuids = AssetDatabase.FindAssets("t:Folder", new[] { iconRootPath });
            foreach (string guid in subFolderGuids)
            {
                string folderPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(folderPath, iconRootPath))
                {
                    continue;
                }

                string groupName = Path.GetFileName(folderPath);
                string assetPath = $"{groupAssetRootPath}/{groupName}.asset";

                BlockGroupSO groupAsset = AssetDatabase.LoadAssetAtPath<BlockGroupSO>(assetPath);
                if (groupAsset == null)
                {
                    groupAsset = ScriptableObject.CreateInstance<BlockGroupSO>();
                    groupAsset.name = groupName;
                    AssetDatabase.CreateAsset(groupAsset, assetPath);
                }

                List<Sprite> sprites = AssetDatabase
                    .FindAssets("t:Sprite", new[] { folderPath })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Distinct()
                    .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                    .Where(sprite => sprite != null)
                    .OrderBy(sprite => sprite.name)
                    .ToList();

                groupAsset.Icons = sprites;
                EditorUtility.SetDirty(groupAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BlockGroupSyncUtility] Sync block groups from icon folders completed.");
        }
    }
}
#endif
