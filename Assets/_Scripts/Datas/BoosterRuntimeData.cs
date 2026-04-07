using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Managers;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;

namespace Assets._Scripts.Datas
{
    public abstract class BoosterRuntimeData
    {
        public bool LockStatus {get; private set;}

        public BoosterRuntimeData(bool lockStatus)
        {
            LockStatus = lockStatus;
        }

        public void Do()
        {
            OnUsed();
        }

        public abstract void OnUsed();
        public abstract Tween DoMechanicAnim();
        public abstract string GetDetail();
    }

#region ExtraMoveBooster
    public class ExtraMoveBoosterRuntimeData : BoosterRuntimeData
    {
        private int _bonusAmount; 

        public ExtraMoveBoosterRuntimeData(bool lockStatus, int bonusAmount) : base(lockStatus)
        {
            _bonusAmount = bonusAmount;
        }

        public override void OnUsed()
        {
            GameManager.Instance.ChangeMoveCount(_bonusAmount, false);
            Debug.Log("Used Extra Move");
        }

        public override Tween DoMechanicAnim()
        {
            return IngameVisualController.Instance.UpdateMoveCount(LevelManager.PlayingLevel.MoveCount, true);
        }

        public override string GetDetail()
        {
            return $"Use it to get {_bonusAmount} extra moves";
        }
    }
#endregion

#region ShuffleBooster
    public class ShuffleBoosterRuntimeData : BoosterRuntimeData
    {
        private List<BlockController> _availableBlocks;
        private Vector3 _gatherPoint;

        public ShuffleBoosterRuntimeData(bool lockStatus, Vector3 gatherPoint) : base(lockStatus)
        {
            _availableBlocks = new();
            _gatherPoint = gatherPoint;
        }

        public override void OnUsed()
        {
            _availableBlocks.Clear();
            //TODO: Check mechanic of pillars
            var pillars = BoardController.Instance.GetAllPillars().Where(p => !p.IsLocked()).ToList();

            // Get all available blocks and group them
            List<List<BlockController>> matchedBlocks = new();
            foreach(var pillar in pillars)
            {
                //TODO: Check mechanic of blocks
                while (pillar.TryRemoveTopBlocks(out var toAdd))
                {
                    matchedBlocks.Add(toAdd);
                    _availableBlocks.AddRange(toAdd);
                }
                // Debug.Log($"Pillar {pillar.name} has {pillar.GetBlockCount()} blocks");
            }
            matchedBlocks = matchedBlocks.OrderByDescending(l => l.Count).ToList();
            var blockCount = matchedBlocks.Sum(mb => mb.Count);
            Debug.Log($"Shuffling {blockCount} blocks");
            
            // Get all available slot
            List<(PillarController, List<int>)> availableSlots = new();
            // List<(int, List<int>)> availableSlots = new(); // (int, List<int>) is (PillarId, List<SlotIndex>)
            foreach (var pillar in pillars)
            {
                // Debug.Log($"Check pillar {pillar.name}");
                List<int> slots = new();
                // List<int> slots = pillar.GetAvailableSlots();
                for (int i = 0; i < PillarController.MAX_BLOCKS; i++)
                {
                    if (!pillar.TryGetBlockAt(i, out var _))
                    {
                        slots.Add(i);
                        if (i >= PillarController.MAX_BLOCKS - 1) 
                        {
                            // availableSlots.Add((pillar.Id, slots));
                            availableSlots.Add((pillar, slots));
                            // Debug.Log($"Add {slots.Count} slots from pillar {pillar.name}");
                            slots = new();
                        }
                    }
                    else
                    {
                        // availableSlots.Add((pillar.Id, slots));
                        availableSlots.Add((pillar, slots));
                        // Debug.Log($"Add {slots.Count} slots from pillar {pillar.name}");
                        slots = new();
                    } 

                    
                }
            }
            Debug.Log($"Found {availableSlots.Count} group slots");

            // Add block to slot in order
            foreach (var matched in matchedBlocks)
            {
                var suitedSlots = availableSlots.Where(s => s.Item2.Count >= matched.Count).ToArray();
                // Debug.Log($"Found {suitedSlots.Length} suited slots for a {matched.Count}-block group");
                var randomSlots = suitedSlots[Random.Range(0, suitedSlots.Length)];
                availableSlots.Remove(randomSlots);
                
                var randomRange = randomSlots.Item2.Count - matched.Count;
                var pillar = randomSlots.Item1;
                var toSlot = Random.Range(randomSlots.Item2[0], randomSlots.Item2[0] + randomRange + 1);
                Debug.Log($"Move {matched.Count} blocks to slot {toSlot} of pillar {pillar.name}");

                for (int i = 0; i < matched.Count; i++)
                {
                    var slot = toSlot + i;
                    pillar.AddBlockToSlot(slot, matched[i]);
                    randomSlots.Item2.Remove(slot);
                }

                if (randomSlots.Item2.Count == 0) continue;

                var prefix = randomSlots.Item2.Where(s => s < toSlot).ToList();
                var suffix = randomSlots.Item2.Where(s => s >= toSlot + matched.Count).ToList();

                if (prefix.Count > 0) availableSlots.Add((pillar, prefix));
                if (suffix.Count > 0) availableSlots.Add((pillar, suffix));
            }

            foreach (var pillar in pillars) pillar.Arrange();

            Debug.Log("Used Shuffle");
        }

