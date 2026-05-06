using Assets._Scripts.Enums;
using Assets._Scripts.Patterns.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Tools.UI
{
    public class DifficultSelector : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _difficultyDropdown;

        void Start()
        {
            _difficultyDropdown.onValueChanged.AddListener((index) =>
            {
                var difficulty = (EDifficulty)index;
                LevelEditor.ChangeDifficulty(difficulty);
                EventBus<EditorDifficultyChanged>.Publish(new EditorDifficultyChanged{Difficulty = difficulty});
            });
        }
    }
}