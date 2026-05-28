using Assets._Scripts.Services.APIs;
using Assets._Scripts.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Scripts.Controllers
{
    public class RankingVisualControl : MonoBehaviour
    {
        private const int TopUserCount = 20;
        private const int TopThreeCount = 3;

        [Header("References")]
        [SerializeField] private TopRankHolderVisual _top1Holder;
        [SerializeField] private TopRankHolderVisual _top2Holder;
        [SerializeField] private TopRankHolderVisual _top3Holder;
        [SerializeField] private PlayerRankHolderVisual[] _otherRankHolders;

        private TopRankHolderVisual[] _topRankHolders;
        private int _loadVersion;

        private void Awake()
        {
            BuildTopHolderCache();
        }

        public void InitVisual()
        {
            BuildTopHolderCache();
            _loadVersion++;
            _ = InitVisualAsync(_loadVersion);
        }

        private async Task InitVisualAsync(int loadVersion)
        {
            Debug.Log("Init ranking visual");

            try
            {
                var rankedUsers = await UserAPI.GetTopUsersByCurrentLevelAsync(TopUserCount);
                if (loadVersion != _loadVersion)
                {
                    return;
                }

                ApplyRanking(rankedUsers);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load ranking data: {ex}");
            }
        }

        private void ApplyRanking(UserModel[] rankedUsers)
        {
            var topUsers = rankedUsers ?? Array.Empty<UserModel>();
            BindTopThree(topUsers);
            BindOtherRanks(topUsers.Skip(TopThreeCount).ToArray(), TopThreeCount + 1);
        }

        private void BindTopThree(IReadOnlyList<UserModel> users)
        {
            BuildTopHolderCache();

            for (int i = 0; i < _topRankHolders.Length; i++)
            {
                var holder = _topRankHolders[i];
                if (holder == null)
                {
                    continue;
                }

                bool hasUser = i < users.Count;
                holder.gameObject.SetActive(hasUser);
                if (!hasUser)
                {
                    continue;
                }

                var user = users[i];
                holder.SetData(
                    GetDisplayName(user, i + 1),
                    $"Level {Mathf.Max(1, user.CurrentLevelIndex)}",
                    null,
                    $"NO. {i + 1}");
            }
        }

        private void BindOtherRanks(IReadOnlyList<UserModel> users, int startRank)
        {
            if (_otherRankHolders == null)
            {
                return;
            }

            for (int i = 0; i < _otherRankHolders.Length; i++)
            {
                var holder = _otherRankHolders[i];
                if (holder == null)
                {
                    continue;
                }

                bool hasUser = i < users.Count;
                holder.gameObject.SetActive(hasUser);
                if (!hasUser)
                {
                    continue;
                }

                var user = users[i];
                holder.SetData(startRank + i, null, GetDisplayName(user, startRank + i), "Level", Mathf.Max(1, user.CurrentLevelIndex).ToString());
            }
        }

        private void BuildTopHolderCache()
        {
            _topRankHolders = new[]
            {
                _top1Holder,
                _top2Holder,
                _top3Holder
            };
        }

        private static string GetDisplayName(UserModel user, int fallbackRank)
        {
            if (!string.IsNullOrWhiteSpace(user.Name))
            {
                return user.Name;
            }

            if (!string.IsNullOrWhiteSpace(user.UID))
            {
                return user.UID;
            }

            return $"Player {fallbackRank}";
        }
    }
}
