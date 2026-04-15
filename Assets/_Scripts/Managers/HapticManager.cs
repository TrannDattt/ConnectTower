using CandyCoded.HapticFeedback;

namespace Assets._Scripts.Managers
{
    public static class HapticManager
    {
        public static bool IsEnable {get; private set;} = true;

        public static void SetEnable(bool state) => IsEnable = state;

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