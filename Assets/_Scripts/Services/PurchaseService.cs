using Assets._Scripts.Datas;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Services
{
    public static class PurchaseService
    {
        private static bool _isLoading;

        public static bool TryPurchaseBundle(BundleSO bundle)
        {
            _isLoading = true;
            //TODO: Check if this bundle is paid with coin
            if (!UserManager.TryLoseCoin((int)bundle.Price))
            {
                Debug.Log($"You dont have enough coin to purchase bundle {bundle.Name}");
                _isLoading = false;
                return false;
            }
            else
            {
                Debug.Log($"Purchased bundle {bundle.Name} successfully");
                _isLoading = false;
                UserManager.GetBundle(bundle);
                return true;
            }
        }
    }
}