using System.Collections.Generic;
using Assets._Scripts.Datas;
using Newtonsoft.Json;
using SFB;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
                SaveAction saveAction = AskSaveAction(fileName);
                switch (saveAction)
                {
                    case SaveAction.SaveAsNew:
                        path = GetUniqueCopyPath(fileName);
                        break;
                    case SaveAction.Overwrite:
                        break;
                    case SaveAction.Cancel:
                        Debug.Log($"Save level canceled: {fileName}");
                        return;
                }
            }

            System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);
            Debug.Log($"Level {levelData.Index} saved to {path}");
        }

        private enum SaveAction
        {
            SaveAsNew,
            Overwrite,
            Cancel
        }

        private static string GetUniqueCopyPath(string fileName)
        {
            int copyCount = 1;
            string newPath;
            do
            {
                newPath = Application.dataPath + $"{FOLDER_PATH}/{fileName}({copyCount}).json";
                copyCount++;
            } while (System.IO.File.Exists(newPath));

            return newPath;
        }

        private static SaveAction AskSaveAction(string fileName)
        {
#if UNITY_EDITOR
            int option = EditorUtility.DisplayDialogComplex(
                "Level File Already Exists",
                $"{fileName}.json already exists. Choose how to save:",
                "Save As New",
                "Cancel",
                "Overwrite");

            return option switch
            {
                0 => SaveAction.SaveAsNew,
                2 => SaveAction.Overwrite,
                _ => SaveAction.Cancel
            };
#else
            return SaveAction.Overwrite;
#endif
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
