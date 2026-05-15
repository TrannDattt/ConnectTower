using Assets._Scripts.Datas;

namespace Assets._Scripts.Tools.UI
{
    public class ScratchedBlockAdderButton : MechanicAdderButton
    {
        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            data = new ScratchedBlockMechanic();
            return true;
        }

        protected override void AddMechanicIds(LevelJSON levelJSON)
        {
            foreach (var id in levelJSON.ScratchedBlockDatas.BlockIds)
            {
                _idInput.text = id.ToString();
                AddIdFromLevel();
            }
        }

        protected override void Start()
        {
            _mechanicType = Enums.EMechanic.ScratchBlock;
            base.Start();
        }
    }
}