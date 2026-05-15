using System;
using System.Collections.Generic;
using Assets._Scripts.Enums;

namespace Assets._Scripts.Datas
{
    public class LevelJSON
    {
        public int Index;
        public EDifficulty Difficulty;
        public int MoveLimit;
        public List<BlockGroup> BlockGroups;
        public List<PillarData> PillarDatas;
        public HiddenBlockData HiddenBlockDatas;
        public List<CoveredPillarData> CoveredPillarDatas;
        public List<FrozenBlockData> FrozenBlockDatas;
        public ScratchBlockData ScratchedBlockDatas;
        public StickyBlockData StickyBlockDatas;
        public List<TrapPillarData> TrapPillarDatas;
        public int CoinReward = 20;

        public LevelJSON()
        {
            Index = -1;
            Difficulty = EDifficulty.Normal;
            MoveLimit = 0;

            BlockGroups = new();
            PillarDatas = new();

            HiddenBlockDatas = new();
            CoveredPillarDatas = new();
            FrozenBlockDatas = new();
            ScratchedBlockDatas = new();
            StickyBlockDatas = new();
            TrapPillarDatas = new();
            
            CoinReward = 0;
        }
    }

    [Serializable]
    public class BlockGroup
    {
        public string Tag;
        public List<BlockData> BlockDatas = new();
        public bool Trackable = true;
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
    public class ScratchBlockData
    {
        public HashSet<int> BlockIds = new();
    }

    [Serializable]
    public class StickyBlockData
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

    [Serializable]
    public class TrapPillarData
    {
        public int PillarId;
        public bool IsTrap;
    }
}