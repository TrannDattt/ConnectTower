using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Helpers;
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    public class LevelManager : Singleton<LevelManager>
    {
        private List<LevelRuntimeData> _levels = new();
        public static LevelRuntimeData PlayingLevel { get; private set; }

        public void SetPlayingLevel(LevelRuntimeData levelData)
        {
            PlayingLevel = new(levelData);
        }

        public LevelRuntimeData GetLatestNotClearedLevel()
        {
            return _levels.FirstOrDefault(l => !l.IsCleared);
        }

        public LevelRuntimeData GetLevel(int index)
        {
            if (index - 1 < 0 || index > _levels.Count) return null;
            return _levels[index - 1];
        }

        public List<LevelRuntimeData> GetAllLevels()
        {
            return _levels;
        }

        public int GetTotalLevelCount()
        {
            return _levels.Count;
        }

        protected override void Awake()
        {
            base.Awake();

            LevelDataHelper.LoadAllLevels(out var levelJSONs);
            _levels = levelJSONs.Select(d => new LevelRuntimeData(d)).OrderBy(so => so.Index).ToList();
        }
    }
}