using Assets._Scripts.Datas;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class RevivePopupVisual : BundlePurchasePopupVisual
    {
        protected override void Start()
        {
            _closeButton.OnClicked.AddListener(GameManager.Instance.FailedLevel);
            //TODO: Check if purchase succcessful
            _buyButton.OnClicked.AddListener(() => 
            {
                GameManager.Instance.ChangeMoveCount(5);
                GameManager.Instance.ContinuePlaying();
            });

            base.Start();
        }
    }
}