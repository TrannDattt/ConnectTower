using TMPro;
using UnityEngine;

namespace Assets._Scripts.Tools.UI
{
    public class MoveLimitInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _moveLimitInput;

        void Start()
        {
            _moveLimitInput.onEndEdit.AddListener((text) =>
            {
                if (int.TryParse(text.Trim(), out int moveLimit))
                {
                    LevelEditor.ChangeMoveLimit(moveLimit);
                }
                else
                {
                    Debug.LogWarning("Invalid move limit input.");
                }
            });

            LevelEditor.OnLevelCleared.AddListener(() =>
            {
                _moveLimitInput.text = "";
            });

            LevelEditor.OnLevelLoaded.AddListener((json) =>
            {
                _moveLimitInput.text = json.MoveLimit.ToString();
            });
        }
    }
}