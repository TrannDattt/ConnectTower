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

        void Start()
        {
            _addBlockGroupButton.onClick.AddListener(OnAddBlockGroupClicked);
        }
    }
}