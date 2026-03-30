using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Interfaces;
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

        public UnityEvent<PillarController> OnPillarClicked = new();
        public UnityEvent<string> OnFullMatched = new();
        public EMechanic ActiveMechanic { get; set; } = EMechanic.None;
        public MechanicVisualControl MechanicVisual { get; set; }

        #region Getter
        // TODO: Assumed all lower slots have block.
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
            // Debug.Log($"Moved {blocks.Count} blocks");
            // Debug.Log($"Check lock: {IsLocked()}");

            if (IsLocked())
            {
                Debug.Log("Locked");
                OnFullMatched?.Invoke(blocks[0].Tag);
                //TODO: Merge VFX
            }
        }
#endregion

#region Remover
        public bool TryRemoveTopBlocks(out List<BlockController> result)
        {
            result = new List<BlockController>();
            if (!HasBlock() || IsLocked())
            {
                return false;
            }

            var toCompare = GetTopBlock();
            if (toCompare == null || !(toCompare as IMechanicHandler).IsInteractable())
            {
                return false;
            }

            var checkIndex = -1;
            for (int i = MAX_BLOCKS - 1; i >= 0; i--)
            {
                if (_blocks[i] != null)
                {
                    checkIndex = i;
                    break;
                }
            }

            if (checkIndex == -1) return false;

            while (checkIndex >= 0)
            {
                var toCheck = _blocks[checkIndex];
                if (toCheck == null) break;

                var handler = toCheck as IMechanicHandler;
                if (!handler.IsInteractable() || handler.IsHidden())
                {
                    break;
                }

                if (toCheck.IsSameTag(toCompare))
                {
                    result.Add(toCheck);
                    _blocks[checkIndex] = null;
                    checkIndex--;
                }
                else break;
            }

            return result.Count > 0;
        }
#endregion

#region Checker
        public bool IsLocked()
        {
            // TODO: Check for mechanics
            return _blocks.All(b => b != null && b.IsSameTag(_blocks[0]));
        }

        public bool HasBlock()
        {
            return _blocks.Any(b => b != null);
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

        void Awake()
        {
            MechanicVisual = GetComponent<MechanicVisualControl>();
        }

        void OnMouseDown()
        {
            // Debug.Log($"Pillar {name} clicked");
            if ((this as IMechanicHandler).IsInteractable())
            {
                OnPillarClicked?.Invoke(this);
            }
        }
    }
}