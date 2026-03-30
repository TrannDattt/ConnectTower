using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class AddBlockButton : MonoBehaviour
    {
        [field: SerializeField] public Button AddButton {get; private set;}
        [field: SerializeField] public Button RemoveButton {get; private set;}
        [SerializeField] private TextMeshProUGUI _buttonIdText;

        // public int BlockId { get; private set; } = -1;

        public void SetId(int id)
        {
            // _buttonIdText.gameObject.SetActive(id >= 0);
            if (_buttonIdText != null)
            {
                _buttonIdText.text = $"{id}";
            }

            RemoveButton.gameObject.SetActive(id >= 0);
        }

        void Start()
        {
            RemoveButton.gameObject.SetActive(false);

            RemoveButton.onClick.AddListener(() =>
            {
                RemoveButton.gameObject.SetActive(false);
            });
        }
    }
}