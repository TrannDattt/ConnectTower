using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Interfaces;
using Assets._Scripts.Managers;
using Assets._Scripts.Patterns.EventBus;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Datas
{
    public abstract class MechanicRuntimeData
    {
        public EMechanic Key {get; protected set;}
        protected IMechanicHandler _target;
        private UnityAction<BlocksMovedEvent> OnCheckCondicion;

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

            var mechanicSFX = Key switch
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

        private UnityAction<PillarFullMatchedEvent> OnCheckCondicion;
        private EventBinding<PillarFullMatchedEvent> _pillarFullMatchedBinding;

        public CoveredPillarMechanic(string tagToOpen) : base()
        {
            Key = EMechanic.CoveredPillar;
            TagToOpen = tagToOpen;

            OnCheckCondicion = (evt) =>
            {
                if (CheckRemoveCondition(evt.Tag))
                {
                    Remove();
                }
            };

            _pillarFullMatchedBinding = new(OnCheckCondicion);
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

        // protected override void DoApplyAnim(IMechanicHandler target)
        // {
        //     throw new System.NotImplementedException();
        // }

        // protected override void DoRemoveAnim(IMechanicHandler target)
        // {
        //     throw new System.NotImplementedException();
        // }
    }
#endregion

#region Frozen Block
    public class FrozenBlockMechanic : MechanicRuntimeData
    {
        public int MoveCountToRemove {get; private set;}
        private int _currentMoveCount = 0;

        public FrozenBlockMechanic(int moveCountToRemove) : base()
        {
            Key = EMechanic.FrozenBlock;
            MoveCountToRemove = moveCountToRemove;
            // BlockMovementController.Instance.OnBlocksMoved.AddListener((moveByPlayer) =>
            // {
            //     if (moveByPlayer)
            //         _currentMoveCount++;
            // });
        }

        protected override bool CheckRemoveCondition()
        {
            return _currentMoveCount >= MoveCountToRemove;
        }

        // protected override void DoApplyAnim(IMechanicHandler target)
        // {
        //     throw new System.NotImplementedException();
        // }

        // protected override void DoRemoveAnim(IMechanicHandler target)
        // {
        //     throw new System.NotImplementedException();
        // }
    }
#endregion
}