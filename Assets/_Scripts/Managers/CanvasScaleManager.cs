using System.Collections.Generic;
using Assets._Scripts.Controllers;
using Assets._Scripts.Patterns.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Managers
{
    public static class CanvasScaleManager
    {
        private static CanvasScaler[] _canvasScalers;
        private static EventBinding<ScreenRatioChangeEvent> _screenRatioChangeBinding;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            _canvasScalers = Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            _screenRatioChangeBinding = new(UpdateScalers);
            EventBus<ScreenRatioChangeEvent>.Subscribe(_screenRatioChangeBinding);
            UpdateScalers();
        }

        private static void UpdateScalers()
        {
            float screenRatio = (float)Screen.height / Screen.width;
            foreach (var scaler in _canvasScalers)
            {
                scaler.matchWidthOrHeight = screenRatio < 1.5f ? 1f : 0f;
            }
        }
    }
}