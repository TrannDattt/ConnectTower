using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class LevelIndexVisual : MonoBehaviour
    {
        [SerializeField] private Text _levelIndexText;

        public void SetLevelIndex(int index)
        {
            if (_levelIndexText != null)
            {
                _levelIndexText.text = $"Level {index}";
            }
        }
    }
}