using System.Collections;
using Assets._Scripts.Datas;
using Assets._Scripts.Managers;
using Assets._Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class BundlePurchasePopupVisual : GamePopupVisual
    {
        [SerializeField] protected Text _bundleNameText;
        [SerializeField] protected Image _bundleIcon;
        [SerializeField] protected Text _bundleCountText;
        [SerializeField] protected Text _bundleDetailText;
        [SerializeField] protected Text _bundlePriceText;
        [SerializeField] protected GameButtonVisual _buyButton;

        private BundleSO _thisBundle;

        public virtual IEnumerator ShowBundle(BundleSO bundle)
        {
            if (bundle == null) yield break;
            yield return Show();
            InitVisual(bundle);
            Debug.Log($"Show bundle {_thisBundle.Name}");
        }

        protected virtual void InitVisual(BundleSO bundle)
        {
            _thisBundle = bundle;
            _bundleNameText.text = _thisBundle.Name;
            _bundleIcon.sprite = _thisBundle.Icon;
            _bundleDetailText.text = _thisBundle.Detail;
            _bundlePriceText.text = _thisBundle.Price.ToString();
        }

        protected override void Start()
        {
            base.Start();

            _buyButton.OnClicked.AddListener(() => 
            {
                if (!PurchaseService.TryPurchaseBundle(_thisBundle))
                {
                    PopupManager.Instance.ShowPopupText("Purchase failed", _buyButton.transform.position);
                    return;
                }
                StartCoroutine(Hide());
                Debug.Log($"Purchased bundle {_bundleNameText.text}");
            });
        }

        protected override void OnDestroy()
        {
            _buyButton.OnClicked.RemoveAllListeners();

            base.OnDestroy();
        }
    }
}