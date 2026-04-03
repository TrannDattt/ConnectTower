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

        public override void Show()
        {
            base.Show();

            _homeButton.gameObject.SetActive(GameManager.Instance.CurState != Enums.EGameState.Menu);
        }

        protected override void Start()
        {
            _audioButton.OnClicked.AddListener(() => Debug.Log("Audio button clicked"));
            _vibrateButton.OnClicked.AddListener(() => Debug.Log("Vibrate button clicked"));
            _supportButton.OnClicked.AddListener(() => Debug.Log("Support button clicked"));
            _policyButton.OnClicked.AddListener(() => Debug.Log("Policy button clicked"));
            _homeButton.OnClicked.AddListener(() => 
            {
                //TODO: Popup warning

                if (GameManager.Instance.IsPlayTest)
                {
                    Hide();
                    GameSceneManager.Instance.ChangeScene(Enums.EGameScene.Editor);
                    return;
                }

                Debug.Log("Home button clicked");
                Hide();
                GameManager.Instance.GoToMenu();
            });

            base.Start();
        }
    }
}