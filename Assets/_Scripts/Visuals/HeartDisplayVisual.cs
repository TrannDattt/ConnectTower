using Assets._Scripts.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class HeartDisplayVisual : MonoBehaviour
    {
        [SerializeField] private GameButtonVisual _buyHeartButton;
        [SerializeField] private Text _heartRecoverTimeText;

        public void UpdateVisual()
        {
            //TODO: Update heart count
        }
    }
}