using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Tools.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Tools
{
    public class Pillar : MonoBehaviour
    {
        public int PillarId { get; private set; }
        [SerializeField] private TextMeshProUGUI _pillarIdText;
        [SerializeField] private AddBlockButton[] _selectBlockButtons = new AddBlockButton[4];
        private int[] _blockIds = new int[] { -1, -2, -3, -4 };
        public int[] BlockIds => _blockIds;
        [SerializeField] private Button _removePillarButton;

        [field: SerializeField] public UnityEvent OnPillarRemoved {get; private set;} = new();

        public void SetPillarId(int pillarId)
        {
            PillarId = pillarId;
            if (_pillarIdText != null)
            {
                _pillarIdText.text = $"Index: {PillarId}";
            }
        }

        public void AddBlockId(int blockId, int position)
        {
            if (position >= 0 && position < _blockIds.Length)
            {
                _blockIds[position] = blockId;
                LevelEditor.UpdatePillarData(PillarId, new PillarData
                {
                    Id = PillarId,
                    BlockIds = _blockIds.ToHashSet()
                });
            }
            else
            {
                Debug.LogWarning($"Invalid position {position}. Cannot add block.");
            }
        }

        public void RemoveBlockId(int position)
        {
            if (position >= 0 && position < _blockIds.Length)
            {
                _blockIds[position] = -position - 1;
                LevelEditor.UpdatePillarData(PillarId, new PillarData
                {
                    Id = PillarId,
                    BlockIds = _blockIds.ToHashSet()
                });
            }
            else
            {
                Debug.LogWarning("Invalid position. Cannot remove block.");
            }
        }

        public void ClearPillar()
        {
            for (int i = 0; i < _blockIds.Length; i++)
            {
                _blockIds[i] = -1 - i;
            }
        }

        public void RemovePillar()
        {
            LevelEditor.RemovePillar(PillarId);
            gameObject.SetActive(false);
            OnPillarRemoved.Invoke();
            Destroy(gameObject);
        }

        void Start()
        {
            _removePillarButton.onClick.AddListener(RemovePillar);

            var popup = FindFirstObjectByType<BlockSelectorPopup>(FindObjectsInactive.Include);

            for (int i = 0; i < _selectBlockButtons.Length; i++)
            {
                if (_selectBlockButtons[i] == null) continue;
                
                int index = i;
                _selectBlockButtons[index].SetId(-1 - index);

                _selectBlockButtons[index].AddButton.onClick.AddListener(() =>
                {
                    if (popup != null)
                    {
                        popup.Show(id =>
                        {
                            AddBlockId(id, index);
                            _selectBlockButtons[index].SetId(id);
                        });
                    }
                });

                _selectBlockButtons[index].RemoveButton.onClick.AddListener(() =>
                {
                    RemoveBlockId(index);
                    _selectBlockButtons[index].SetId(-1 - index);
                });
            }
        }
    }
}