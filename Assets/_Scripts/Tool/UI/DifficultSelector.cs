using Assets._Scripts.Enums;
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
                LevelEditor.ChangeDifficulty((EDifficulty)index);
            });
        }
    }
}