using System.Collections.Generic;
using Assets._Scripts.Datas;
using Newtonsoft.Json;
using SFB;
using UnityEngine;

namespace Assets._Scripts.Helpers
{
    public static class LevelDataHelper
    {
        private const string FOLDER_PATH = "/GameDatas/Resources/Levels";

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
            string fileName = $"Level_{levelData.Index}";
            string path = Application.dataPath + $"{FOLDER_PATH}/{fileName}.json";
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            if (System.IO.File.Exists(path))
            {
                int copyCount = 1;
                string newPath;
                do
                {
                    newPath = Application.dataPath + $"{FOLDER_PATH}/{fileName}({copyCount}).json";
                    copyCount++;
                } while (System.IO.File.Exists(newPath));
                path = newPath;
            }
            System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);
            Debug.Log($"Level {levelData.Index} saved to {path}");
        }

        public static LevelJSON OpenLevelFileDialog()
        {
            string folderPath = (Application.dataPath + FOLDER_PATH).Replace("/", "\\");
            var filter = new[] { new ExtensionFilter("JSON files", "json") };

            string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Level JSON", folderPath, filter, false);
            if (paths != null && paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                string json = System.IO.File.ReadAllText(paths[0]);
                return JsonConvert.DeserializeObject<LevelJSON>(json);
            }
            return null;
        }
    }
}