using System;
using System.Collections.Generic;
using Assets._Scripts.Enums;
using UnityEngine;

namespace Assets._Scripts.Datas
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData", order = 1)]
    public class LevelSO :ScriptableObject
    {
        public int Id;
        public int Index => Id;
        public EDifficulty Difficulty;
        public int MoveLimit;
        public List<BlockGroup> BlockGroups;
        public List<PillarData> PillarDatas;
        public HiddenBlockData HiddenBlockDatas;
        public List<CoveredPillarData> CoveredPillarDatas;
        public List<FrozenBlockData> FrozenBlockDatas;
        public int CoinReward = 20;
    }

    [Serializable]
    public class BlockGroup
    {
        public string Tag;
        public List<BlockData> BlockDatas = new();
    }

    [Serializable]
    public class BlockData
    {
        public int Id;
        public string IconId;
    }

    [Serializable]
    public class PillarData
    {
        public int Id;
        public HashSet<int> BlockIds = new();
    }

    [Serializable]
    public class HiddenBlockData
    {
        public HashSet<int> BlockIds = new();
    }

    [Serializable]
    public class CoveredPillarData
    {
        public string TagToOpen;
        public HashSet<int> PillarIds = new();
    }

    [Serializable]
    public class FrozenBlockData
    {
        public int MoveCountToRemove;
        public HashSet<int> BlockIds = new();
    }
}