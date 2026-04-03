using Assets._Scripts.Controllers;
using Assets._Scripts.Enums;
using Assets._Scripts.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts.Datas
{
    public abstract class MechanicRuntimeData
    {
        public EMechanic Key {get; protected set;}
        protected IMechanicHandler _target;
        private UnityAction<bool> OnCheckCondicion;

        public MechanicRuntimeData()
        {
            OnCheckCondicion = (_) =>
            {
                if (CheckRemoveCondition())
                {
                    Remove();
                }
            };
        }

        protected abstract bool CheckRemoveCondition();
        public virtual void Apply(IMechanicHandler target)
        {
            Debug.Log($"Applying mechanic {Key} to {target}");
            _target = target;
            if (_target.ActiveMechanic == Key) return;
            _target.UpdateMechanic(this);
            
            BlockMovementController.Instance.OnBlocksMoved.AddListener(OnCheckCondicion);
            
            OnCheckCondicion?.Invoke(true);
        }
        public virtual void Remove()
        {
            Debug.Log($"Removing mechanic {Key} from {_target}");
            if (_target == null) return;
            _target.ClearMechanic();
            _target = null;
            
            BlockMovementController.Instance.OnBlocksMoved.RemoveListener(OnCheckCondicion);
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

        private UnityAction<string> OnCheckCondicion;

        public CoveredPillarMechanic(string tagToOpen) : base()
        {
            Key = EMechanic.CoveredPillar;
            TagToOpen = tagToOpen;

            OnCheckCondicion = (tag) =>
            {
                if (CheckRemoveCondition(tag))
                {
                    Remove();
                }
            };
        }

        public override void Apply(IMechanicHandler target)
        {
            _target = target;
            if (_target.ActiveMechanic == Key) return;
            _target.UpdateMechanic(this);
            
            var pillars = BoardController.Instance.GetAllPillars();
            pillars.ForEach(p => p.OnFullMatched.AddListener(OnCheckCondicion));
        }

        public override void Remove()
        {
            if (_target == null) return;
            _target.ClearMechanic();
            
            var pillars = BoardController.Instance.GetAllPillars();
            pillars.ForEach(p => p.OnFullMatched.RemoveListener(OnCheckCondicion));
            
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
            BlockMovementController.Instance.OnBlocksMoved.AddListener((moveByPlayer) =>
            {
                if (moveByPlayer)
                    _currentMoveCount++;
            });
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