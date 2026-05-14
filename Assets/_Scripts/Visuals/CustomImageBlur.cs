using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    [RequireComponent(typeof(Image))]
    public class CustomImageBlur : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _factor;

        private UIEffect _uiEffect;

        private void Awake()
        {
            EnsureEffect();
            ApplyBlur();
        }

        private void OnEnable()
        {
            EnsureEffect();
            ApplyBlur();
        }

        private void EnsureEffect()
        {
            if (_uiEffect == null && !TryGetComponent(out _uiEffect))
            {
                _uiEffect = gameObject.AddComponent<UIEffect>();
            }
        }

        private void ApplyBlur()
        {
            EnsureEffect();
            _factor = Mathf.Clamp01(_factor);

            _uiEffect.samplingIntensity = _factor;
            _uiEffect.samplingFilter = _factor > 0f
                ? SamplingFilter.BlurMedium
                : SamplingFilter.None;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureEffect();
            ApplyBlur();
        }
#endif
    }
}
