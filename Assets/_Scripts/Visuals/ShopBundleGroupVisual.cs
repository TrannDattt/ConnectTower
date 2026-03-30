using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class ShopBundleGroupVisual : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _groupName;
        [SerializeField] private RectTransform _bundleContainer;

        private List<ShopBundleVisual> _bundles = new();

        private void Init()
        {
            _bundles.Clear();
            
            //--------------
            // TODO: Load from Resource
            foreach (Transform t in _bundleContainer)
            {
                if (t.TryGetComponent(out ShopBundleVisual bundle))
                {
                    _bundles.Add(bundle);
                }
            }
            //--------------
            // TODO: Resize to fit with its content
        }
    }
}