using System.Collections;
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

        [SerializeField] private ParticleSystem _topConfetti1;
        [SerializeField] private ParticleSystem _topConfetti2;
        [SerializeField] private ParticleSystem _bottomConfetti1;
        [SerializeField] private ParticleSystem _bottomConfetti2;

        private LevelRuntimeData _curLevelData => LevelManager.PlayingLevel;

        public override IEnumerator Show()
        {
            var clearedState = _curLevelData.IsCleared;
            _continueButton.gameObject.SetActive(clearedState);
            _normalRewardText.text = _curLevelData.CoinReward.ToString();
            _normalRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardButton.gameObject.SetActive(!clearedState);
            _adsRewardText.text = (_curLevelData.CoinReward * 2).ToString();

            yield return base.Show();

            SoundManager.Instance.PlayRandomSFX(ESfx.Win);

            yield return PlayParticles();
        }

        private IEnumerator PlayParticles()
        {
            _topConfetti1.Play();
            _topConfetti2.Play();
            yield return new WaitForSeconds(.5f);
            _bottomConfetti1.Play();
            _bottomConfetti2.Play();
        }

        protected override void Start()
        {
            _continueButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Continue next level");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu();
            });
            _normalRewardButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Gained level reward");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu(() => UserManager.GainCoin(_curLevelData.CoinReward));
            });
            _adsRewardButton.OnClicked.AddListener(() => 
            {
                //TODO: Ads Service
                Debug.Log("Gained double reward via ads");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu(() => UserManager.GainCoin(_curLevelData.CoinReward * 2));
            });

            base.Start();
        }
    }
}