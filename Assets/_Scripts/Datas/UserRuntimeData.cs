using System.Collections.Generic;
using Assets._Scripts.Enums;
using Newtonsoft.Json;

namespace Assets._Scripts.Datas
{
    public class UserRuntimeData
    {
        public string Id;
        public string Name;
        public string AvatarURL;
        // CURRENCY
        public int CoinCount;
        public int HeartCount;
        // PROGRESS
        public int CurrentLevelIndex;
        // BOOSTER
        [JsonProperty]
        public Dictionary<EBooster, int> BoosterCount = new();
        // TUTORIAL
        [JsonProperty]
        private HashSet<ETutorial> _playedTutorials = new();

        public bool HasPlayedTutorial(ETutorial tutorial) => _playedTutorials.Contains(tutorial);

        public void MarkTutorialPlayed(ETutorial tutorial) => _playedTutorials.Add(tutorial);

        // Constructor for new users
        public UserRuntimeData()
        {
            Id = "Admin_0";
            Name = "Admin";
            AvatarURL = "*";

            CoinCount = 1000;

            CurrentLevelIndex = 1;
            HeartCount = 5;

            BoosterCount[EBooster.ExtraMove] = 3;
            BoosterCount[EBooster.Shuffle] = 4;
            BoosterCount[EBooster.Hint] = 5;
            BoosterCount[EBooster.AddPillar] = 6;
        }

        // JsonConstructor for deserialization if needed, 
        // but default constructor works fine for public fields.
    }
}