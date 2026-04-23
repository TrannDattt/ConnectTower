using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Patterns.EventBus;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Controllers
{
    public class PillarController : MonoBehaviour, IMechanicHandler
    {
        public int Id {get; private set;} = -1;
        [field: SerializeField] public Transform TopPillar {get; private set;}
        [field: SerializeField] public Transform BlockContainer {get; private set;}
        private List<BlockController> _blocks = new() {null, null, null, null}; // Block with index 0 is at the bottom, index 3 is at the top.

        public const int MAX_BLOCKS = 4;

        public EMechanic ActiveMechanic { get; set; } = EMechanic.None;
        public MechanicVisualControl MechanicVisual { get; set; }

        #region Getter
        public int GetBlockCount()
        {
            var topBlock = GetTopBlock();
            return topBlock == null ? 0 : GetBlockIndex(topBlock) + 1;
        }

        public ICollection<BlockController> GetAllBlocks()
        {
            return _blocks.Where(b => b != null).ToList();
        }

        public bool TryGetBlockAt(int index, out BlockController block)
        {
            block = null;
            if (index >= MAX_BLOCKS || index < 0) return false;

            block = _blocks[index];
            return block != null;
        }

        public BlockController GetTopBlock()
        {
            return _blocks[0] == null ? null : _blocks.Last(b => b != null);
        }

        public int GetBlockIndex(BlockController block)
        {
            return _blocks.Contains(block) ? _blocks.IndexOf(block) : -1;
        }

        public List<int> GetBlocksIndices(List<BlockController> blocks)
        {
            return blocks.Select(b => GetBlockIndex(b)).ToList();
        }

        public int GetAvailableSlotCount()
        {
            return _blocks.Count(b => b == null);
        }

        public List<int> GetAvailableSlots()
        {
            return _blocks.Select(b => _blocks.IndexOf(b)).Where(i => _blocks[i] == null).ToList();
        }
#endregion

        public void Init(PillarData data)
        {
            if (data == null) return;
            Id = data.Id;
            _isFull = false;
            ActiveMechanic = EMechanic.None;
        }

#region Adder
        public void AddBlockToSlot(int index, BlockController block)
        {
            if (_blocks[index] != null)
            {
                Debug.Log($"Pillar {name} has already have block at index {index}");
                return;
            }

            _blocks[index] = block;
            block.transform.SetParent(BlockContainer);
        }

        public void AddBlockToTop(BlockController block)
        {
            var toSlot = 3;
            while (toSlot >= 0 && _blocks[toSlot] == null)
            {
                toSlot--;
            }
            toSlot += 1;
            AddBlockToSlot(toSlot, block);
        }

        public void AddBlocksToTop(List<BlockController> blocks)
        {
            foreach (var block in blocks)
            {
                AddBlockToTop(block);
            }
        }
#endregion

#region Remover
        private void RemoveBlock(BlockController block)
        {
            var index = GetBlockIndex(block);
            if (index >= 0)
            {
                _blocks[index] = null;
            }
        }

        public void RemoveBlocks(List<BlockController> blocks)
        {
            foreach (var block in blocks)
            {
                RemoveBlock(block);
            }
        }

        public void RemoveAllBlocks()
        {
            StopAllCoroutines();
            BlockContainer.DetachChildren();
            _blocks = new() {null, null, null, null};
        }

        public bool TryRemoveTopBlocks(out List<BlockController> result, bool skipMechanic = false)
        {
            if (TryGetTopBlocks(out result, skipMechanic))
            {
                RemoveBlocks(result);
                return true;
            }
            return false;
        }

        public bool TryGetTopBlocks(out List<BlockController> result, bool skipMechanic = false, bool ignoreLock = false)
        {
            result = new List<BlockController>();
            if (!HasBlock() || (IsLocked() && !ignoreLock) || !(this as IMechanicHandler).IsInteractable()) return false;

            int i = MAX_BLOCKS - 1;
            while (i >= 0 && _blocks[i] == null) i--;

            bool IsInteractable(int idx) => (_blocks[idx] as IMechanicHandler).IsInteractable();

            if (i >= 0 && !IsInteractable(i))
            {
                if (!skipMechanic) return false;
                while (i >= 0 && !IsInteractable(i)) i--;
            }

            if (i < 0) return false;

            BlockController toCompare = _blocks[i];
            while (i >= 0 && _blocks[i] != null && _blocks[i].IsSameTag(toCompare) && (skipMechanic || IsInteractable(i)))
            {
                result.Add(_blocks[i--]);
            }

            return result.Count > 0;
        }
#endregion

#region Checker
        public bool IsLocked()
        {
            return _blocks.All(b => b != null && b.IsSameTag(_blocks[0]) && (b as IMechanicHandler).IsInteractable());
        }

        public bool HasBlock()
        {
            return _blocks.Any(b => b != null);
        }

        private bool _isFull;
        public void CheckFullMatch()
        {
            if (!_isFull && IsLocked())
            {
                Debug.Log("Locked");
                _isFull = true;
                EventBus<PillarFullMatchedEvent>.Publish(new PillarFullMatchedEvent { Pillar = this , Tag = _blocks[0].Tag});
            }
        }
#endregion

        public void Arrange()
        {
            if (!HasBlock()) return;

            BlockController curBlock, nextBlock;
            for (int i = 0; i < MAX_BLOCKS - 1; i++)
            {
                curBlock = _blocks[i];
                if (curBlock != null) continue;

                int j = i + 1;
                while (j < MAX_BLOCKS && !_blocks[j]) j++;
                if (j == MAX_BLOCKS) break;

                nextBlock = _blocks[j];
                _blocks[i] = nextBlock;
                _blocks[j] = curBlock;
            }
        }

        public IEnumerator DoSpawnBlockAnim()
        {

            float fallDuration = .4f;
            float offsetY = 4.2f;

            var activeBlocks = GetAllBlocks();
            if (activeBlocks.Count == 0) yield break;

            var sequence = DOTween.Sequence().SetTarget(this).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            for (int i = 0; i < activeBlocks.Count; i++)
            {
                var block = activeBlocks.ElementAt(i);
                var targetPos = block.transform.position;
                block.gameObject.SetActive(true);
                block.transform.position = targetPos + Vector3.up * offsetY;
                sequence.Insert(i * .1f, block.transform.DOMove(targetPos, fallDuration).SetEase(Ease.InQuad));
            }

            yield return sequence.WaitForCompletion();
        }

        void Awake()
        {
            MechanicVisual = GetComponent<MechanicVisualControl>();
        }

        void Start()
        {
        }

        void OnMouseDown()
        {
            // Debug.Log($"Pillar {name} clicked");
            if ((this as IMechanicHandler).IsInteractable())
            {
                EventBus<PillarClickedEvent>.Publish(new PillarClickedEvent { Pillar = this });
            }
        }
    }

    public struct PillarClickedEvent : IEvent
    {
        public PillarController Pillar;
    }

    public struct PillarFullMatchedEvent : IEvent
    {
        public PillarController Pillar;
        public string Tag;
    }
}
