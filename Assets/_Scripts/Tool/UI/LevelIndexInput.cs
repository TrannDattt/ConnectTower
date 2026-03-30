using TMPro;
using UnityEngine;

namespace Assets._Scripts.Tools.UI
{
    public class LevelIndexInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _levelIndexInput;

        void Start()
        {
            _levelIndexInput.onEndEdit.AddListener((text) =>
            {
                if (int.TryParse(text.Trim(), out int levelIndex))
                {
                    LevelEditor.ChangeLevelIndex(levelIndex);
                }
                else
                {
                    Debug.LogWarning("Invalid level index input.");
                }
            });
        }
    }
}