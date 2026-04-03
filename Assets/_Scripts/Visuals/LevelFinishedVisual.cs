using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class LevelFinishedVisual : GamePopupVisual
    {
        [SerializeField] private GameButtonVisual _continueButton;
        [SerializeField] private GameButtonVisual _normalRewardButton;
        [SerializeField] private Text _normalRewardText;
        [SerializeField] private GameButtonVisual _adsRewardButton;
        [SerializeField] private Text _adsRewardText;

        private LevelRuntimeData _curLevelData;

        public override void Show()
        {
            base.Show();

            var clearedState = _curLevelData.IsCleared;
            _continueButton.gameObject.SetActive(clearedState);
            _normalRewardText.text = _curLevelData.CoinReward.ToString();
            _normalRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardText.text = (_curLevelData.CoinReward * 2).ToString();
        }
        
        public void SetData(LevelRuntimeData levelData)
        {
            _curLevelData = new(levelData);
        }

        protected override void Start()
        {
            _continueButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Continue next level");
                Hide();
                GameManager.Instance.GoToMenu();
            });
            _normalRewardButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Gained level reward");
                Hide();
                GameManager.Instance.GoToMenu();
                UserManager.GainCoin(_curLevelData.CoinReward);
            });
            _adsRewardButton.OnClicked.AddListener(() => 
            {
                //TODO: Ads Service
                Debug.Log("Gained double reward via ads");
                Hide();
                GameManager.Instance.GoToMenu();
                UserManager.GainCoin(_curLevelData.CoinReward * 2);
            });

            base.Start();
        }
    }
}