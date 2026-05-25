using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using UnityEngine;

namespace Assets._Scripts.Tools.UI
{
    public class FrozenBlockAdderButton : MechanicAdderButton
    {
        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            data = new FrozenBlockMechanic();
            return true;
        }

        protected override void AddMechanicIds(LevelJSON levelJSON)
        {
            foreach (var fbm in levelJSON.FrozenBlockDatas)
            {
                _idInput.text = fbm.BlockIds.ToString();
                AddIdFromLevel();
            }
        }

        protected override void Start()
        {
            _mechanicType = EMechanic.FrozenBlock;
            base.Start();
        }
    }
}
