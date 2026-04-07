using UnityEngine;
using UnityEngine.UI;
using Assets._Scripts.Managers; // Đổi sang TMPro để hỗ trợ nảy từng ký tự

namespace Assets._Scripts.Visuals
{
    public class LevelPlayButton : GameButtonVisual
    {
        [SerializeField] private Text _levelText;

        public void UpdateVisual()
        {
            var progress = UserManager.CurUser.CurrentLevelIndex;
            _levelText.text = $"Level {progress}";
        }
    }
}
