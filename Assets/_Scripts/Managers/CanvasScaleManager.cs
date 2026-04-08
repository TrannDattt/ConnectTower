using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Managers
{
    public static class CanvasScaleManager
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            var canvasScalers = Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            // Debug.Log($"Found {canvasScalers.Length} canvas scalers in scene");
            float screenRatio = (float)Screen.height / Screen.width;
            // Debug.Log($"Screen ratio is {Screen.height}/{Screen.width}");

            foreach (var scaler in canvasScalers)
            {
                scaler.matchWidthOrHeight = screenRatio < 1.5f ? 1f : 0f;
            }
        }
    }
}