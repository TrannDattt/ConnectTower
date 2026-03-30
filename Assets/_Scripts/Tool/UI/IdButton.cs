using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class IdButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private Button _removeButton;

        private int _id;

        public UnityEvent<int> OnRemoveClicked { get; private set; } = new();

        public void SetId(int id)
        {
            _id = id;
            if (_idText != null)
            {
                _idText.text = id.ToString();
            }   
        }

        void Start()
        {
            _removeButton.onClick.AddListener(() =>
            {
                OnRemoveClicked.Invoke(_id);
                Destroy(gameObject);
            });
        }

        void OnDestroy()
        {
            OnRemoveClicked.RemoveAllListeners();
            _removeButton.onClick.RemoveAllListeners();
        }
    }
}