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
            _audioButton.UpdateToggle(SoundManager.Instance.IsEnable, false);
            _vibrateButton.UpdateToggle(HapticManager.IsEnable, false);

            _audioButton.OnToggled.AddListener((isActive) => SoundManager.Instance.ChangeSoundVolume(isActive ? 1f : 0f));
            _vibrateButton.OnToggled.AddListener((isActive) => HapticManager.SetEnable(isActive));
            _supportButton.OnClicked.AddListener(() => Debug.Log("Support button clicked"));
            _policyButton.OnClicked.AddListener(() => Debug.Log("Policy button clicked"));
            _homeButton.OnClicked.AddListener(() => 
            {
#if UNITY_EDITOR
                if (GameManager.Instance.IsPlayTest)
                {
                    StartCoroutine(Hide());
                    GameSceneManager.Instance.ChangeScene(Enums.EGameScene.Editor);
                    return;
                }

#endif
                Debug.Log("Home button clicked");
                StartCoroutine(PopupManager.Instance.ShowConfirmPopup("Are you sure to go to Main menu?\n You will lose a heart.",
                                                                      "Home",
                                                                      () =>
                                                                      {
                                                                          StartCoroutine(Hide());
                                                                          UserManager.LostHeart();
                                                                          GameManager.Instance.GoToMenu();
                                                                      },
                                                                      "Cancel",
                                                                      null));
                
            });

            base.Start();
        }
    }
}