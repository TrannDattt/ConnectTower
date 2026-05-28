using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class TopRankHolderVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text _rankText;
        [SerializeField] private Text _playerNameText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Image _playerIcon;

        public void SetData(string playerName, string score, Sprite playerIcon, string rankLabel = null)
        {
            SetPlayerName(playerName);
            SetScore(score);
            SetPlayerIcon(playerIcon);

            if (!string.IsNullOrEmpty(rankLabel))
            {
                SetRank(rankLabel);
            }
        }

        public void SetData(string playerName, int score, Sprite playerIcon, string rankLabel = null)
        {
            SetData(playerName, score.ToString(), playerIcon, rankLabel);
        }

        public void SetRank(string rankLabel)
        {
            if (_rankText != null)
            {
                _rankText.text = rankLabel;
            }
        }

        public void SetPlayerName(string playerName)
        {
            if (_playerNameText != null)
            {
                _playerNameText.text = playerName;
            }
        }

        public void SetScore(string score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = score;
            }
        }

        public void SetScore(int score)
        {
            SetScore(score.ToString());
        }

        public void SetPlayerIcon(Sprite icon)
        {
            if (_playerIcon != null && icon != null)
            {
                _playerIcon.sprite = icon;
                _playerIcon.preserveAspect = true;
            }
        }
    }
}
