using Assets._Scripts.Visuals;
using TMPro;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class HeartDisplayVisual : MonoBehaviour
    {
        [SerializeField] private GameButtonVisual _buyHeartButton;
        [SerializeField] private TextMeshProUGUI _heartRecoverTimeText;

        public void UpdateVisual()
        {
            //TODO: Update heart count
        }
    }
}