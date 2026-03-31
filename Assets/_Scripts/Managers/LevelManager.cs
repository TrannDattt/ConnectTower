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
        //------------------------------
        //TODO: Load level SO from resource + data from JSON
        [SerializeField] private List<LevelSO> _levelDatas;
        //--------------------------

        private List<LevelRuntimeData> _levels = new();
        public static LevelRuntimeData PlayingLevel { get; private set; }

        public void SetPlayingLevel(LevelRuntimeData levelData)
        {
            PlayingLevel = levelData;
        }

        public LevelRuntimeData GetLatestNotClearedLevel()
        {
            return _levels.LastOrDefault(l => !l.IsCleared);
        }

        public List<LevelRuntimeData> GetAllLevel()
        {
            if (_levels.Count == 0)
                _levels = _levelDatas.Select(d => new LevelRuntimeData(d)).OrderBy(so => so.Index).ToList();
            return _levels;
        }

        public bool TryGetLevelData(int index, out LevelRuntimeData data)
        {
            data = null;
            var levelSO = _levelDatas.First(l => l.Index == index);
            if (levelSO == null) return false;
            data = new(levelSO);
            return true;
        }

        protected override void Awake()
        {
            base.Awake();

            LevelDataHelper.LoadAllLevels(out var levelJSONs);
            _levels = levelJSONs.Select(d => new LevelRuntimeData(d)).OrderBy(so => so.Index).ToList();
        }
    }
}