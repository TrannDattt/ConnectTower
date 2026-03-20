using UnityEngine;

namespace Assets._Scripts.Visuals
{
    public class SettingPopupVisual : GamePopupVisual
    {
        [SerializeField] private GameButtonVisual _audioButton;
        [SerializeField] private GameButtonVisual _vibrateButton;
        [SerializeField] private GameButtonVisual _supportButton;
        [SerializeField] private GameButtonVisual _policyButton;
        [SerializeField] private GameButtonVisual _homeButton;

        public override void Show()
        {
            base.Show();

            //TODO: Hide home button if it's not in main menu
        }

        protected override void Start()
        {
            _audioButton.OnClicked.AddListener(() => Debug.Log("Audio button clicked"));
            _vibrateButton.OnClicked.AddListener(() => Debug.Log("Vibrate button clicked"));
            _supportButton.OnClicked.AddListener(() => Debug.Log("Support button clicked"));
            _policyButton.OnClicked.AddListener(() => Debug.Log("Policy button clicked"));
            _homeButton.OnClicked.AddListener(() => Debug.Log("Home button clicked"));

            base.Start();
        }
    }
}