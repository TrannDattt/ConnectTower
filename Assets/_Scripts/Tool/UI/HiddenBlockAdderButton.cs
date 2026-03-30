using Assets._Scripts.Datas;
using Assets._Scripts.Enums;

namespace Assets._Scripts.Tools.UI
{
    public class HiddenBlockAdderButton : MechanicAdderButton
    {
        protected EMechanic _mechanicType = EMechanic.HiddenBlock;

        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            data = new HiddenBlockMechanic();
            return true;
        }

        protected override void Start()
        {
            _mechanicType = EMechanic.HiddenBlock;
            base.Start();
        }
    }
}