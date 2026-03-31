using System.Collections.Generic;
using Assets._Scripts.Datas;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Tools
{
    public class BlockGroup : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _groupNameInput;
        [SerializeField] private Block[] _blocks = new Block[4];
        [SerializeField] private Button _removeGroupButton;

        public string GroupTag {get; private set;} = "";
        public List<BlockData> Blocks
        {
            get
            {
                List<BlockData> blockDatas = new List<BlockData>();
                foreach (var block in _blocks)
                {
                    blockDatas.Add(new BlockData
                    {
                        Id = block.BlockId,
                        IconId = block.IconId
                    });
                }
                return blockDatas; 
            }
        }

        [field: SerializeField] public UnityEvent OnGroupRemoved { get; private set; } = new();


        public void InitGroup(int lastBlockId, string tag = "", params string[] iconIds)
        {
            GroupTag = tag;
            _groupNameInput.text = tag;

            for (int i = 0; i < _blocks.Length; i++)
            {
                string iconId = (iconIds != null && i < iconIds.Length) ? iconIds[i] : "";
                _blocks[i].InitBlock(lastBlockId + 1, iconId);
                lastBlockId++;
            }
        }

        public void RemoveGroup()
        {
            gameObject.SetActive(false);
            OnGroupRemoved.Invoke();
            LevelEditor.RemoveBlockGroup(this);
            Destroy(gameObject);
        }

        void Start()
        {
            if (_removeGroupButton != null)
            {
                _removeGroupButton.onClick.AddListener(RemoveGroup);
            }

            _groupNameInput.onEndEdit.AddListener((value) =>
            {
                LevelEditor.UpdateBlockGroupData(this, new Datas.BlockGroup
                {
                    Tag = value,
                    BlockDatas = Blocks
                });
                GroupTag = value;
            });
        }
    }
}