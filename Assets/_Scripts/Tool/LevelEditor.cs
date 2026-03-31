using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Tools
{
    public static class LevelEditor
    {
        private static LevelJSON _levelData = new();

        public static List<BlockData> BlockDatas => _levelData.BlockGroups.SelectMany(bg => bg.BlockDatas).ToList();
        public static List<string> GroupTags => _levelData.BlockGroups.Select(bg => bg.Tag).ToList();
        public static List<PillarData> PillarDatas => _levelData.PillarDatas;

        public static void ChangeLevelIndex(int index)
        {
            _levelData.Index = index;
        }

        public static void ChangeMoveLimit(int moveLimit)
        {
            _levelData.MoveLimit = moveLimit;
        }

        public static void ChangeDifficulty(EDifficulty difficulty)
        {
            _levelData.Difficulty = difficulty;
        }

        public static void AddBlockGroup(BlockGroup blockGroup)
        {
            if (_levelData == null) return;
            
            var firstBlockId = blockGroup.Blocks[0].Id;
            if (_levelData.BlockGroups.Any(bg => bg.BlockDatas.Any(b => b.Id == firstBlockId)))
            {
                return;
            }

            _levelData.BlockGroups.Add(new Datas.BlockGroup
            {
                Tag = blockGroup.GroupTag,
                BlockDatas = blockGroup.Blocks
            });
            // Debug.Log($"Added block group with tag: {blockGroup.GroupTag} with blocks: {string.Join(", ", blockGroup.Blocks.Select(b => b.Id))}");
        }

        public static void RemoveBlockGroup(BlockGroup blockGroup)
        {
            var firstBlockId = blockGroup.Blocks[0].Id;
            var toRemove = _levelData.BlockGroups.FirstOrDefault(bg => bg.BlockDatas.Any(b => b.Id == firstBlockId));
            if (toRemove != null) _levelData.BlockGroups.Remove(toRemove);
        }

        public static void UpdateBlockGroupData(BlockGroup blockGroup, Datas.BlockGroup newData)
        {
            bool tagExists = _levelData.BlockGroups.Any(bg => bg.Tag == newData.Tag && bg.BlockDatas[0].Id != blockGroup.Blocks[0].Id);
            if (tagExists)
            {
                Debug.Log($"Tag {newData.Tag} already exists in another block group. Please choose a different tag.");
                return;
            }

            var firstBlockId = blockGroup.Blocks[0].Id;
            var toUpdate = _levelData.BlockGroups.FirstOrDefault(bg => bg.BlockDatas.Any(b => b.Id == firstBlockId));
                
            if (toUpdate != null)
            {
                toUpdate.Tag = newData.Tag;
                toUpdate.BlockDatas = newData.BlockDatas;
            }
        }

        public static void UpdateBlockData(int blockId, BlockData newData)
        {
            foreach (var blockGroup in _levelData.BlockGroups)
            {
                var block = blockGroup.BlockDatas.FirstOrDefault(b => b.Id == blockId);
                if (block != null)
                {
                    block.Id = newData.Id;
                    block.IconId = newData.IconId;
                    break;
                }
            }
        }

        public static void AddPillar(PillarData pillar)
        {
            if (!PillarDatas.Contains(pillar))
            {
                PillarDatas.Add(pillar);
            }
        }

        public static void RemovePillar(int pillarId)
        {
            var pillar = PillarDatas.FirstOrDefault(p => p.Id == pillarId);
            if (pillar != null)
            {
                _levelData.PillarDatas.Remove(pillar);
            }
        }

        public static void UpdatePillarData(int pillarId, PillarData newData)
        {
            var pillar = PillarDatas.FirstOrDefault(p => p.Id == pillarId);
            if (pillar != null)
            {
                pillar.BlockIds = newData.BlockIds;
            }
        }

        public static bool TryAddMechanic(int id, MechanicRuntimeData mechanicData)
        {
            switch (mechanicData.Key)
            {
                case EMechanic.HiddenBlock:
                    _levelData.HiddenBlockDatas.BlockIds.Add(id);
                    return true;
                case EMechanic.CoveredPillar:
                    var coveredPillarData = mechanicData as CoveredPillarMechanic;
                    var sameId = _levelData.CoveredPillarDatas.FirstOrDefault(cpd => cpd.PillarIds.Contains(id));
                    if (sameId != null) return false;
                    var sameTag = _levelData.CoveredPillarDatas.FirstOrDefault(cpd => cpd.TagToOpen == coveredPillarData.TagToOpen);
                    if (sameTag != null)
                    {
                        if (sameTag.PillarIds.Contains(id)) return false;
                        sameTag.PillarIds.Add(id);
                    }
                    else
                    {
                        _levelData.CoveredPillarDatas.Add(new CoveredPillarData
                        {
                            TagToOpen = coveredPillarData.TagToOpen,
                            PillarIds = new() { id }
                        });
                    }
                    return true;
                case EMechanic.FrozenBlock:
                    var frozenBlockData = mechanicData as FrozenBlockMechanic;
                    var existingFB = _levelData.FrozenBlockDatas.FirstOrDefault(fbd => fbd.BlockIds.Contains(id));
                    if (existingFB != null)
                    {
                        if (existingFB.BlockIds.Contains(id)) return false;
                        existingFB.BlockIds.Add(id);
                    }
                    else
                    {
                        _levelData.FrozenBlockDatas.Add(new FrozenBlockData
                        {
                            MoveCountToRemove = frozenBlockData.MoveCountToRemove,
                            BlockIds = new() { id }
                        });
                    }
                    return true;
            }
            return false;
        }

        public static void RemoveMechanic(int id, EMechanic mechanicType)
        {
            switch (mechanicType)
            {
                case EMechanic.HiddenBlock:
                    _levelData.HiddenBlockDatas.BlockIds.Remove(id);
                    break;
                case EMechanic.CoveredPillar:
                    var toRemove = _levelData.CoveredPillarDatas.FirstOrDefault(cpd => cpd.PillarIds.Contains(id));
                    if (toRemove != null)
                    {
                        _levelData.CoveredPillarDatas.Remove(toRemove);
                    }
                    break;
                case EMechanic.FrozenBlock:
                    var frozenToRemove = _levelData.FrozenBlockDatas.FirstOrDefault(fbd => fbd.BlockIds.Contains(id));
                    if (frozenToRemove != null)
                    {
                        _levelData.FrozenBlockDatas.Remove(frozenToRemove);
                    }
                    break;
            }
        }

        public static void ChangeCoinReward(int coinReward)
        {
            _levelData.CoinReward = coinReward;
        }

        public static UnityEvent OnLevelCleared = new();
        public static void ClearLevel()
        {
            _levelData = new LevelJSON();
            OnLevelCleared?.Invoke();
        }

        public static UnityEvent<LevelJSON> OnLevelLoaded = new();
        public static void LoadLevel()
        {
#if UNITY_EDITOR
            var levelData = LevelDataHelper.OpenLevelFileDialog();
            if (levelData != null)
            {
                _levelData = levelData;
                OnLevelCleared?.Invoke();
                OnLevelLoaded?.Invoke(_levelData);
            }
#endif
        }

        public static void SaveLevel()
        {
            LevelDataHelper.SaveLevel(_levelData);
        }

        private static void RemoveAllListeners()
        {
            OnLevelCleared.RemoveAllListeners();
            OnLevelLoaded.RemoveAllListeners();
        }
    }
}