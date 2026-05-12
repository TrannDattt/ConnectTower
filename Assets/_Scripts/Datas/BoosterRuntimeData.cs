using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Datas
{
    public abstract class BoosterRuntimeData
    {
        public EBooster Key;
        public bool LockStatus {get; private set;}

        public BoosterRuntimeData(bool lockStatus)
        {
            LockStatus = lockStatus;
        }

        public void Do()
        {
            BlockMovementController.Instance.PutBackSelectedBlocks();
            OnUsed();
        }

        public abstract void OnUsed();
        public abstract string GetDetail();
    }

#region EXTRA MOVE
    public class ExtraMoveBoosterRuntimeData : BoosterRuntimeData
    {
        private int _bonusAmount; 

        public ExtraMoveBoosterRuntimeData(bool lockStatus, int bonusAmount) : base(lockStatus)
        {
            _bonusAmount = bonusAmount;
            Key = EBooster.ExtraMove;
        }

        public override void OnUsed()
        {
            GameManager.Instance.ChangeMoveCount(_bonusAmount, false);
            Debug.Log("Used Extra Move");
        }

        public override string GetDetail()
        {
            return $"Use it to get {_bonusAmount} extra moves";
        }
    }
#endregion

#region SHUFFLE
    public class ShuffleBoosterRuntimeData : BoosterRuntimeData
    {
        public Dictionary<BlockController, (PillarController, PillarController)> AvailableBlocks {get; private set;}

        public ShuffleBoosterRuntimeData(bool lockStatus) : base(lockStatus)
        {
            AvailableBlocks = new();
            Key = EBooster.Shuffle;
        }

        public override void OnUsed()
        {
            AvailableBlocks.Clear();
            var pillars = BoardController.Instance.GetAllPillars().Where(p => !p.IsLocked()).ToList();

            // Get all available blocks and group them
            List<List<BlockController>> matchedBlocks = new();
            foreach(var pillar in pillars)
            {
                while (pillar.TryRemoveTopBlocks(out var toAdd))
                {
                    matchedBlocks.Add(toAdd);
                    foreach(var block in toAdd)
                    {
                        AvailableBlocks[block] = (pillar, null);
                    }
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
                    AvailableBlocks[matched[i]] = (AvailableBlocks[matched[i]].Item1, pillar);
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

        public override string GetDetail()
        {
            return $"Use it to get shuffle all available blocks";
        }
    }
#endregion

#region HINT
    public class HintBoosterRuntimeData : BoosterRuntimeData
    {
        public BlockController[] GroupBlock {get; private set;}

        public HintBoosterRuntimeData(bool lockStatus) : base(lockStatus)
        {
            Key = EBooster.Hint;
        }

        public override void OnUsed()
        {
            GroupBlock = null;

            // 1. Lấy tất cả blocks hợp lệ từ các pillars hợp lệ (Flattening)
            var allValidBlocks = BoardController.Instance.GetAllPillars()
                .Where(p => !p.IsLocked() && ((IMechanicHandler)p).IsInMechanic())
                .SelectMany(p => p.GetAllBlocks())
                .ToArray();

            // 2. Nhóm các block theo Tag và lọc ra các nhóm có từ 2 block trở lên (để đảm bảo có cặp)
            // Giả sử b.GetTag() hoặc một thuộc tính tương đương trả về giá trị để so sánh tag
            var validGroups = allValidBlocks
                .GroupBy(b => b.Tag)
                .Where(g => g.Count() >= 2)
                .ToArray();

            var noneColorGroups = validGroups.Where(g => g.ToArray()[0].gameObject.GetComponent<BlockEffectVisual>().GetCurrentColor() == EColor.None).ToArray();

            // 3. Chọn ngẫu nhiên một nhóm (tag) ngẫu nhiên trong nhóm đó
            if (noneColorGroups.Length > 0)
            {
                GroupBlock = noneColorGroups[Random.Range(0, noneColorGroups.Length)].ToArray();
            }
            else if (validGroups.Length > 0)
            {
                GroupBlock = validGroups[Random.Range(0, validGroups.Length)].ToArray();
            }
            Debug.Log("Used Hint");
        }

        public override string GetDetail()
        {
            return $"Use it to see {2} blocks with same tag";
        }
    }
    #endregion

    #region ADD PILLAR
    public class AddPillarRuntimeData : BoosterRuntimeData
    {
        public PillarController NewPillar {get;private set;}
        private int _bonusAmount;

        public AddPillarRuntimeData(bool lockStatus, int bonusAmount) : base(lockStatus)
        {
            _bonusAmount = bonusAmount;
            Key = EBooster.AddPillar;
        }

        public override void OnUsed()
        {
            BoardController.Instance.AddNewPillar(out var newPillar);
            NewPillar = newPillar;
        }

        public override string GetDetail()
        {
            return $"Use it to get {_bonusAmount} extra moves";
        }
    }
    #endregion
}
