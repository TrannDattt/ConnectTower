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

        public void OnAddedGroup(string name)
        {
            var groupData = BlockGroupMapper.GetGroupData(name);
            if (_blockGroupPrefab != null && _blockGroupParent != null)
            {
                // Debug.Log($"Get 4 {name} icons from pool with {icons.Count}");
                var newGroup = Instantiate(_blockGroupPrefab, _blockGroupParent);
                newGroup.InitGroup(LevelEditor.GetLastBlockId() + 1, groupData);
                LevelEditor.AddBlockGroup(newGroup, true);
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
        }

        public void AddBlockGroupsFromJson(LevelJSON levelJSON)
        {
            foreach (var blockGroup in levelJSON.BlockGroups)
            {
                if (!blockGroup.Trackable) continue;
                var newGroup = Instantiate(_blockGroupPrefab, _blockGroupParent);
                var groupData = BlockGroupMapper.GetGroupData(blockGroup.Tag);
                if (groupData != null)
                    newGroup.InitGroup(LevelEditor.GetLastBlockId() + 1, groupData);
                else
                    newGroup.InitGroup(blockGroup.BlockDatas[0].Id, blockGroup.Tag, new Sprite[] {null, null, null, null});
            }
        }

        void Start()
        {
            // _groupDropdownSelector.OnGroupSelected.AddListener(OnAddedGroup);
            // _addBlockGroupButton.onClick.AddListener(() => _groupDropdownSelector.Show());
            _addBlockGroupButton.onClick.AddListener(() => OnAddedGroup(""));

            LevelEditor.OnLevelCleared.AddListener(RemoveAllGroups);
            LevelEditor.OnLevelLoaded.AddListener(AddBlockGroupsFromJson);
        }
    }
}