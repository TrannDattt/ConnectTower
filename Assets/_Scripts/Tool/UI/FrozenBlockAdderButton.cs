using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class FrozenBlockAdderButton : MechanicAdderButton
    {
        [SerializeField] private TMP_InputField _moveCountInput;
        protected EMechanic _mechanicType = EMechanic.FrozenBlock;

        override protected void ResetInputs()
        {
            base.ResetInputs();
            _moveCountInput.text = "";
        }

        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            if (int.TryParse(_moveCountInput.text.Trim(), out int moveCount))
            {
                data = new FrozenBlockMechanic(moveCount);
                return true;
            }
            data = null;
            return false;
        }

        protected override void AddMechanicIds(LevelJSON levelJSON)
        {
            foreach (var fbm in levelJSON.FrozenBlockDatas)
            {
                _idInput.text = fbm.BlockIds.ToString();
                _moveCountInput.text = fbm.MoveCountToRemove.ToString();
                AddId();
            }
        }

        protected override void Start()
        {
            _mechanicType = EMechanic.FrozenBlock;
            base.Start();
        }
    }

}