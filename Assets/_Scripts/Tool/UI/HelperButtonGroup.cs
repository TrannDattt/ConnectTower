using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class HelperButtonGroup : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _playTestButton;

        void Start()
        {
            _saveButton.onClick.AddListener(() =>
            {
                LevelEditor.SaveLevel();
            });

            _loadButton.onClick.AddListener(() =>
            {
                LevelEditor.LoadLevel();
            });

            _clearButton.onClick.AddListener(() =>
            {
                LevelEditor.ClearLevel();
            });

            _playTestButton.onClick.AddListener(() =>
            {
                LevelEditor.TestCurrentLevel();
            });
        }
    }
}