using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class RevivePopupVisual : BundlePurchasePopupVisual
    {
        public override void Show()
        {
            Debug.Log($"Show {name}");
            // _popupPanel.SetActive(true);
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
            // _popupPanel.SetActive(false);
        }

        protected override void Start()
        {
            _closeButton.OnClicked.AddListener(GameManager.Instance.FailedLevel);
            //TODO: Check if purchase succcessful
            _buyButton.OnClicked.AddListener(GameManager.Instance.ContinuePlaying);

            base.Start();
        }
    }
}