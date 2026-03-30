using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class BoardController : Singleton<BoardController>
    {
        [SerializeField] private Transform _boardTransform;

        private List<PillarController> _pillars = new();

        public void InitBoard(LevelRuntimeData levelData)
        {
            //Init blocks
            List<BlockController> blocks = new();
            foreach (var blockGroup in levelData.BlockGroups)
            {
                blockGroup.BlockDatas.ForEach(d =>
                {
                    var newBlock = SlotLayoutManager.Instance.GetBlock(-1, null);
                    //-------------------------
                    //TODO:
                    newBlock.Init(d, blockGroup.Tag);
                    //-----------------------
                    blocks.Add(newBlock);
                });
            }

            //Init pillars
            if (_pillars.Count == 0)
            {
                foreach(Transform t in _boardTransform)
                {
                    if (t.TryGetComponent(out PillarController pillar))
                        _pillars.Add(pillar);
                }
            }
            SlotLayoutManager.Instance.ReturnAllPillar(_pillars);

            _pillars = SlotLayoutManager.Instance.GetPillars(levelData.PillarDatas.Count);
            
            for (int i = 0; i < _pillars.Count; i++)
            {
                _pillars[i].Init(levelData.PillarDatas[i]);
                if (levelData.PillarDatas[i].BlockIds.Count == 0) continue;
                int indexOffset = 0;
                for(int j = 0; j < PillarController.MAX_BLOCKS; j++)
                {
                    var blockId = levelData.PillarDatas[i].BlockIds.ElementAt(j);
                    var block = blocks.FirstOrDefault(b => b.Id == blockId);
                    if (block == default(BlockController))
                    {
                        indexOffset++;
                    }
                    else
                    {
                        _pillars[i].AddBlockToSlot(j - indexOffset, block);
                        block.transform.position = _pillars[i].BlockContainer.transform.position + GameObjectDataHelper.BlockHeight * (j - indexOffset) * Vector3.up;
                    }
                }
                _pillars[i].Arrange();
            }

            // Init Mechanics
            foreach (var id in levelData.HiddenBlockDatas.BlockIds)
            {
                var toApply = blocks.FirstOrDefault(b => b.Id == id);
                var mechanic = new HiddenBlockMechanic();
                mechanic.Apply(toApply);
            };

            levelData.CoveredPillarDatas.ForEach(data =>
            {
                foreach (var id in data.PillarIds)
                {
                    var toApply = _pillars.FirstOrDefault(p => p.Id == id);
                    var mechanic = new CoveredPillarMechanic(data.TagToOpen);
                    mechanic.Apply(toApply);
                };
            });

            levelData.FrozenBlockDatas.ForEach(data =>
            {
                foreach (var id in data.BlockIds)
                {
                    var toApply = blocks.FirstOrDefault(b => b.Id == id);
                    var mechanic = new FrozenBlockMechanic(data.MoveCountToRemove);
                    mechanic.Apply(toApply);
                };
            });
        }

        public List<PillarController> GetAllPillars()
        {
            return _pillars;
        }

        public List<BlockController> GetAllBlocks()
        {
            List<BlockController> allBlocks = new();
            var pillars = GetAllPillars();
            pillars.ForEach(p => allBlocks.AddRange(p.GetAllBlocks()));
            return allBlocks;
        }

        public (PillarController, int) GetBlockPosition(BlockController block)
        {
            var pillars = GetAllPillars();
            foreach (var pillar in pillars)
            {
                int index = pillar.GetBlockIndex(block);
                if (index >= 0) return (pillar, index);
            }

            return (null, -1);
        }
    }
}