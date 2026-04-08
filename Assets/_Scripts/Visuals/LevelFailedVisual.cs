using System.Collections;
using Assets._Scripts.Controllers;
using Assets._Scripts.Datas;
using Assets._Scripts.Enums;
using Assets._Scripts.Managers;
using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class LevelFailedVisual : GamePopupVisual
    {
        [SerializeField] private GameButtonVisual _retryButton;
        [SerializeField] private GameButtonVisual _homeButton;

        private LevelRuntimeData _curLevelData => LevelManager.PlayingLevel;

        public override IEnumerator Show()
        {
            SoundManager.Instance.PlayRandomSFX(ESfx.Lose);
            return base.Show();
        }

        protected override void Start()
        {
            _retryButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Retry level");
                StartCoroutine(Hide());
                LevelRuntimeData toRestart = new(_curLevelData);
                GameManager.Instance.StartLevel(toRestart);
            });
            _homeButton.OnClicked.AddListener(() => 
            {
                Debug.Log("Go to main menu");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu();
            });

            base.Start();
        }
    }
}