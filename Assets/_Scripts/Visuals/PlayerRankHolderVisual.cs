using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Visuals
{
    public class PlayerRankHolderVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text _rankText;
        [SerializeField] private Image _avatarImage;
        [SerializeField] private Text _playerNameText;
        [SerializeField] private Text _statTitleText;
        [SerializeField] private Text _statValueText;

        public void SetData(int rank, Sprite avatar, string playerName, string statTitle, string statValue)
        {
            SetRank(rank);
            SetAvatar(avatar);
            SetPlayerName(playerName);
            SetStat(statTitle, statValue);
        }

        public void SetData(string rank, Sprite avatar, string playerName, string statTitle, string statValue)
        {
            SetRank(rank);
            SetAvatar(avatar);
            SetPlayerName(playerName);
            SetStat(statTitle, statValue);
        }

        public void SetRank(int rank)
        {
            SetRank(rank.ToString());
        }

        public void SetRank(string rank)
        {
            if (_rankText != null)
            {
                _rankText.text = rank;
            }
        }

        public void SetAvatar(Sprite avatar)
        {
            if (_avatarImage != null && avatar != null)
            {
                _avatarImage.sprite = avatar;
                _avatarImage.preserveAspect = true;
            }
        }

        public void SetPlayerName(string playerName)
        {
            if (_playerNameText != null)
            {
                _playerNameText.text = playerName;
            }
        }

        public void SetStat(string title, string value)
        {
            if (_statTitleText != null)
            {
                _statTitleText.text = title;
            }

            if (_statValueText != null)
            {
                _statValueText.text = value;
            }
        }

        public void SetLevel(int level)
        {
            SetStat("Level", level.ToString());
        }

        public void SetScore(int score)
        {
            SetStat("Score", score.ToString());
        }

        public void SetScore(string score)
        {
            SetStat("Score", score);
        }
    }
}
