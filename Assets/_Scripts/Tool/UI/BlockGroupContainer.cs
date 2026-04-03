using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class BlockGroupContainer : MonoBehaviour
    {
        [SerializeField] private Transform _blockGroupParent;
        [SerializeField] private BlockGroup _blockGroupPrefab;
        [SerializeField] private Button _addBlockGroupButton;
        [SerializeField] private GroupDropdownSelector _groupDropdownSelector;
        
        private int _lastBlockId = -1;

        public void OnAddedGroup(string name)
        {
            var icons = BlockGroupMapper.GetGroupIcons(name);
            if (_blockGroupPrefab != null && _blockGroupParent != null)
            {
                var newGroup = Instantiate(_blockGroupPrefab, _blockGroupParent);
                newGroup.InitGroup(_lastBlockId, name, icons.Select(i => i.name).ToArray());
                _lastBlockId += 4; 
                LevelEditor.AddBlockGroup(newGroup);
            }
            else
            {
                Debug.LogWarning("Block group prefab or parent is not assigned.");
            }
        }

        public void RemoveAllGroups()
        {
            var groups = _blockGroupParent.GetComponentsInChildren<BlockGroup>();
            foreach (var group in groups)
            {
                Destroy(group.gameObject);
            }
            _lastBlockId = -1;
        }

        public void AddBlockGroups(LevelJSON levelJSON)
        {
            foreach (var blockGroup in levelJSON.BlockGroups)
            {
                var newGroup = Instantiate(_blockGroupPrefab, _blockGroupParent);
                newGroup.InitGroup(blockGroup.BlockDatas[0].Id - 1, blockGroup.Tag, blockGroup.BlockDatas.Select(b => b.IconId).ToArray());
            }
            _lastBlockId = levelJSON.BlockGroups.SelectMany(g => g.BlockDatas).Count() > 0 ? levelJSON.BlockGroups.SelectMany(g => g.BlockDatas).Max(b => b.Id) : -1;
        }

        void Start()
        {
            _groupDropdownSelector.OnGroupSelected.AddListener(OnAddedGroup);
            _addBlockGroupButton.onClick.AddListener(() => _groupDropdownSelector.Show());

            LevelEditor.OnLevelCleared.AddListener(RemoveAllGroups);
            LevelEditor.OnLevelLoaded.AddListener(AddBlockGroups);
        }
    }
}