using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets._Scripts.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class IconSelectorPopup : MonoBehaviour
    {
        [SerializeField] private Transform _buttonParent;
        [SerializeField] private Button _iconPrefab;
        [SerializeField] private Button _closeButton;

        private Action<Sprite> _onIdSelected;
        private bool _isOpen = false;
        private CanvasGroup _canvasGroup;
        private LayoutGroup _layoutGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _layoutGroup = _buttonParent != null ? _buttonParent.GetComponent<LayoutGroup>() : null;
            
            // Start hidden
            TogglePopup(false);
        }

        public async void Show(Action<Sprite> onIdSelected)
        {
            if (_isOpen) return;
            _isOpen = true;
            _onIdSelected = onIdSelected;
            
            // Show immediately via alpha
            TogglePopup(true);
            
            await RefreshListAsync();
        }

        public async Task<List<Sprite>> GetUnassignedIconAsync()
        {
            var blocks = LevelEditor.BlockDatas;
            if (blocks == null) return new List<Sprite>();

            var assignedIconNames = new HashSet<string>();
            foreach (var block in blocks)
            {
                if (block != null && !string.IsNullOrEmpty(block.IconId))
                {
                    assignedIconNames.Add(block.IconId);
                }
            }

            var allIcons = await BlockIconMapper.GetAllIconsAsync();
            var unassignedIcons = allIcons;
            // var unassignedIcons = new List<Sprite>();

            // foreach (var icon in allIcons)
            // {
            //     if (!assignedIconNames.Contains(icon.name))
            //     {
            //         unassignedIcons.Add(icon);
            //     }
            // }

            return unassignedIcons;
        }

        private async Task RefreshListAsync()
        {
            if (_buttonParent == null || _iconPrefab == null) return;

            // Prevent layout rebuilds for every minor change
            if (_layoutGroup != null) _layoutGroup.enabled = false;

            var unassignedIcons = await GetUnassignedIconAsync();
            int neededCount = unassignedIcons.Count;
            int currentChildCount = _buttonParent.childCount;

            for (int i = currentChildCount; i < neededCount; i++)
            {
                Instantiate(_iconPrefab, _buttonParent);
            }

            for (int i = 0; i < _buttonParent.childCount; i++)
            {
                var child = _buttonParent.GetChild(i);
                if (i < neededCount)
                {
                    child.gameObject.SetActive(true);
                    var button = child.GetComponent<Button>();
                    var image = child.GetComponent<Image>();
                    
                    button.onClick.RemoveAllListeners();
                    
                    int index = i;
                    button.onClick.AddListener(() => SelectIcon(unassignedIcons[index]));
                    
                    if (image != null)
                    {
                        image.sprite = unassignedIcons[index];
                    }
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }

            // Re-enable layout and force one single rebuild
            if (_layoutGroup != null)
            {
                _layoutGroup.enabled = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonParent as RectTransform);
            }
        }

        private void SelectIcon(Sprite icon)
        {
            _onIdSelected?.Invoke(icon);
            Close();
        }

        public void Close()
        {
            if (!_isOpen) return;
            _onIdSelected = null;
            TogglePopup(false);
            _isOpen = false;
        }

        private void TogglePopup(bool show)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.alpha = show ? 1f : 0f;
            _canvasGroup.blocksRaycasts = show;
            _canvasGroup.interactable = show;
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
