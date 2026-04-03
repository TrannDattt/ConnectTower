using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class ProgressBarVisual  : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Slider _slider;
        [SerializeField] private Text _progressText;

        public void UpdateProgress(int current, int target)
        {
            if (_slider != null)
            {
                DOTween.To(() => _slider.value, x => _slider.value = x, (float)current / target, 0.5f).SetEase(Ease.InOutSine);
            }

            if (_progressText != null)
            {
                _progressText.text = $"{current}/{target}";
            }
        }
    }
}