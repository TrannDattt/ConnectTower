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
                if (fbm?.BlockIds == null) continue;

                foreach (var id in fbm.BlockIds)
                {
                    AddIdFromLevel(id);
                }
            }
        }

        private void AddIdFromLevel(int id)
        {
            var newIdButton = Instantiate(_idDisplayPrefab, _idContainer);
            newIdButton.SetId(id);
            newIdButton.OnRemoveClicked.AddListener(RemoveId);
        }

        protected override void Start()
        {
            _mechanicType = EMechanic.FrozenBlock;
            base.Start();
        }
    }
}
