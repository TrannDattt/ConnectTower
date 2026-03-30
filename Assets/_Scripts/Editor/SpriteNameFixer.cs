#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteNameFixer : EditorWindow
{
    [MenuItem("Tools/Fix Sprite Names")]
    public static void FixNames()
    {
        string path = "Assets/Sprite/Resources/Icons";
        string[] assetGuids = AssetDatabase.FindAssets("t:Sprite", new[] { path });
        int count = 0;

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            if (sprite != null && sprite.name != fileName)
            {
                sprite.name = fileName;
                EditorUtility.SetDirty(sprite);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Đã sửa tên cho {count} Sprites!");
    }
}
#endif
