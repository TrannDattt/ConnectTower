using System;
using Assets._Scripts.Datas;
using Assets._Scripts.Helpers;
using Assets._Scripts.Tools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools
{
    public class Block : MonoBehaviour
    {
        public int BlockId { get; private set; }
        public string IconId { get; private set; }
        [SerializeField] private TextMeshProUGUI _blockIdText;
        [SerializeField] private Button _changeIconButton;
        [SerializeField] private Image _blockIcon;

        public void InitBlock(int blockId, Sprite icon)
        {
            BlockId = blockId;
            IconId = icon.name;
            if (_blockIdText != null)
            {
                _blockIdText.text = $"{BlockId}";
            }
            
            if (!string.IsNullOrEmpty(IconId))
            {
                ChangeIcon(icon);
            }
        }

        public void ChangeIcon(Sprite icon)
        {
            IconId = icon.name;
            if (_blockIcon != null)
            {
                _blockIcon.sprite = icon;
            }
        }

        void Start()
        {
            var iconSelector = FindFirstObjectByType<IconSelectorPopup>(FindObjectsInactive.Include);
            _changeIconButton.onClick.AddListener(() =>
            {
                iconSelector.Show(selectedIcon =>
                {
                    ChangeIcon(selectedIcon);
                    LevelEditor.UpdateBlockData(BlockId, new BlockData
                    {
                        Id = BlockId,
                        IconId = IconId
                    });
                });
            });
        }
    }
}