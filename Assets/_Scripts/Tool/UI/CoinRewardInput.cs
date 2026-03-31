using TMPro;
using UnityEngine;

namespace Assets._Scripts.Tools.UI
{
    public class CoinRewardInput : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _coinRewardInput;

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
        }
    }
}