using System.Linq;
using Assets._Scripts.Datas;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class BlockGroupContainer : MonoBehaviour
    {
        [SerializeField] private Transform _blockGroupParent;
        [SerializeField] private BlockGroup _blockGroupPrefab;
        [SerializeField] private Button _addBlockGroupButton;
        
        private int _lastBlockId = -1;

        public void OnAddBlockGroupClicked()
        {
            if (_blockGroupPrefab != null && _blockGroupParent != null)
            {
                var newGroup = Instantiate(_blockGroupPrefab, _blockGroupParent);
                newGroup.InitGroup(_lastBlockId);
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
                newGroup.InitGroup(_lastBlockId, blockGroup.Tag, blockGroup.BlockDatas.Select(b => b.IconId).ToArray());
                _lastBlockId += 4; 
            }
        }

        void Start()
        {
            _addBlockGroupButton.onClick.AddListener(OnAddBlockGroupClicked);

            LevelEditor.OnLevelCleared.AddListener(RemoveAllGroups);
            LevelEditor.OnLevelLoaded.AddListener(AddBlockGroups);
        }
    }
}