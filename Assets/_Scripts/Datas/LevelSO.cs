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
    }

    [Serializable]
    public class BlockGroup
    {
        public string Tag;
        public List<int> BlockIds;
        // TODO: Make a list of block data
    }

    [Serializable]
    public class PillarData
    {
        public int Id;
        public List<int> BlockIds;
    }

    [Serializable]
    public class MechanicData
    {
        
    }
}