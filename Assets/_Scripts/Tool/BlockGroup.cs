using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Helpers;
using Assets._Scripts.Patterns.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Tools
{
    public class BlockGroup : MonoBehaviour
    {
        [SerializeField] private Dropdown _groupNameDD;
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
        // public bool Trackable {get; private set;} = true;

        [field: SerializeField] public UnityEvent OnGroupRemoved { get; private set; } = new();

        public void InitGroup(int startId, string tag, Sprite[] icons)
        {
            GroupTag = tag;
            var option = _groupNameDD.options.FirstOrDefault(o => string.Equals(o.text, tag));
            if (option == null)
            {
                option = new (tag);
                Debug.Log($"Add new option: {tag}");
                _groupNameDD.options.Add(option);
            }

            Debug.Log($"Set tag to {tag}");
            Debug.Log($"Index of {tag} is: {_groupNameDD.options.IndexOf(option)}");
            _groupNameDD.SetValueWithoutNotify(_groupNameDD.options.IndexOf(option));
            _groupNameDD.RefreshShownValue();

            for (int i = 0; i < _blocks.Length; i++)
            {
                _blocks[i].InitBlock(startId + i, icons[i]);
                // Debug.Log($"Init block with id: {_blocks[i].BlockId}");
            }
        }

        public void InitGroup(int startId, BlockGroupSO data)
        {
            data = data != null ? data : BlockGroupMapper.GetGroupData(_groupNameDD.options[0].text);
            var tag = data.Name;
            // Trackable = trackable;
            var icons = new List<Sprite>(data.Icons);
            GroupTag = tag;
            var option = _groupNameDD.options.FirstOrDefault(o => string.Equals(o.text, tag));
            if (option == null)
            {
                option = new (tag);
                Debug.Log($"Add new option: {tag}");
                _groupNameDD.options.Add(option);
            }

            Debug.Log($"Set tag to {tag}");
            Debug.Log($"Index of {tag} is: {_groupNameDD.options.IndexOf(option)}");
            _groupNameDD.SetValueWithoutNotify(_groupNameDD.options.IndexOf(option));
            _groupNameDD.RefreshShownValue();

            for (int i = 0; i < _blocks.Length; i++)
            {
                if (icons == null || icons.Count == 0) break;
                var randomIcon = icons[Random.Range(0, icons.Count)];
                icons.Remove(randomIcon);
                _blocks[i].InitBlock(startId + i, randomIcon);
                // Debug.Log($"Init block with id: {_blocks[i].BlockId}");
            }

            if (data == null) LevelEditor.AddBlockGroup(this, true);
        }

        public void RemoveGroup()
        {
            gameObject.SetActive(false);
            OnGroupRemoved.Invoke();
            LevelEditor.RemoveBlockGroup(this);
            EventBus<IEditorChangeBlockGroup>.Publish(new IEditorChangeBlockGroup { From = GroupTag, To = "" });
            Destroy(gameObject);
        }

        private void OnOptionsChanged(IEditorChangeBlockGroup @event)
        {
            if (_groupNameDD == @event.Base) return;
            
            if (!string.IsNullOrEmpty(@event.To))
            {
                var toRemove = _groupNameDD.options.FirstOrDefault(o => string.Equals(o.text, @event.To));
                if (toRemove != null) _groupNameDD.options.Remove(toRemove);
            }

            if (!string.IsNullOrEmpty(@event.From))
            {
                var toAdd = _groupNameDD.options.FirstOrDefault(o => string.Equals(o.text, @event.From));
                if (toAdd == null) _groupNameDD.options.Add(new (@event.From));
            }
        }

        private EventBinding<IEditorChangeBlockGroup> _groupChangedBinding;

        void Awake()
        {
            if (_removeGroupButton != null)
            {
                _removeGroupButton.onClick.AddListener(RemoveGroup);
            }

            var groupNames = BlockGroupMapper.GetAllGroups().Where(s => !LevelEditor.GroupTags.Contains(s)).ToList();
            _groupNameDD.ClearOptions();
            _groupNameDD.AddOptions(groupNames);

            _groupNameDD.onValueChanged.AddListener((value) =>
            {
                var tag = _groupNameDD.options[value].text;
                var icons = new List<Sprite>(BlockGroupMapper.GetGroupIcons(tag));
                // Debug.Log($"Get 4 {tag} icons from pool with {icons.Count}");
                for(int i = 0; i < _blocks.Length; i++)
                {
                    var randomIcon = icons[Random.Range(0, icons.Count)];
                    _blocks[i].ChangeIcon(randomIcon);
                    icons.Remove(randomIcon);
                }

                LevelEditor.UpdateBlockGroupData(this, new Datas.BlockGroup
                {
                    Tag = tag,
                    BlockDatas = Blocks,
                    Trackable = true
                });
                EventBus<IEditorChangeBlockGroup>.Publish(new IEditorChangeBlockGroup { Base = _groupNameDD, From = GroupTag, To = tag });
                GroupTag = tag;
            });

            _groupChangedBinding = new(OnOptionsChanged);
            EventBus<IEditorChangeBlockGroup>.Subscribe(_groupChangedBinding);
        }

        void OnDestroy()
        {
            EventBus<IEditorChangeBlockGroup>.Unsubscribe(_groupChangedBinding);
        }
    }

    public struct IEditorChangeBlockGroup : IEvent
    {
        public Dropdown Base;
        public string From;
        public string To;
    }
}
