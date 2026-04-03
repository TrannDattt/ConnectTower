using System;
using System.Linq;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class ShopBundleVisual : MonoBehaviour
    {
        [Serializable]
        public class BundleItemVisual
        {
            public Image Icon;
            public Text AmountText;

            public void SetVisual(Sprite icon, int amount)
            {
                Icon.sprite = icon;
                AmountText.text = amount.ToString();
            }
        }

        [SerializeField] private Text _name;
        [SerializeField] private BundleItemVisual _coin;
        [SerializeField] private BundleItemVisual _extraMove;
        [SerializeField] private BundleItemVisual _shuffle;
        [SerializeField] private BundleItemVisual _hint;
        [SerializeField] private BundleItemVisual _heart;
        [SerializeField] private Text _priceText;
        [SerializeField] private Text _currencyText;
        [SerializeField] private GameButtonVisual _purchaseButton;

        public void InitVisual(BundleSO bundleSO)
        {
            if (_name != null) _name.text = bundleSO.Name;
            //TODO: Map icon with reward
            var coinIcon = (Sprite)null;
            _coin?.SetVisual(coinIcon, bundleSO.Reward.CoinAmount);

            var extraMoveIcon = (Sprite)null;
            _extraMove?.SetVisual(extraMoveIcon, bundleSO.Reward.ExtraMoveAmount);

            var shuffleIcon = (Sprite)null;
            _shuffle?.SetVisual(shuffleIcon, bundleSO.Reward.ShuffleAmount);

            var hintIcon = (Sprite)null;
            _hint?.SetVisual(hintIcon, bundleSO.Reward.HintAmount);

            var heartIcon = (Sprite)null;
            _heart?.SetVisual(heartIcon, bundleSO.Reward.HeartAmount);
            
            _priceText.text = bundleSO.Price.ToString();
        }

        void Start()
        {
            _purchaseButton.OnClicked.AddListener(() =>
            {
                Debug.Log($"Process to purchase bundle {_name?.text}");
            });
        }
    }
}