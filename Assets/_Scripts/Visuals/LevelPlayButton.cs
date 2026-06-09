using UnityEngine;
using UnityEngine.UI;
using Assets._Scripts.Managers;
using TMPro; // Đổi sang TMPro để hỗ trợ nảy từng ký tự

namespace Assets._Scripts.Visuals
{
    public class LevelPlayButton : GameButtonVisual
    {
        [SerializeField] private TextMeshProUGUI _levelText;

        public void UpdateVisual()
        {
            var progress = UserManager.CurUser.CurrentLevelIndex;
            _levelText.SetText($"Level {progress}");
        }
    }
}
