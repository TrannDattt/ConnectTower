using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets._Scripts.Patterns.EventBus
{
    public static class EventBusUtils
    {
        public static IReadOnlyList<Type> EventTypes {get; set; }
        public static IReadOnlyList<Type> EventBusTypes {get; set; }

#if UNITY_EDITOR
        public static PlayModeStateChange PlayModeState { get; set; }

        [InitializeOnLoadMethod]
        public static void EditorInitialize()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            PlayModeState = state;
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                ClearAllBuses();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
            EventBusTypes = InitializeEventBusTypes();
        }

        private static List<Type> InitializeEventBusTypes()
        {
            var eventBusTypes = new List<Type>();
            foreach (var eventType in EventTypes)
            {
                var eventBusType = typeof(EventBus<>).MakeGenericType(eventType);
                eventBusTypes.Add(eventBusType);
                Debug.Log($"Initialized EventBus<{eventType.Name}>");
            }
            return eventBusTypes;
        }

        public static void ClearAllBuses()
        {
            Debug.Log("Clearing all Event Buses");
            foreach (var eventBusType in EventBusTypes)
            {
                var clearMethod = eventBusType.GetMethod("Clear", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                clearMethod?.Invoke(null, null);
            }
        }
    }
}
