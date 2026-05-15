using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using UnityEngine;

namespace Assets._Scripts.Tools.UI
{
    public class StickyBlockAdderButton : MechanicAdderButton
    {
        protected override void AddMechanicIds(LevelJSON levelJSON)
        {
            foreach (var id in levelJSON.StickyBlockDatas.BlockIds)
            {
                AddIdFromLevel(id);
            }
        }

        protected override void AddIdToLevel()
        {
            int id = LevelEditor.GetLastBlockId() + 1;
            if (id == -1) return;

            if (MechanicData == null) return;
            LevelEditor.AddBlockGroup(new Datas.BlockGroup
            {
                Tag = "",
                BlockDatas = new List<BlockData> { new() { Id = id, IconId = ""}},
                Trackable = false 
            });

            if(LevelEditor.TryAddMechanic(id, MechanicData))
            {
                var newIdButton = Instantiate(_idDisplayPrefab, _idContainer);
                newIdButton.SetId(id);
                newIdButton.OnRemoveClicked.AddListener(RemoveId);
            }
            else
            {
                Debug.Log($"Id {id} already has mechanic {_mechanicType}");
            }
        }

        private void AddIdFromLevel(int id)
        {
            // Debug.Log($"Add sticky block with id {id}");
            var newIdButton = Instantiate(_idDisplayPrefab, _idContainer);
            newIdButton.SetId(id);
            newIdButton.OnRemoveClicked.AddListener(RemoveId);
        }

        protected override bool TryGetMechanicData(out MechanicRuntimeData data)
        {
            data = new StickyBlockMechanic();
            return true;
        }

        protected override void RemoveId(int id)
        {
            base.RemoveId(id);
            LevelEditor.RemoveBlockGroup(id);
        }

        protected override void Start()
        {
            _mechanicType = Enums.EMechanic.StickyBlock;
            base.Start();
        }
    }
}