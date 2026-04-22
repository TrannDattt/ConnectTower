using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Helpers;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
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
        public abstract Tween DoBoosterAnim();
        public abstract Tween DoBoosterButtonAnim(Image target);
        public abstract string GetDetail();
    }

#region ExtraMoveBooster
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

        public override Tween DoBoosterAnim()
        {
            return IngameVisualController.Instance.UpdateMoveCount(LevelManager.PlayingLevel.MoveCount, true).SetTarget(this);
        }

        public override string GetDetail()
        {
            return $"Use it to get {_bonusAmount} extra moves";
        }

        public override Tween DoBoosterButtonAnim(Image target)
        {
            ParticleSystem particle = null;
            var attractor = Object.FindFirstObjectByType<MoveCountVisual>().GetComponentInChildren<UIParticleAttractor>();
            return DOTween.Sequence().AppendCallback(() => 
            {
                var it = ParticleManager.Instance.PlayParticle(EParticle.Firefly, target.transform.position, target.transform.parent);
                BoosterController.Instance.StartCoroutine(it);
                particle = it.Current;
                
                if (particle != null && attractor != null)
                    attractor.AddParticleSystem(particle);
            })
            .AppendInterval(ParticleManager.Instance.GetParticleDuration(EParticle.Firefly) + .4f)
            .OnComplete(() =>
            {
                if (particle != null && attractor != null)
                    attractor.RemoveParticleSystem(particle);
            })
            .OnKill(() =>
            {
                if (particle != null && attractor != null)
                    attractor.RemoveParticleSystem(particle);
            });
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
            Key = EBooster.Shuffle;
        }

        public override void OnUsed()
        {
            _availableBlocks.Clear();
            var pillars = BoardController.Instance.GetAllPillars().Where(p => !p.IsLocked()).ToList();

            // Get all available blocks and group them
            List<List<BlockController>> matchedBlocks = new();
            foreach(var pillar in pillars)
            {
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

        public override Tween DoBoosterAnim()
        {
            Debug.Log("Do shuffle anim");
            var moveTime = .5f;
            var totalDelayMove = 1.5f;
            var defaultDelayMove = .05f;
            var delayMoveTime = Mathf.Min(totalDelayMove / (_availableBlocks.Count - 1), defaultDelayMove);
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

        public override Tween DoBoosterButtonAnim(Image target)
        {
            float duration = 4f;
            float cycles = 10;
            return target.transform.DORotate(new Vector3(0, 0, -360 * cycles), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutCubic);
        }
    }
#endregion

#region HintBooster
    public class HintBoosterRuntimeData : BoosterRuntimeData
    {
        private BlockController _randomBlock, _sameBlock;

        public HintBoosterRuntimeData(bool lockStatus) : base(lockStatus)
        {
            Key = EBooster.Hint;
        }

        public override void OnUsed()
        {
            var availablePillars = BoardController.Instance.GetAllPillars().Where(p => !p.IsLocked() && (p as IMechanicHandler).IsInteractable()).ToArray();
            List<BlockController> avilableBlocks = new();
            foreach(var pillar in availablePillars)
            {
                avilableBlocks.AddRange(pillar.GetAllBlocks().Where(b => (b as IMechanicHandler).IsInteractable() && b.GetComponent<BlockEffectVisual>().GetCurrentColor() == EColor.None));
            }
            
            _randomBlock = avilableBlocks[Random.Range(0, avilableBlocks.Count)];
            avilableBlocks.Remove(_randomBlock);

            var sameTag = avilableBlocks.Where(b => b.IsSameTag(_randomBlock)).ToArray();
            _sameBlock = sameTag[Random.Range(0, sameTag.Length)];

            Debug.Log("Used Hint");
        }

        public override Tween DoBoosterAnim()
        {
            if (!_sameBlock || !_randomBlock) return null;

            var animDuration = .3f;
            var animDelayTime = .5f;
            var sequence = DOTween.Sequence().SetTarget(this);

            sequence.AppendInterval(animDelayTime)
                    .Append(_randomBlock.transform.DOScale(1.3f, animDuration).SetEase(Ease.InSine))
                    .Join(_sameBlock.transform.DOScale(1.3f, animDuration).SetEase(Ease.InSine))
                    .Append(_randomBlock.transform.DOScale(1f, animDuration).SetEase(Ease.InSine))
                    .Join(_sameBlock.transform.DOScale(1f, animDuration).SetEase(Ease.InSine))
                    .AppendCallback(() =>
                    {
                        var randomBlockVisual = _randomBlock.GetComponent<BlockEffectVisual>();
                        var sameBlockVisual = _sameBlock.GetComponent<BlockEffectVisual>();
                        var toChange = randomBlockVisual.GetCurrentColor() != EColor.None ?
                                        randomBlockVisual.GetCurrentColor() : sameBlockVisual.GetCurrentColor() != EColor.None ?
                                        sameBlockVisual.GetCurrentColor() : GetRandomUnusedColor();

                        randomBlockVisual.ChangeColor(toChange);
                        sameBlockVisual.ChangeColor(toChange);
                    });

            return sequence.Play();
        }

        private EColor GetRandomUnusedColor()
        {
            var blockVisuals = BoardController.Instance.GetAllBlocks().Select(b => b.GetComponent<BlockEffectVisual>());
            HashSet<EColor> usedColors = new();
            foreach(var visual in blockVisuals)
            {
                usedColors.Add(visual.GetCurrentColor());
            }

            var availableColors = ColorMapper.GetAllColors().Where(c => !usedColors.Contains(c)).ToArray();
            if (availableColors.Length == 0) return usedColors.First();
            return availableColors[Random.Range(0, availableColors.Length)];
        }

        public override string GetDetail()
        {
            return $"Use it to see {2} blocks with same tag";
        }

        public override Tween DoBoosterButtonAnim(Image target)
        {
            return DOTween.Sequence().AppendCallback(() =>
            {
                for (int i = 0; i < 2; i++)
                {
                    var it = ParticleManager.Instance.PlayParticle(EParticle.Hint, target.transform.position, target.transform.parent);
                    BoosterController.Instance.StartCoroutine(it);
                    var hintImg = it.Current;
                    var hintTarget = i == 0 ? _randomBlock : _sameBlock;
                    var attractor = hintTarget.gameObject.AddComponent<UIParticleAttractor>();
                    attractor.AddParticleSystem(hintImg);
                    attractor.movement = UIParticleAttractor.Movement.Sphere;
                    attractor.maxSpeed = .3f;
                }
            })
            .AppendInterval(ParticleManager.Instance.GetParticleDuration(EParticle.Hint) + .3f)
            .OnComplete(() =>
            {
                if (_randomBlock != null && _randomBlock.TryGetComponent<UIParticleAttractor>(out var a1)) Object.Destroy(a1);
                if (_sameBlock != null && _sameBlock.TryGetComponent<UIParticleAttractor>(out var a2)) Object.Destroy(a2);
            })
            .OnKill(() =>
            {
                if (_randomBlock != null && _randomBlock.TryGetComponent<UIParticleAttractor>(out var a1)) Object.Destroy(a1);
                if (_sameBlock != null && _sameBlock.TryGetComponent<UIParticleAttractor>(out var a2)) Object.Destroy(a2);
            });
        }
    }

    #endregion
}