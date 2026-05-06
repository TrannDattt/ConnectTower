using Assets._Scripts.Enums;
using Assets._Scripts.Patterns.EventBus;
using TMPro;
using UnityEngine;

namespace Assets._Scripts.Tools.UI
{
    public class CoinRewardInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _coinRewardInput;
        private EventBinding<EditorDifficultyChanged> _difficultyChanged;

        private void OnDifficultyChanged(EditorDifficultyChanged @event)
        {
            var coinReward =  @event.Difficulty switch
            {
                EDifficulty.Normal => 20,
                EDifficulty.Hard => 60,
                EDifficulty.SuperHard => 100,
                _ => 0
            };

            _coinRewardInput.text = coinReward.ToString();
            LevelEditor.ChangeCoinReward(coinReward);
        }

        void Start()
        {
            _coinRewardInput.onEndEdit.AddListener((text) =>
            {
                if (int.TryParse(text.Trim(), out int coinReward))
                {
                    LevelEditor.ChangeCoinReward(coinReward);
                }
                else
                {
                    Debug.LogWarning("Invalid coin reward input.");
                }
            });

            LevelEditor.OnLevelCleared.AddListener(() =>
            {
                _coinRewardInput.text = "";
            });

            LevelEditor.OnLevelLoaded.AddListener((json) =>
            {
                _coinRewardInput.text = json.CoinReward.ToString();
            });

            _difficultyChanged = new(OnDifficultyChanged);
            EventBus<EditorDifficultyChanged>.Subscribe(_difficultyChanged);
        }

        void OnDestroy()
        {
            EventBus<EditorDifficultyChanged>.Unsubscribe(_difficultyChanged);
        }
    }

    public struct EditorDifficultyChanged : IEvent
    {
        public EDifficulty Difficulty;
    }
}