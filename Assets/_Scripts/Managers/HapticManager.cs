using CandyCoded.HapticFeedback;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    public static class HapticManager
    {
        public static bool IsEnable {get; private set;} = true;
        public static int VibrationLevel {get; private set;} = 1;

        public static void SetEnable(bool state) => IsEnable = state;

        public static void SetVibrationLevel(float level) => VibrationLevel = (int)Mathf.Clamp(level, 0f, 4f);

        public static void DoFeedBack()
        {
            if (!IsEnable) return;
            switch (VibrationLevel)
            {
                case 0:
                    DoLightFeedback();
                    break;
                case 1:
                    DoMediumFeedback();
                    break;
                case 2:
                    DoHeavyFeedback();
                    break;
            }
        }

        public static void DoLightFeedback()
        {
            if (!IsEnable) return;
            HapticFeedback.LightFeedback();
        }

        public static void DoMediumFeedback()
        {
            if (!IsEnable) return;
            HapticFeedback.MediumFeedback();
        }

        public static void DoHeavyFeedback()
        {
            if (!IsEnable) return;
            HapticFeedback.HeavyFeedback();
        }
    }
}