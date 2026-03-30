using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Assets._Scripts.Tools.UI
{
    public class BlockSelectorPopup : MonoBehaviour
    {
        [SerializeField] private Transform _buttonParent;
        [SerializeField] private Button _idButtonPrefab;
        [SerializeField] private Button _closeButton;

        private Action<int> _onIdSelected;
        private bool _isOpen = false;

        public void Show(Action<int> onIdSelected)
        {
            if (_isOpen) return;
            _onIdSelected = onIdSelected;
            gameObject.SetActive(true);
            _isOpen = true;
            RefreshList();
        }

        public List<int> GetUnassignedId()
        {
            var blockIds = LevelEditor.BlockDatas.Select(block => block.Id).ToList();
            // Debug.Log($"Total block in LevelEditor: {LevelEditor.BlockDatas.Count}");
            // Debug.Log($"Found {blockIds.Count} block IDs in LevelEditor: {string.Join(", ", blockIds)}");
            if (blockIds == null) return new List<int>();

            HashSet<int> assignedIds = new HashSet<int>();
            var allPillars = FindObjectsByType<Pillar>(FindObjectsSortMode.None);
            
            foreach (var pillar in allPillars)
            {
                if (pillar == null || pillar.BlockIds == null) continue;
                foreach (int id in pillar.BlockIds)
                {
                    if (id < 0) continue;
                    assignedIds.Add(id);
                }
                // Debug.Log($"Pillar {pillar.PillarId} has block IDs: {string.Join(", ", pillar.BlockIds)}");
            }
            return blockIds.Where(id => !assignedIds.Contains(id)).ToList();
        }

        private void RefreshList()
        {
            if (_buttonParent == null || _idButtonPrefab == null) return;

            // Clear existing buttons
            var children = _buttonParent.GetComponentsInChildren<Button>();
            foreach (var child in children)
            {
                Destroy(child.gameObject);
            }

            // Populate with current BlockIds from LevelDataManager
            foreach (var blockId in GetUnassignedId())
            {
                var newButton = Instantiate(_idButtonPrefab, _buttonParent);
                var text = newButton.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null) text.text = blockId.ToString();
                
                newButton.onClick.AddListener(() => 
                {
                    SelectId(blockId);
                });
            }
        }

        private void SelectId(int id)
        {
            _onIdSelected?.Invoke(id);
            Close();
        }

        public void Close()
        {
            if (!_isOpen) return;
            _onIdSelected = null;
            gameObject.SetActive(false);
            _isOpen = false;
        }

        void Start()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Close);
            }
        }
    }
}
