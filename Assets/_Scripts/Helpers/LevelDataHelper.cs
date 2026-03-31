using System.Collections.Generic;
using Assets._Scripts.Datas;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class LevelDataHelper
    {
        public static bool TryLoadLevel(int levelIndex, out LevelJSON levelData)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/Level_{levelIndex}");
            if (jsonFile == null)
            {
                levelData = null;
                return false;
            }
            levelData = JsonConvert.DeserializeObject<LevelJSON>(jsonFile.text);
            return true;
        }

        public static bool LoadAllLevels(out List<LevelJSON> levelDatas)
        {
            levelDatas = new List<LevelJSON>();
            TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Levels");
            if (jsonFiles.Length == 0) return false;
            foreach (var jsonFile in jsonFiles)
            {
                LevelJSON levelData = JsonConvert.DeserializeObject<LevelJSON>(jsonFile.text);
                levelDatas.Add(levelData);
            }
            return true;
        }

        public static void SaveLevel(LevelJSON levelData)
        {
            string json = JsonConvert.SerializeObject(levelData, Formatting.Indented);
            string path = Application.dataPath + $"/GameDatas/Resources/Levels/Level_{levelData.Index}.json";
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            System.IO.File.WriteAllText(path, json);
        }

#if UNITY_EDITOR
        public static LevelJSON OpenLevelFileDialog()
        {
            string folderPath = Application.dataPath + "/GameDatas/Resources/Levels";
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select Level JSON", folderPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = System.IO.File.ReadAllText(path);
                return JsonConvert.DeserializeObject<LevelJSON>(json);
            }
            return null;
        }
#endif
    }
}