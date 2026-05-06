using System.Linq;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using UnityEngine.Events;

namespace Assets._Scripts.Interfaces
{
    public interface IMechanicHandler
    {
        public EMechanic ActiveMechanic {get; set;}
        private static readonly EMechanic[] HiddenMechanics = new EMechanic[]
        {
            EMechanic.HiddenBlock,
            EMechanic.CoveredPillar
        };

        private static readonly EMechanic[] UnmovableMechanics = new EMechanic[]
        {
            EMechanic.FrozenBlock,
            EMechanic.CoveredPillar
        };

        public bool IsHidden() => HiddenMechanics.Contains(ActiveMechanic);
        public bool IsMovable() => !UnmovableMechanics.Contains(ActiveMechanic);
        public bool IsInMechanic() => ActiveMechanic ==  EMechanic.None;
        public MechanicVisualControl MechanicVisual {get; set;}

        public void UpdateMechanic(MechanicRuntimeData mechanicData)
        {
            MechanicVisual.RemoveVisual(ActiveMechanic);
            ActiveMechanic = mechanicData.Key;
            MechanicVisual.ApplyVisual(mechanicData);
        }

        public void ClearMechanic(bool doEffect = true)
        {
            MechanicVisual.RemoveVisual(ActiveMechanic, doEffect);
            ActiveMechanic = EMechanic.None;
        }
    }
}