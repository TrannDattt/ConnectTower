using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

namespace Assets._Scripts.Helpers
{
    public static class BlockIconMapper
    {
        private const string AtlasBasePath = "Atlases";
        private const string AtlasName = "BlockIcon";
        private readonly static Dictionary<int, Sprite> IconDict = new();
        private readonly static List<Sprite> IconList = new();

        private static Task _initTask;
        public static Task InitTask => _initTask ??= InitializeCacheAsync();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void StartInitialization()
        {
            _ = InitTask;
        }

        public static async Task InitializeCacheAsync()
        {
            IconDict.Clear();
            IconList.Clear();
            
            string atlasPath = $"{AtlasBasePath}/{AtlasName}";
            ResourceRequest request = Resources.LoadAsync<SpriteAtlas>(atlasPath);

            while (!request.isDone)
            {
                await Task.Yield();
            }

            SpriteAtlas atlas = request.asset as SpriteAtlas;

            if (atlas != null)
            {
                Sprite[] loadedIcons = new Sprite[atlas.spriteCount];
                atlas.GetSprites(loadedIcons);
                
                foreach (var icon in loadedIcons)
                {
                    if (icon == null) continue;
                    string spriteName = icon.name.Replace("(Clone)", "");
                    IconList.Add(icon);
                    IconDict[spriteName.GetHashCode()] = icon;
                }
                Debug.Log($"[BlockIconMapper] Loaded {IconDict.Count} icons from SpriteAtlas: {atlasPath}");
            }
            else
            {
                Debug.LogError($"[BlockIconMapper] Could not find SpriteAtlas at: Resources/{atlasPath}");
            }
        }

        public static async Task<Sprite> GetIconAsync(string id)
        {
            await InitTask;
            return GetIcon(id);
        }

        public static Sprite GetIcon(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            int hash = id.GetHashCode();
            return IconDict.TryGetValue(hash, out var sprite) ? sprite : null;
        }

        public static async Task<List<Sprite>> GetAllIconsAsync()
        {
            await InitTask;
            return IconList;
        }

        public static List<Sprite> GetAllIcons()
        {
            return IconList;
        }
    }
}
