using System;
using Assets._Scripts.Enums;
using UnityEngine;

namespace Assets._Scripts.Datas
{
    [CreateAssetMenu(fileName = "BundleData", menuName = "ScriptableObjects/BundleData", order = 2)]
    public class BundleSO : ScriptableObject
    {
        public int Id;
        public string Name;
        public Sprite Icon;
        public BundleReward Reward;
        public string Detail;
        public float Price;
        public bool UseOnBought;
    }

    [Serializable]
    public struct BundleReward
    {
        public int CoinAmount; 
        public int HeartAmount; 
        public int ExtraMoveAmount;
        public int ShuffleAmount;
        public int HintAmount;
    }

    [Serializable]
    public struct BoosterReward
    {
        public EBooster Type;
        public int Amount;
    }
}
