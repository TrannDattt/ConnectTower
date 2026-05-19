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
        public EMechanic Key {get; protected set;}
        protected IMechanicHandler _target;
        protected UnityAction<BlocksMovedEvent> OnCheckCondicion;

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
        public virtual void Apply(IMechanicHandler target)
        {
            Debug.Log($"Applying mechanic {Key} to {target}");
            _target = target;
            if (_target.ActiveMechanic == Key) return;
            _target.UpdateMechanic(this);
            
            EventBus<BlocksMovedEvent>.Subscribe(_blocksMovedBinding);
            
            OnCheckCondicion?.Invoke(new BlocksMovedEvent { MovedByPlayer = false });
        }

        public virtual void Remove(bool doEffect = true)
        {
            Debug.Log($"Removing mechanic {Key} from {_target}");
            if (_target == null) return;
            _target.ClearMechanic(doEffect);
            _target = null;
            
            EventBus<BlocksMovedEvent>.Unsubscribe(_blocksMovedBinding);
            if (!doEffect) return;
            DoMechanicSFX(Key);
        }

        protected void DoMechanicSFX(EMechanic key)
        {
            var mechanicSFX = key switch
            {
                EMechanic.HiddenBlock => ESfx.HiddenBlockExit,
                EMechanic.CoveredPillar => ESfx.CoveredPillarExit,
                EMechanic.FrozenBlock => ESfx.FrozenBlockExit,
                _ => ESfx.None
            };
            SoundManager.Instance.PlayRandomSFX(mechanicSFX);
        }
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
            _target = target;
            if (_target.ActiveMechanic == Key) return;
            _target.UpdateMechanic(this);
            
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchedBinding);
        }

        public override void Remove(bool doEffect = true)
        {
            if (_target == null) return;
            _target.ClearMechanic();
            
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
        public int MoveCountToRemove {get; private set;}
        private int _currentMoveCount = 0;
        private EventBinding<PillarFullMatchedEvent> _pillarFullMatchedBinding;

        public FrozenBlockMechanic(int moveCountToRemove) : base()
        {
            Key = EMechanic.FrozenBlock;
            MoveCountToRemove = moveCountToRemove;
            _pillarFullMatchedBinding = new((evt) =>
            {
                if (_target is PillarController pillar)
                {
                    if (evt.Pillar == pillar) Remove();
                }
                else if (_target is BlockController block)
                {
                    if (block.IsSameTag(evt.Tag)) Remove();
                }
            });
        }

        public override void Apply(IMechanicHandler target)
        {
            base.Apply(target);
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchedBinding);
        }

        public override void Remove(bool doEffect = true)
        {
            EventBus<PillarFullMatchedEvent>.Unsubscribe(_pillarFullMatchedBinding);
            base.Remove(doEffect);
        }

        protected override bool CheckRemoveCondition()
        {
            return _currentMoveCount >= MoveCountToRemove;
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
        private static readonly Dictionary<int, ScratchResolutionState> _scratchResolutionByPillar = new();
        private static bool _isResolvingScratchRemoval;
        public int ScratchState {get; private set;}
        private EventBinding<BlocksMatchedEvent> _blockMatchBinding;

        private EventBinding<PillarFullMatchedEvent> _pillarFullMatchBinding;

        public ScratchedBlockMechanic() : base()
        {
            Key = EMechanic.ScratchBlock;
            _pillarFullMatchBinding = new((evt) =>
            {
                if (_isResolvingScratchRemoval || !(_target is BlockController block) || evt.Pillar == null)
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
            _blocksMovedBinding = new(() => {});
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

        public override void Apply(IMechanicHandler target)
        {
            EventBus<PillarFullMatchedEvent>.Subscribe(_pillarFullMatchBinding);
            EventBus<BlocksMatchedEvent>.Subscribe(_blockMatchBinding);
            _scratchedBlocks = BoardController.Instance.GetAllBlocks().Where(b => (b as IMechanicHandler).ActiveMechanic == EMechanic.ScratchBlock).ToHashSet();

            if (target is BlockController block)
                _scratchedBlocks.Add(block);

            base.Apply(target);
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
                pillar.CheckFullMatch();
                if (pillar.IsFullMatch) pillar.DoFullMatchAnim();
            } 
        }

        protected override bool CheckRemoveCondition()
        {
            return _target is BlockController block && block.Id == _sharedBlockId;
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
        public bool IsTrap {get; private set;}

        public TrapPillarMechanic(bool isTrap) : base()
        {
            Key = EMechanic.TrapPillar;
            IsTrap = isTrap;

            OnCheckCondicion = (_) =>
            {
                var isTrapSnapshot = IsTrap;
                IsTrap = !IsTrap;
                if (isTrapSnapshot) Remove();
                else Apply(_lastTarget);
            };
            _blocksMovedBinding = new(OnCheckCondicion);
            EventBus<BlocksMovedEvent>.Subscribe(_blocksMovedBinding);
        }

        public override void Apply(IMechanicHandler target)
        {
            Debug.Log($"Applying mechanic {Key} to {target}");
            _lastTarget = target;
            _target = target;
            if (_target.ActiveMechanic == Key) return;
            _target.UpdateMechanic(this);
            
            if (!IsTrap) Remove();
        }

        public override void Remove(bool doEffect = true)
        {
            Debug.Log($"Removing mechanic {Key} from {_target}");
            if (_target == null) return;
            _target.ClearMechanic(doEffect);
            _target = null;
            
            if (!doEffect) return;
            DoMechanicSFX(Key);
        }


        protected override bool CheckRemoveCondition()
        {
            return true;
        }
    }
    #endregion
}
