using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using UnityEngine.Events;

namespace Assets._Scripts.Interfaces
{
    public interface IMechanicHandler
    {
        public EMechanic ActiveMechanic {get; set;}
        public bool IsHidden() => ActiveMechanic == EMechanic.HiddenBlock;
        public bool IsInteractable() => ActiveMechanic ==  EMechanic.None;
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