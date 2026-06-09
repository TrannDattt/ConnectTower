using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using Assets._Scripts.Visuals;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Datas
{
    public abstract class MechanicRuntimeData
    {
        private static readonly Dictionary<IMechanicHandler, HashSet<MechanicRuntimeData>> MechanicsByTarget = new();
        private static long _applySequence;

        public EMechanic Key {get; protected set;}
        protected IMechanicHandler _target;
        protected UnityAction<BlocksMovedEvent> OnCheckCondicion;
        private long _applyOrder;

        protected EventBinding<BlocksMovedEvent> _blocksMovedBinding;

        public MechanicRuntimeData()
        {
            OnCheckCondicion = (_) =>
            {
                if (CheckRemoveCondition())
                {
                    Remove();
                }
            };

            _blocksMovedBinding = new(OnCheckCondicion);
        }

        protected abstract bool CheckRemoveCondition();

        public static MechanicRuntimeData GetLatestRegisteredMechanic(IMechanicHandler target)
        {
            if (target == null || !MechanicsByTarget.TryGetValue(target, out var mechanics))
                return null;

            return mechanics
                .Where(mechanic => mechanic != null && mechanic._target == target)
                .OrderByDescending(mechanic => mechanic._applyOrder)
                .FirstOrDefault();
        }

        protected bool TryPrepareTarget(IMechanicHandler target)
        {
            if (target == null)
                return false;

            if (!ReferenceEquals(_target, target))
                UnregisterCurrentTarget();

            _target = target;
            if (_target.ActiveMechanic == Key)
                return false;

            RegisterCurrentTarget();
            return true;
        }

        protected void RegisterCurrentTarget()
        {
            if (_target == null)
                return;

            if (!MechanicsByTarget.TryGetValue(_target, out var mechanics))
            {
                mechanics = new();
                MechanicsByTarget[_target] = mechanics;
            }

            mechanics.Add(this);
            _applyOrder = ++_applySequence;
        }

        protected void UnregisterCurrentTarget()
        {
            if (_target == null || !MechanicsByTarget.TryGetValue(_target, out var mechanics))
                return;

            mechanics.Remove(this);
            if (mechanics.Count == 0)
                MechanicsByTarget.Remove(_target);
        }

        public virtual void Apply(IMechanicHandler target)
        {
            Debug.Log($"Applying mechanic {Key} to {target}");
            if (!TryPrepareTarget(target)) return;
            _target.UpdateMechanic(this);
            
            EventBus<BlocksMovedEvent>.Subscribe(_blocksMovedBinding);
            
            OnCheckCondicion?.Invoke(new BlocksMovedEvent { MovedByPlayer = false });
        }

        public virtual void ApplyImmediate(IMechanicHandler target)
        {
            Debug.Log($"Applying mechanic immediately {Key} to {target}");
            if (!TryPrepareTarget(target)) return;
            _target.UpdateMechanicImmediate(this);

            EventBus<BlocksMovedEvent>.Subscribe(_blocksMovedBinding);

            OnCheckCondicion?.Invoke(new BlocksMovedEvent { MovedByPlayer = false });
        }

        public virtual void Remove(bool doEffect = true)
        {
            Debug.Log($"Removing mechanic {Key} from {_target}");
            if (_target == null) return;
            var target = _target;
            UnregisterCurrentTarget();
            target.ClearMechanic(doEffect);
            _target = null;
            
            EventBus<BlocksMovedEvent>.Unsubscribe(_blocksMovedBinding);
            if (!doEffect) return;
            // DoMechanicSFX(Key);
        }

        public virtual void RemoveImmediate(bool doEffect = true)
        {
            Debug.Log($"Removing mechanic immediately {Key} from {_target}");
            if (_target == null) return;
            var target = _target;
            UnregisterCurrentTarget();
            target.ClearMechanicImmediate(doEffect);
            _target = null;

            EventBus<BlocksMovedEvent>.Unsubscribe(_blocksMovedBinding);
            if (!doEffect) return;
            // DoMechanicSFX(Key);
        }

        // protected void DoMechanicSFX(EMechanic key)
        // {
        //     var mechanicSFX = key switch
        //     {
        //         EMechanic.HiddenBlock => ESfx.HiddenBlockExit,
        //         EMechanic.CoveredPillar => ESfx.CoveredPillarExit,
        //         EMechanic.FrozenBlock => ESfx.FrozenBlockExit,
        //         _ => ESfx.None
        //     };
        //     SoundManager.Instance.PlayRandomSFX(mechanicSFX);
        // }
    }

#region Hidden Block
    public class HiddenBlockMechanic : MechanicRuntimeData
    {
        public HiddenBlockMechanic() : base()
        {
            Key = EMechanic.HiddenBlock;
        }

        protected override bool CheckRemoveCondition()
        {
            var block = _target as BlockController;
            return block != null && block.GetPillarParent().GetTopBlock() == block;
        }
    }
#endregion

#region Covered Pillar
    public class CoveredPillarMechanic : MechanicRuntimeData
    {
        public string TagToOpen {get; private set;}

        private UnityAction<PillarFullMatchedEvent> OnCheckCoveredPillarCondicion;
        private EventBinding<PillarFullMatchedEvent> _pillarFullMatchedBinding;

        public CoveredPillarMechanic(string tagToOpen) : base()
        {
            Key = EMechanic.CoveredPillar;
            TagToOpen = tagToOpen;

            OnCheckCoveredPillarCondicion = (evt) =>
            {
                if (CheckRemoveCondition(evt.Tag))
                {
                    Remove();
                }
            };

            _pillarFullMatchedBinding = new(OnCheckCoveredPillarCondicion);
            _blocksMovedBinding = new(() => {});
        }

        public override void Apply(IMechanicHandler target)
        {
            if (!TryPrepareTarget(target)) return;
            _target.UpdateMechanic(this);
            
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchedBinding);
        }

        public override void ApplyImmediate(IMechanicHandler target)
        {
            if (!TryPrepareTarget(target)) return;
            _target.UpdateMechanicImmediate(this);

            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchedBinding);
        }

        public override void Remove(bool doEffect = true)
        {
            if (_target == null) return;
            var target = _target;
            UnregisterCurrentTarget();
            target.ClearMechanic(doEffect);
            
            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchedBinding);
            
            _target = null;
        }

        public override void RemoveImmediate(bool doEffect = true)
        {
            if (_target == null) return;
            var target = _target;
            UnregisterCurrentTarget();
            target.ClearMechanicImmediate(doEffect);

            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchedBinding);

            _target = null;
        }

        protected override bool CheckRemoveCondition()
        {
            var pillar = _target as PillarController;
            return pillar != null && pillar.IsLocked() && pillar.GetTopBlock().IsSameTag(TagToOpen);
        }

        private bool CheckRemoveCondition(string tag)
        {
            return _target != null && tag == TagToOpen;
        }
    }
