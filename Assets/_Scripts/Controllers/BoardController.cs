using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class BoardController : Singleton<BoardController>
    {
        [SerializeField] private Transform _boardTransform;
        [SerializeField] private PillarController _pillarPrefab;
        [SerializeField] private BlockController _blockPrefab;

        private List<PillarController> _pillars = new();
        private List<BlockController> _blocks = new();
        private List<MechanicRuntimeData> _mechanics = new();

        private Pooling<PillarController> _pillarPool = new();
        private Pooling<BlockController> _blockPool = new();

        public void InitBoard(LevelRuntimeData levelData)
        {
            ClearBoard();
            //Init blocks
            foreach (var blockGroup in levelData.BlockGroups)
            {
                blockGroup.BlockDatas.ForEach(d =>
                {
                    var newBlock = _blockPool.GetItem();
                    newBlock.Init(d, blockGroup.Tag);
                    _blocks.Add(newBlock);
                });
            }

            //Init pillars
            var pillarPos = SlotLayoutManager.Instance.GetPillarPositions(levelData.PillarDatas.Count, _boardTransform);
            for(int i = 0; i < levelData.PillarDatas.Count; i++)
            {
                var newPillar = _pillarPool.GetItem();
                newPillar.transform.position = pillarPos[i];

                newPillar.Init(levelData.PillarDatas[i]);
                if (levelData.PillarDatas[i].BlockIds.Count == 0) continue;

                int indexOffset = 0;
                for(int j = 0; j < PillarController.MAX_BLOCKS; j++)
                {
                    var blockId = levelData.PillarDatas[i].BlockIds.ElementAt(j);
                    var block = _blocks.FirstOrDefault(b => b.Id == blockId);
                    if (block == default(BlockController))
                    {
                        indexOffset++;
                    }
                    else
                    {
                        newPillar.AddBlockToSlot(j - indexOffset, block);
                        block.transform.position = newPillar.BlockContainer.transform.position + GameObjectDataHelper.BlockHeight * (j - indexOffset) * Vector3.up;
                    }
                }
                newPillar.Arrange();
                _pillars.Add(newPillar);
            }

            // Init Mechanics
            if (levelData.HiddenBlockDatas?.BlockIds != null)
            {
                Debug.Log($"Init {levelData.HiddenBlockDatas.BlockIds.Count} hidden blocks");
                foreach (var id in levelData.HiddenBlockDatas.BlockIds)
                {
                    var toApply = _blocks.FirstOrDefault(b => b.Id == id);
                    if (toApply == null) continue;
                    var mechanic = new HiddenBlockMechanic();
                    mechanic.Apply(toApply);
                    _mechanics.Add(mechanic);
                }
            }

            levelData.CoveredPillarDatas?.ForEach(data =>
            {
                Debug.Log($"Init {data.PillarIds} covered pillars");
                foreach (var id in data.PillarIds)
                {
                    var toApply = _pillars.FirstOrDefault(p => p.Id == id);
                    if (toApply == null) continue;
                    var mechanic = new CoveredPillarMechanic(data.TagToOpen);
                    mechanic.Apply(toApply);
                    _mechanics.Add(mechanic);
                };
            });

            levelData.FrozenBlockDatas?.ForEach(data =>
            {
                Debug.Log($"Init {data.BlockIds} frozen blocks");
                foreach (var id in data.BlockIds)
                {
                    var toApply = _blocks.FirstOrDefault(b => b.Id == id);
                    if (toApply == null) continue;
                    var mechanic = new FrozenBlockMechanic(data.MoveCountToRemove);
                    mechanic.Apply(toApply);
                    _mechanics.Add(mechanic);
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
        
        public IEnumerator DoSpawnBlockAnim()
        {
            var delaySpawn = new WaitForSeconds(.1f);
            var nonMechanicPillars = _pillars.Where(p => (p as IMechanicHandler).IsInteractable()).ToArray();

            foreach(var pillar in nonMechanicPillars)
            {
                foreach(var block in pillar.GetAllBlocks()) block.gameObject.SetActive(false);
            }

            foreach (var pillar in nonMechanicPillars)
            {
                pillar.StartCoroutine(pillar.DoSpawnBlockAnim());
                yield return delaySpawn;
            }
            yield return new WaitForSeconds(.8f);
        }

        public void ClearBoard()
        {
            foreach (var mechanic in _mechanics)
            {
                mechanic.Remove(false);
            }
            _mechanics.Clear();

            foreach (var pillar in _pillars)
            {
                pillar.RemoveAllBlocks();
                if (pillar.gameObject.TryGetComponent(out PillarEffectVisual pillarVisual))
                    pillarVisual.ResetVisual();
                _pillarPool.ReturnItem(pillar);
            }
            _pillars.Clear();

            foreach(var block in _blocks)
            {
                if (block.gameObject.TryGetComponent(out BlockEffectVisual blockVisual))
                    blockVisual.ResetVisual();
                _blockPool.ReturnItem(block);
                block.transform.SetParent(transform);
            }
            _blocks.Clear();
        } 

        protected override void Awake()
        {
            base.Awake();

            var pillars = _boardTransform.GetComponentsInChildren<PillarController>();
            foreach(var pillar in pillars) Destroy(pillar.gameObject);

            _pillarPool = new(_pillarPrefab, 10, _boardTransform);
            _blockPool = new(_blockPrefab, 40, _boardTransform);
        }
    }
}