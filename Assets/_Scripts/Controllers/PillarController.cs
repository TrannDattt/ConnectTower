using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Controllers
{
    public class PillarController : MonoBehaviour
    {
        [field: SerializeField] public Transform BlockContainer {get; private set;}
        private Stack<BlockController> _blocks = new();
        private int _maxBlocks = 4;

        public UnityEvent OnPillarClicked = new();
        public UnityEvent OnFullMatched = new();

        public int GetBlockCount()
        {
            return _blocks.Count;
        }

        public Stack<BlockController> GetAllBlocks()
        {
            foreach(Transform child in BlockContainer)
            {
                if (child.TryGetComponent(out BlockController block))
                {
                    block.transform.position = BlockContainer.transform.position;
                    _blocks.Push(block);
                }
            }
            return _blocks;
        }

        public void AddBlock(BlockController block)
        {
            _blocks.Push(block);
            block.transform.SetParent(BlockContainer);
        }

        public void AddBlocks(List<BlockController> blocks)
        {
            foreach (var block in blocks)
            {
                AddBlock(block);
            }

            if (IsLocked())
            {
                OnFullMatched?.Invoke();
                //TODO: Merge VFX
            }
        }

        public BlockController RemoveTopBlock()
        {
            if (_blocks.Count == 0)
            {
                return null;
            }

            var toRemove = _blocks.Pop();
            toRemove.transform.SetParent(null);
            return toRemove;
        }

        public bool TryRemoveTopBlock(out BlockController block)
        {
            if (_blocks.Count == 0)
            {
                block = null;
                return false;
            }

            var toRemove = _blocks.Pop();
            toRemove.transform.SetParent(null);
            block = toRemove;
            return true;
        }

        public List<BlockController> TryGetBlocks()
        {
            if (_blocks.Count == 0)
            {
                return new List<BlockController>();
            }

            //TODO: Check for mechanics
            List<BlockController> result = new()
            {
                _blocks.Pop()
            };
            while (_blocks.TryPop(out BlockController block))
            {
                if (block.IsSameTag(result[0]))
                {
                    result.Add(block);
                }
                else
                {
                    _blocks.Push(block);
                    break;
                }
            }
            return result;
        }

        public bool IsLocked()
        {
            // TODO: Check for mechanics
            return _blocks.Count == _maxBlocks
                   && _blocks.All(b => b.IsSameTag(_blocks.Peek()));
        }

        public int GetAvailableSpace()
        {
            return _maxBlocks - _blocks.Count;
        }

        void Start()
        {
            GetAllBlocks();
        }

        void OnMouseDown()
        {
            OnPillarClicked?.Invoke();
        }
    }
}