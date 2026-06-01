using TMPro;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class Debug_FPS : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _fps;

        void Update()
        {
            UpdateFpsDisplay();
        }

        private void UpdateFpsDisplay()
        {
            if (_fps == null || Time.unscaledDeltaTime <= 0f)
            {
                return;
            }

            int currentFps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            _fps.SetText($"FPS: {currentFps}");
            _fps.color = GetFpsColor(currentFps);
        }

        private static Color GetFpsColor(int fps)
        {
            if (fps >= 58)
            {
                return Color.green;
            }

            if (fps >= 30)
            {
                return Color.yellow;
            }

            return Color.red;
        }
    }
}
