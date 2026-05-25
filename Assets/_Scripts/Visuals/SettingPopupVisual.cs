using System.Collections;
using Assets._Scripts.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets._Scripts.Visuals
{
    public class SettingPopupVisual : GamePopupVisual
    {
        // [SerializeField] private ToggleButtonVisual _audioButton;
        // [SerializeField] private ToggleButtonVisual _vibrateButton;
        [SerializeField] private AudioSliderButton _bgmSlider;
        [SerializeField] private AudioSliderButton _sfxSlider;
        [SerializeField] private AudioSliderButton _hapticSlider;
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
            _bgmSlider.UpdateToggle(SoundManager.Instance.IsEnable(true), false);
            _sfxSlider.UpdateToggle(SoundManager.Instance.IsEnable(false), false);
            _hapticSlider.UpdateToggle(HapticManager.IsEnable, false);

            _bgmSlider.UpdateSlider(SoundManager.Instance.BgmVolume, SoundManager.Instance.IsEnable(true));
            _sfxSlider.UpdateSlider(SoundManager.Instance.SfxVolume, SoundManager.Instance.IsEnable(false));
            _hapticSlider.UpdateSlider(HapticManager.VibrationLevel);

            _bgmSlider.OnToggled.AddListener((isActive) => SoundManager.Instance.SetEnable(true, isActive));
            _sfxSlider.OnToggled.AddListener((isActive) => SoundManager.Instance.SetEnable(false, isActive));
            _hapticSlider.OnToggled.AddListener((isActive) => HapticManager.SetEnable(isActive));
            
            _bgmSlider.OnValueChanged.AddListener((value) => SoundManager.Instance.ChangeBgmVolume(value));
            _sfxSlider.OnValueChanged.AddListener((value) => SoundManager.Instance.ChangeSfxVolume(value));
            _hapticSlider.OnValueChanged.AddListener((value) => HapticManager.SetVibrationLevel(value));

            _supportButton?.OnClicked.AddListener(() => Debug.Log("Support button clicked"));
            _policyButton?.OnClicked.AddListener(() => Debug.Log("Policy button clicked"));
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
                PopupManager.Instance.StartCoroutine(Hide());
                PopupManager.Instance.StartCoroutine(PopupManager.Instance.ShowConfirmPopup("Are you sure to go to Main menu?\n You will lose a heart.",
                                                                      "Home",
                                                                      () =>
                                                                      {
                                                                          PopupManager.Instance.StartCoroutine(Hide());
                                                                          UserManager.LostHeart();
                                                                          GameManager.Instance.GoToMenu();
                                                                      },
                                                                      "Cancel",
                                                                      () =>
                                                                      {
                                                                          PopupManager.Instance.StartCoroutine(Show());
                                                                      }));
                
            });

            base.Start();
        }
    }
}