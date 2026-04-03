using Assets._Scripts.Datas;
using Assets._Scripts.Enums;

namespace Assets._Scripts.Tools.UI
{
    public class HiddenBlockAdderButton : MechanicAdderButton
    {
        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            data = new HiddenBlockMechanic();
            return true;
        }

        protected override void AddMechanicIds(LevelJSON levelJSON)
        {
            foreach (var id in levelJSON.HiddenBlockDatas.BlockIds)
            {
                _idInput.text = id.ToString();
                AddIdFromLevel();
            }
        }

        protected override void Start()
        {
            _mechanicType = EMechanic.HiddenBlock;
            base.Start();
        }
    }
}