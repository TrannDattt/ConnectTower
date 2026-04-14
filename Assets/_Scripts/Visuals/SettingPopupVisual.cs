using System.Collections;
using Assets._Scripts.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._Scripts.Visuals
{
    public class SettingPopupVisual : GamePopupVisual
    {
        [SerializeField] private ToggleButtonVisual _audioButton;
        [SerializeField] private ToggleButtonVisual _vibrateButton;
        [SerializeField] private GameButtonVisual _supportButton;
        [SerializeField] private GameButtonVisual _policyButton;
        [SerializeField] private GameButtonVisual _homeButton;

        public override IEnumerator Show()
        {
            yield return base.Show();

            _homeButton.gameObject.SetActive(GameManager.Instance.CurState != Enums.EGameState.None);
        }

        protected override void Start()
        {
            _audioButton.OnToggled.AddListener((isActive) => 
            {
                SoundManager.Instance.gameObject.SetActive(isActive);
            });
            _vibrateButton.OnClicked.AddListener(() => Debug.Log("Vibrate button clicked"));
            _supportButton.OnClicked.AddListener(() => Debug.Log("Support button clicked"));
            _policyButton.OnClicked.AddListener(() => Debug.Log("Policy button clicked"));
            _homeButton.OnClicked.AddListener(() => 
            {
                //TODO: Popup warning

#if UNITY_EDITOR
                if (GameManager.Instance.IsPlayTest)
                {
                    StartCoroutine(Hide());
                    GameSceneManager.Instance.ChangeScene(Enums.EGameScene.Editor);
                    return;
                }
#endif

                Debug.Log("Home button clicked");
                StartCoroutine(Hide());
                GameManager.Instance.GoToMenu();
            });

            base.Start();
        }
    }
}