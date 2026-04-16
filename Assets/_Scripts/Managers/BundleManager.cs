using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Patterns;
using UnityEngine;

namespace Assets._Scripts.Managers
{
    //TODO: Change to a mapper class
    public class BundleManager : Singleton<BundleManager>
    {
        [SerializeField] private BundleSO _noAdsBundle;
        [SerializeField] private BundleSO _getLifeBundle;
        [SerializeField] private BundleSO _ingameExtraMoveBundle;
        [SerializeField] private BundleSO _ingameShuffleBundle;
        [SerializeField] private BundleSO _ingameHintBundle;
        [SerializeField] private BundleSO _reviveBundle;

        public BundleSO GetIngameBoosterBundle(EBooster type)
        {
            return type switch
            {
                EBooster.ExtraMove => _ingameExtraMoveBundle,
                EBooster.Shuffle => _ingameShuffleBundle,
                EBooster.Hint => _ingameHintBundle,
                _ => null
            };
        }

        public BundleSO GetReviveBundle() => _reviveBundle;
        public BundleSO GetNoAdsBundle() => _noAdsBundle;
        public BundleSO GetLifeBundle() => _getLifeBundle;
    }
}