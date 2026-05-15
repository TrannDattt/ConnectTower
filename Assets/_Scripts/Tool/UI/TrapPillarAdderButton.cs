using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class TrapPillarAdderButton : MechanicAdderButton
    {
        [SerializeField] private Toggle _trapToggle;

        public TrapPillarAdderButton()
        {
        }

        protected override void AddMechanicIds(LevelJSON levelJSON)
        {
            foreach (var trapData in levelJSON.TrapPillarDatas)
            {
                _idInput.text = trapData.PillarId.ToString();
                _trapToggle.SetIsOnWithoutNotify(trapData.IsTrap);
                AddIdFromLevel();
            }
        }

        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            data = new TrapPillarMechanic(_trapToggle.isOn);
            return true;
        }

        protected override void ResetInputs()
        {
            base.ResetInputs();
            _trapToggle.SetIsOnWithoutNotify(false);
        }

        protected override void Start()
        {
            _mechanicType = EMechanic.TrapPillar;
            base.Start();
            _trapToggle.SetIsOnWithoutNotify(false);
        }
    }
}