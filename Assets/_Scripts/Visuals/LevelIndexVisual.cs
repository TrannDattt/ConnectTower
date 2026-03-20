using TMPro;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class LevelIndexVisual : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelIndexText;

        public void SetLevelIndex(int index)
        {
            if (_levelIndexText != null)
            {
                _levelIndexText.text = $"{index}";
            }
        }
    }
}