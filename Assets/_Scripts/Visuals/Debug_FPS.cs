using TMPro;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class Debug_FPS : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _fps;

        private float _elapsedTime;
        private int _frameCount;

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

            _elapsedTime += Time.unscaledDeltaTime;
            _frameCount++;

            if (_elapsedTime < 1f)
            {
                return;
            }

            int currentFps = Mathf.RoundToInt(_frameCount / _elapsedTime);
            _fps.SetText($"FPS: {currentFps}");
            _fps.color = GetFpsColor(currentFps);

            _elapsedTime = 0f;
            _frameCount = 0;
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
