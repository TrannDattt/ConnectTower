using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class CoveredPillarAdderButton : MechanicAdderButton
    {
        [SerializeField] private TMP_InputField _tagInput;

        protected EMechanic _mechanicType = EMechanic.CoveredPillar;

        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            var tag = _tagInput.text.Trim();
            if (string.IsNullOrEmpty(tag))
            {
                Debug.Log("Tag cannot be empty for Covered Pillar mechanic.");
                data = null;
                return false;
            }

            data = new CoveredPillarMechanic(tag);
            return true;
        }

        protected override void Start()
        {
            _mechanicType = EMechanic.CoveredPillar;
            base.Start();
        }
    }
}