#endregion

    #region Frozen Block
    public class FrozenBlockMechanic : MechanicRuntimeData
    {
        private EventBinding<PillarFullMatchedEvent> _pillarFullMatchedBinding;
        private bool _syncImmediately;

        public FrozenBlockMechanic() : base()
        {
            Key = EMechanic.FrozenBlock;
            OnCheckCondicion = (_) =>
            {
                if (_target is PillarController pillar)
                {
                    ApplyFrozenToMatchingBlocks(pillar, _syncImmediately);
                    return;
                }

                if (CheckRemoveCondition())
                {
                    Remove();
                }
            };
            _blocksMovedBinding = new(OnCheckCondicion);

            _pillarFullMatchedBinding = new((evt) =>
            {
                if (_target is PillarController pillar)
                {
                    if (evt.Pillar == pillar) Remove();
                }
                else if (_target is BlockController block)
                {
                    var handler = block as IMechanicHandler;
                    if (block.IsSameTag(evt.Tag) && handler.ActiveMechanic == EMechanic.FrozenBlock)
                        Remove();
                }
            });
        }

        public override void Apply(IMechanicHandler target)
        {
            _syncImmediately = false;
            base.Apply(target);
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchedBinding);
        }

        public override void ApplyImmediate(IMechanicHandler target)
        {
            _syncImmediately = true;
            base.ApplyImmediate(target);
            _syncImmediately = false;
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchedBinding);
        }

        public override void Remove(bool doEffect = true)
        {
            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchedBinding);
            base.Remove(doEffect);
        }

        public override void RemoveImmediate(bool doEffect = true)
        {
            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchedBinding);
            base.RemoveImmediate(doEffect);
        }

        protected override bool CheckRemoveCondition()
        {
            return false;
        }

        public static void ReevaluateFrozenState(PillarController pillar, bool immediate)
        {
            ApplyFrozenToMatchingBlocks(pillar, immediate);
        }

        private static void ApplyFrozenToMatchingBlocks(PillarController pillar, bool immediate)
        {
            if (pillar == null || pillar.ActiveMechanic != EMechanic.FrozenBlock)
                return;

            var pillarBlocks = pillar.GetAllBlocks().ToList();
            if (pillarBlocks.Count == 0)
                return;

            var bottomBlock = pillar.GetBottomBlock();
            if (bottomBlock == null)
                return;

            for (int i = 1; i < pillarBlocks.Count; i++)
            {
                var belowBlock = pillarBlocks[i - 1];
                var block = pillarBlocks[i];
                if (belowBlock == null || block == null || !block.IsSameTag(bottomBlock))
                    continue;

                var belowHandler = belowBlock as IMechanicHandler;
                var handler = block as IMechanicHandler;
                if (belowHandler.ActiveMechanic != EMechanic.FrozenBlock || handler.ActiveMechanic != EMechanic.None)
                    continue;

                var mechanic = new FrozenBlockMechanic();
                if (immediate)
                    mechanic.ApplyImmediate(block);
                else
                    mechanic.Apply(block);

                BoardController.Instance?.RegisterMechanic(mechanic);
            }
        }
    }
    #endregion

    #region Scratched Block
    public class ScratchedBlockMechanic : MechanicRuntimeData
    {
        private struct ScratchResolutionState
        {
            public int BlockId;
            public bool IsResolved;
        }

        private const int InvalidBlockId = -1;
        private static HashSet<BlockController> _scratchedBlocks;
        private static int _sharedBlockId = InvalidBlockId;
        private static int _sharedBlockSelectionFrame = -1;
        private static int _scratchResolutionFrame = -1;
        private static int _playerTriggeredScratchFrame = -1;
        private static readonly Dictionary<int, ScratchResolutionState> _scratchResolutionByPillar = new();
        private static bool _isResolvingScratchRemoval;
        public int ScratchState {get; private set;}
        private EventBinding<BlocksMatchedEvent> _blockMatchBinding;

        private EventBinding<PillarFullMatchedEvent> _pillarFullMatchBinding;

        public ScratchedBlockMechanic() : base()
        {
            Key = EMechanic.ScratchBlock;
            _blocksMovedBinding = new((evt) =>
            {
                if (evt.MovedByPlayer)
                    _playerTriggeredScratchFrame = Time.frameCount;
            });
            _pillarFullMatchBinding = new((evt) =>
            {
                if (_isResolvingScratchRemoval
                    || _playerTriggeredScratchFrame != Time.frameCount
                    || !(_target is BlockController block)
                    || evt.Pillar == null)
                    return;

                if (!TryResolveScratchForMatch(evt.Pillar.Id, block.Id))
                    return;

                _isResolvingScratchRemoval = true;
                try
                {
                    Remove();
                }
                finally
                {
                    _isResolvingScratchRemoval = false;
                }
            });
            _blockMatchBinding = new((evt) =>
            {
                if (_sharedBlockId != (_target as BlockController).Id || evt.MatchCount <= ScratchState) return;
                ScratchState = evt.MatchCount;
                _target.UpdateMechanic(this);
            });

            if (_sharedBlockId == InvalidBlockId) GetRandomBlockId();
            ScratchState = 1;
        }

        private static void ResetScratchResolutionStateIfNeeded()
        {
            if (_scratchResolutionFrame == Time.frameCount) return;

            _scratchResolutionFrame = Time.frameCount;
            _scratchResolutionByPillar.Clear();
        }

        private static bool TryResolveScratchForMatch(int pillarId, int blockId)
        {
            ResetScratchResolutionStateIfNeeded();

            if (!_scratchResolutionByPillar.TryGetValue(pillarId, out var resolution))
            {
                var selectedBlockId = _sharedBlockId;
                if (selectedBlockId == InvalidBlockId)
                    return false;

                resolution = new ScratchResolutionState
                {
                    BlockId = selectedBlockId,
                    IsResolved = false
                };
            }

            if (resolution.IsResolved || resolution.BlockId != blockId)
            {
                _scratchResolutionByPillar[pillarId] = resolution;
                return false;
            }

            resolution.IsResolved = true;
            _scratchResolutionByPillar[pillarId] = resolution;
            return true;
        }

        private static int GetRandomBlockId()
        {
            if (_scratchedBlocks == null || _scratchedBlocks.Count == 0)
            {
                Debug.Log($"Invalid ID: {InvalidBlockId}");
                _sharedBlockId = InvalidBlockId;
                return InvalidBlockId;
            }

            if (_sharedBlockSelectionFrame != Time.frameCount ||
                !_scratchedBlocks.Any(block => block != null && block.Id == _sharedBlockId))
            {
                _sharedBlockSelectionFrame = Time.frameCount;
                _sharedBlockId = _scratchedBlocks.ElementAt(Random.Range(0, _scratchedBlocks.Count)).Id;
            }

            Debug.Log($"Get random ID: {_sharedBlockId}");
            return _sharedBlockId;
        }

        private static void EnsureSharedScratchBlockId()
        {
            if (_scratchedBlocks == null || _scratchedBlocks.Count == 0)
            {
                _sharedBlockId = InvalidBlockId;
                return;
            }

            if (_sharedBlockId == InvalidBlockId
                || !_scratchedBlocks.Any(block => block != null && block.Id == _sharedBlockId))
            {
                GetRandomBlockId();
            }
        }

        public override void Apply(IMechanicHandler target)
        {
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchBinding);
            EventBus<BlocksMatchedEvent>.Subscribe(_blockMatchBinding);
            _scratchedBlocks = BoardController.Instance.GetAllBlocks().Where(b => (b as IMechanicHandler).ActiveMechanic == EMechanic.ScratchBlock).ToHashSet();

            if (target is BlockController block)
                _scratchedBlocks.Add(block);

            EnsureSharedScratchBlockId();

            base.Apply(target);
        }

        public override void ApplyImmediate(IMechanicHandler target)
        {
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchBinding);
            EventBus<BlocksMatchedEvent>.Subscribe(_blockMatchBinding);
            _scratchedBlocks = BoardController.Instance.GetAllBlocks().Where(b => (b as IMechanicHandler).ActiveMechanic == EMechanic.ScratchBlock).ToHashSet();

            if (target is BlockController block)
                _scratchedBlocks.Add(block);

            EnsureSharedScratchBlockId();

            base.ApplyImmediate(target);
        }

        public override void Remove(bool doEffect = true)
        {
            var block = _target as BlockController;
            if (block != null)
                _scratchedBlocks?.Remove(block);

            GetRandomBlockId();
            EventBus<BlocksMatchedEvent>.Unsubscribe(_blockMatchBinding);
            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchBinding);
            base.Remove(doEffect);

            if (block != null)
            {
                var pillar = block.GetPillarParent();
                if (doEffect)
                {
                    (block as IMechanicHandler).TryRestoreRegisteredMechanic();
                    FrozenBlockMechanic.ReevaluateFrozenState(pillar, false);
                }

                pillar.CheckFullMatch();
                if (pillar.IsFullMatch) pillar.DoFullMatchAnim();
            } 
        }

        public override void RemoveImmediate(bool doEffect = true)
        {
            var block = _target as BlockController;
            if (block != null)
                _scratchedBlocks?.Remove(block);

            GetRandomBlockId();
            EventBus<BlocksMatchedEvent>.Unsubscribe(_blockMatchBinding);
            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchBinding);
            base.RemoveImmediate(doEffect);

            if (block != null)
            {
                var pillar = block.GetPillarParent();
                if (doEffect)
                {
                    (block as IMechanicHandler).TryRestoreRegisteredMechanic(true);
                    FrozenBlockMechanic.ReevaluateFrozenState(pillar, true);
                }

                pillar.CheckFullMatch();
            }
        }

        protected override bool CheckRemoveCondition()
        {
            return false;
        }
    }
    #endregion

    #region Sticky Block
    public class StickyBlockMechanic : MechanicRuntimeData
    {
        public StickyBlockMechanic()
        {
            Key = EMechanic.StickyBlock;
        }

        protected override bool CheckRemoveCondition()
        {
            return false;
        }

    }
    #endregion

    #region Trap Pillar
    public class TrapPillarMechanic : MechanicRuntimeData
    {
        private IMechanicHandler _lastTarget;
        private EventBinding<PillarFullMatchedEvent> _pillarFullMatchedBinding;
        public bool IsTrap {get; private set;}

        public TrapPillarMechanic(bool isTrap) : base()
        {
            Key = EMechanic.TrapPillar;
            IsTrap = isTrap;

            OnCheckCondicion = (e) =>
            {
                if (!e.MovedByPlayer) return;
                var isTrapSnapshot = IsTrap;
                IsTrap = !IsTrap;
                if (isTrapSnapshot) Remove();
                else Apply(_lastTarget);
            };
            _blocksMovedBinding = new(OnCheckCondicion);
            _pillarFullMatchedBinding = new(OnPillarFullMatched);
            EventBus<BlocksMovedEvent>.Subscribe(_blocksMovedBinding);
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchedBinding);
        }

        public override void Apply(IMechanicHandler target)
        {
            // Debug.Log($"Applying mechanic {Key} to {target}");
            _lastTarget = target;
            if (!IsTrap) 
            {
                if (target.ActiveMechanic == Key)
                {
                    _target = target;
                    Remove();
                }
                else
                {
                    target.MechanicVisual?.ShowTrapInactiveImmediate();
                }
                _target = null;
                return;
            }

            if (!TryPrepareTarget(target)) return;
            _target.UpdateMechanic(this);
        }

        public override void ApplyImmediate(IMechanicHandler target)
        {
            _lastTarget = target;
            if (!IsTrap)
            {
                if (target.ActiveMechanic == Key)
                {
                    _target = target;
                    RemoveImmediate();
                }
                else
                {
                    target.MechanicVisual?.ShowTrapInactiveImmediate();
                }
                _target = null;
                return;
            }

            if (!TryPrepareTarget(target)) return;
            _target.UpdateMechanicImmediate(this);
        }

        public override void Remove(bool doEffect = true)
        {
            if (!doEffect)
            {
                if (_target != null)
                {
                    var target = _target;
                    UnregisterCurrentTarget();
                    target.ClearMechanic(doEffect);
                }

                _target = null;
                _lastTarget = null;
                EventBus<BlocksMovedEvent>.Unsubscribe(_blocksMovedBinding);
                EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchedBinding);
                return;
            }

            // Debug.Log($"Removing mechanic {Key} from {_target}");
            if (_target == null) return;
            var currentTarget = _target;
            UnregisterCurrentTarget();
            currentTarget.ClearMechanic(doEffect);
            _target = null;

            // DoMechanicSFX(Key);
        }

        public override void RemoveImmediate(bool doEffect = true)
        {
            if (!doEffect)
            {
                if (_target != null)
                {
                    var target = _target;
                    UnregisterCurrentTarget();
                    target.ClearMechanicImmediate(doEffect);
                }

                _target = null;
                _lastTarget = null;
                EventBus<BlocksMovedEvent>.Unsubscribe(_blocksMovedBinding);
                EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchedBinding);
                return;
            }

            if (_target == null) return;
            var currentTarget = _target;
            UnregisterCurrentTarget();
            currentTarget.ClearMechanicImmediate(doEffect);
            _target = null;

            // DoMechanicSFX(Key);
        }


        protected override bool CheckRemoveCondition()
        {
            return true;
        }

        private void OnPillarFullMatched(PillarFullMatchedEvent evt)
        {
            var trapPillar = _lastTarget as PillarController ?? _target as PillarController;
            if (trapPillar == null || evt.Pillar != trapPillar)
                return;

            IsTrap = false;

            if (_target != null)
            {
                var target = _target;
                UnregisterCurrentTarget();
                target.ClearMechanic();
                _target = null;
            }
            else
            {
                trapPillar.MechanicVisual?.RemoveVisualImmediate(Key, false);
            }

            _lastTarget = null;
            EventBus<BlocksMovedEvent>.Unsubscribe(_blocksMovedBinding);
            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchedBinding);
        }
    }
    #endregion
}