        public override Tween DoMechanicAnim()
        {
            Debug.Log("Do shuffle anim");
            var moveTime = .5f;
            var delayMoveTime = .05f;
            var stayTime = .5f;

            var sequence = DOTween.Sequence().SetTarget(this);
            
            float currentTime = 0f;
            foreach (var block in _availableBlocks)
            {
                sequence.Insert(currentTime, block.transform.DOMove(_gatherPoint, moveTime).SetEase(Ease.InSine));
                currentTime += delayMoveTime;
            }

            if (_availableBlocks.Count > 0)
            {
                // Thời điểm nhóm đầu tiên hoàn thành = (số block - 1) * delay + thời gian di chuyển
                currentTime = (_availableBlocks.Count - 1) * delayMoveTime + moveTime + stayTime;
            }
            else
            {
                currentTime = stayTime;
            }

            foreach (var block in _availableBlocks)
            {
                var blockPos = BoardController.Instance.GetBlockPosition(block);
                if (blockPos.Item1 == null || blockPos.Item2 == -1) continue;

                var worldPos = BlockMovementController.Instance.GetBlockPosition(blockPos.Item1, blockPos.Item2);
                sequence.Insert(currentTime, block.transform.DOMove(worldPos, moveTime).SetEase(Ease.InSine));
                currentTime += delayMoveTime;
            }

            sequence.AppendCallback(() => 
            {
                BlockMovementController.Instance.OnBlocksMoved?.Invoke(false);
            });

            return sequence.Play();
        }

        public override string GetDetail()
        {
            return $"Use it to get shuffle all available blocks";
        }
    }
#endregion

#region HintBooster
    public class HintBoosterRuntimeData : BoosterRuntimeData
    {
        private BlockController _randomBlock, _sameBlock;

        public HintBoosterRuntimeData(bool lockStatus) : base(lockStatus)
        {
        }

        public override void OnUsed()
        {
            var availablePillars = BoardController.Instance.GetAllPillars().Where(p => !p.IsLocked());
            List<BlockController> avilableBlocks = new();
            foreach(var pillar in availablePillars)
            {
                avilableBlocks.AddRange(pillar.GetAllBlocks());
            }
            
            _randomBlock = avilableBlocks[Random.Range(0, avilableBlocks.Count)];
            avilableBlocks.Remove(_randomBlock);

            var sameTag = avilableBlocks.Where(b => b.IsSameTag(_randomBlock)).ToArray();
            _sameBlock = sameTag[Random.Range(0, sameTag.Length)];

            Debug.Log("Used Hint");
        }

        public override Tween DoMechanicAnim()
        {
            if (!_sameBlock || !_randomBlock) return null;

            var animDuration = .3f;
            var animDelatTime = .3f;
            var sequence = DOTween.Sequence();

            sequence.AppendInterval(animDelatTime);
            sequence.Append(_randomBlock.transform.DOScale(1.3f, animDuration).SetEase(Ease.InSine))
                    .Join(_sameBlock.transform.DOScale(1.3f, animDuration).SetEase(Ease.InSine))
                    .Append(_randomBlock.transform.DOScale(1f, animDuration).SetEase(Ease.InSine))
                    .Join(_sameBlock.transform.DOScale(1f, animDuration).SetEase(Ease.InSine));

            return sequence.Play();
        }

        public override string GetDetail()
        {
            return $"Use it to see {2} blocks with same tag";
        }
    }

    #endregion
}