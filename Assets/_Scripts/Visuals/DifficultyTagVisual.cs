using Assets._Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class DifficultyTagVisual : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite _normalBackground;
        [SerializeField] private Sprite _hardBackground;
        [SerializeField] private Sprite _superHardBackground;

        [SerializeField] private Image _difficultyTag;
        [SerializeField] private Sprite _hardTag;
        [SerializeField] private Sprite _superHardTag;

        [SerializeField] private TextMeshProUGUI _difficultyText;

        public void SetDifficulty(EDifficulty difficulty)
        {
            switch (difficulty)
            {
                case EDifficulty.Normal:
                    _backgroundImage.sprite = _normalBackground;
                    _difficultyTag.sprite = null;
                    _difficultyTag.gameObject.SetActive(false);
                    _difficultyText.gameObject.SetActive(false);
                    break;
                case EDifficulty.Hard:
                    _backgroundImage.sprite = _hardBackground;
                    _difficultyTag.sprite = _hardTag;
                    _difficultyTag.gameObject.SetActive(true);
                    _difficultyText.gameObject.SetActive(true);
                    _difficultyText.text = "Hard";
                    break;
                case EDifficulty.SuperHard:
                    _backgroundImage.sprite = _superHardBackground;
                    _difficultyTag.sprite = _superHardTag;
                    _difficultyTag.gameObject.SetActive(true);
                    _difficultyText.gameObject.SetActive(true);
                    _difficultyText.text = "Super Hard";
                    break;
            }
        }
    }
}